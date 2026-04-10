using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using LibreHardwareMonitor.Hardware;

namespace MoRoC.Classes
{
    public class CPU
    {
        private Computer _computer => HardwareMonitorService.Instance.Computer;

        public string Name { get; private set; }
        public string Temperature { get; private set; }
        public float TemperatureValue { get; private set; }
        public List<string> Fans { get; private set; }
        public int LogicalCores { get; }
        public int PhysicalCores { get; }
        public int MaxClockSpeed { get; }
        public string PowerUsage { get; private set; }
        public string CpuUsage { get; private set; }
        public List<string> ClockSpeeds { get; private set; }
        public string TotalClockSpeed { get; private set; }
        public double TotalClockSpeedGHz { get; private set; }
        public float CoreLoad { get; private set; }

        public CPU()
        {
            LogicalCores = GetLogicalCores();
            PhysicalCores = GetPhysicalCores();
            MaxClockSpeed = GetMaxClockSpeed();
            Fans = new List<string>();
            ClockSpeeds = new List<string>();

            UpdateName();
            Refresh();
        }

        // ── Helpers ──────────────────────────────────────────────

        private IHardware GetCpuHardware() =>
            _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);

        private ISensor FindCpuSensor(SensorType type, string nameContains = null)
        {
            var cpu = GetCpuHardware();
            if (cpu == null) return null;
            return cpu.Sensors.FirstOrDefault(s =>
                s.SensorType == type &&
                (nameContains == null || s.Name.Contains(nameContains)));
        }

        // ── Static data (called once) ───────────────────────────

        private void UpdateName()
        {
            Name = string.Empty;
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                Name = obj["Name"]?.ToString() ?? "Unknown CPU";
            }
        }

        // ── Dynamic data (called every tick) ────────────────────

        public void Refresh()
        {
            UpdateTemperature();
            UpdatePowerUsage();
            UpdateCpuUsage();
            UpdateFanSpeed();
            UpdateClockSpeeds();
            UpdateTotalClockSpeed();
            UpdateLoad();
        }

        private void UpdateTemperature()
        {
            var cpu = GetCpuHardware();
            if (cpu == null) { Temperature = "N/A"; TemperatureValue = 0; return; }

            // Prefer Package / Tdie / Average sensor, fall back to any temperature sensor
            var preferredSensor = cpu.Sensors.FirstOrDefault(s =>
                s.SensorType == SensorType.Temperature &&
                (s.Name.Contains("Package") || s.Name.Contains("Tdie") || s.Name.Contains("Average")));

            var sensor = preferredSensor
                         ?? cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);

            TemperatureValue = sensor?.Value ?? 0;
            Temperature = sensor?.Value.HasValue == true ? $"{sensor.Value.Value:F1} °C" : "N/A";
        }

        private void UpdatePowerUsage()
        {
            var sensor = FindCpuSensor(SensorType.Power, "CPU Package");
            PowerUsage = sensor?.Value.HasValue == true ? $"{sensor.Value.Value:F1} W" : "N/A";
        }

        private void UpdateCpuUsage()
        {
            var sensor = FindCpuSensor(SensorType.Load, "CPU Total");
            CpuUsage = sensor?.Value.HasValue == true ? $"{sensor.Value.Value:F1} %" : "N/A";
        }

        private void UpdateFanSpeed()
        {
            Fans.Clear();

            // Fan sensors may be under any hardware type (often Motherboard SuperIO),
            // so we collect from all hardware recursively.
            var allFanSensors = new List<ISensor>();
            foreach (var hardware in _computer.Hardware)
            {
                CollectSensorsRecursive(hardware, allFanSensors);
            }

            Fans = allFanSensors
                .Where(s => s.SensorType == SensorType.Fan)
                .OrderBy(s => s.Name)
                .Take(2)
                .Select(s => $"{s.Value?.ToString("F2") ?? "N/A"} RPM")
                .ToList();
        }

        private static void CollectSensorsRecursive(IHardware hardware, List<ISensor> sensors)
        {
            sensors.AddRange(hardware.Sensors);
            foreach (var subHardware in hardware.SubHardware)
            {
                CollectSensorsRecursive(subHardware, sensors);
            }
        }

        private void UpdateClockSpeeds()
        {
            ClockSpeeds.Clear();
            var cpu = GetCpuHardware();
            if (cpu == null) return;

            var speeds = cpu.Sensors
                .Where(s => s.SensorType == SensorType.Clock && s.Value.HasValue)
                .Select(s => $"{s.Name}: {s.Value.Value} MHz");
            ClockSpeeds.AddRange(speeds);
        }

        private void UpdateTotalClockSpeed()
        {
            var cpu = GetCpuHardware();
            if (cpu == null) { TotalClockSpeed = "0.00 GHz"; TotalClockSpeedGHz = 0; return; }

            // Materialize once to avoid double enumeration
            var coreSpeeds = cpu.Sensors
                .Where(s => s.SensorType == SensorType.Clock
                            && s.Value.HasValue
                            && s.Name != "Bus Speed")
                .Select(s => s.Value!.Value)
                .ToList();

            double averageSpeed = coreSpeeds.Count > 0 ? coreSpeeds.Average() : 0;
            TotalClockSpeedGHz = averageSpeed / 1000;
            TotalClockSpeed = $"{TotalClockSpeedGHz:F2} GHz";
        }

        private void UpdateLoad()
        {
            var sensor = FindCpuSensor(SensorType.Load, "CPU Total");
            CoreLoad = sensor?.Value ?? 0;
        }

        // ── WMI static helpers ──────────────────────────────────

        private static int GetLogicalCores()
        {
            try { return Environment.ProcessorCount; }
            catch (Exception ex) { Debug.WriteLine($"GetLogicalCores failed: {ex.Message}"); return -1; }
        }

        private static int GetPhysicalCores()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT NumberOfCores FROM Win32_Processor");
                return searcher.Get().Cast<ManagementObject>()
                    .Sum(item => int.Parse(item["NumberOfCores"].ToString()));
            }
            catch (Exception ex) { Debug.WriteLine($"GetPhysicalCores failed: {ex.Message}"); return -1; }
        }

        private static int GetMaxClockSpeed()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT MaxClockSpeed FROM Win32_Processor");
                return searcher.Get().Cast<ManagementObject>()
                    .Sum(item => int.Parse(item["MaxClockSpeed"].ToString()));
            }
            catch (Exception ex) { Debug.WriteLine($"GetMaxClockSpeed failed: {ex.Message}"); return -1; }
        }
    }
}