using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using LibreHardwareMonitor.Hardware;

namespace MoRoC.Classes
{
    public class GPU
    {
        private Computer _computer => HardwareMonitorService.Instance.Computer;

        public string Name { get; private set; }
        public string BoardManufacturer { get; private set; }
        public string Temperature { get; private set; }
        public float TemperatureValue { get; private set; }
        public float CoreClockSpeed { get; private set; }
        public float MemoryClockSpeed { get; private set; }
        public float PowerLoad { get; private set; }
        public float CoreLoad { get; private set; }
        public List<string> Fans { get; private set; } = new List<string>();

        // PCI Subsystem Vendor ID → board manufacturer name (static, allocated once)
        private static readonly Dictionary<string, string> VendorMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "1043", "ASUS" },
                { "1462", "MSI" },
                { "1458", "Gigabyte" },
                { "3842", "EVGA" },
                { "19DA", "Zotac" },
                { "1569", "Palit" },
                { "196E", "PNY" },
                { "1DA2", "Sapphire" },
                { "174B", "Sapphire" },
                { "1682", "XFX" },
                { "148C", "PowerColor" },
                { "1ACC", "Inno3D" },
                { "807D", "Gainward" },
                { "1849", "ASRock" },
                { "1AB8", "Colorful" },
                { "1B4C", "Galax / KFA2" },
            };

        public GPU()
        {
            UpdateName();
            UpdateBoardManufacturer();
            Refresh();
        }

        // ── Helpers ──────────────────────────────────────────────

        private IHardware GetGpuHardware() =>
            _computer.Hardware.FirstOrDefault(h =>
                h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuAmd);

        private ISensor FindGpuSensor(SensorType type, string nameContains = null)
        {
            var gpu = GetGpuHardware();
            if (gpu == null) return null;
            return gpu.Sensors.FirstOrDefault(s =>
                s.SensorType == type &&
                (nameContains == null || s.Name.Contains(nameContains)));
        }

        // ── Static data (called once) ───────────────────────────

        private void UpdateName()
        {
            Name = string.Empty;
            using var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_VideoController");
            foreach (ManagementObject obj in searcher.Get())
            {
                Name = obj["Caption"]?.ToString() ?? "Unknown GPU";
            }
        }

        private void UpdateBoardManufacturer()
        {
            BoardManufacturer = "Unknown";
            try
            {
                const string displayClassGuid = "{4d36e968-e325-11ce-bfc1-08002be10318}";

                using var pciKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SYSTEM\CurrentControlSet\Enum\PCI");
                if (pciKey == null) return;

                foreach (var devKeyName in pciKey.GetSubKeyNames())
                {
                    // Only NVIDIA (VEN_10DE) or AMD (VEN_1002)
                    if (!devKeyName.StartsWith("VEN_10DE", StringComparison.OrdinalIgnoreCase) &&
                        !devKeyName.StartsWith("VEN_1002", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var subsysIdx = devKeyName.IndexOf("SUBSYS_", StringComparison.OrdinalIgnoreCase);
                    if (subsysIdx < 0 || devKeyName.Length < subsysIdx + 15) continue;

                    var subsysVendor = devKeyName.Substring(subsysIdx + 11, 4);

                    using var devKey = pciKey.OpenSubKey(devKeyName);
                    if (devKey == null) continue;

                    foreach (var instanceKeyName in devKey.GetSubKeyNames())
                    {
                        using var instanceKey = devKey.OpenSubKey(instanceKeyName);
                        if (instanceKey == null) continue;

                        var classGuid = instanceKey.GetValue("ClassGUID") as string;
                        if (classGuid == null ||
                            !classGuid.Equals(displayClassGuid, StringComparison.OrdinalIgnoreCase))
                            continue;

                        BoardManufacturer = VendorMap.TryGetValue(subsysVendor, out var name)
                            ? name
                            : $"Vendor {subsysVendor.ToUpper()}";
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateBoardManufacturer failed: {ex.Message}");
            }
        }

        // ── Dynamic data (called every tick) ────────────────────

        public void Refresh()
        {
            UpdateTemperature();
            UpdateCoreClockSpeed();
            UpdateMemoryClockSpeed();
            UpdatePowerLoad();
            UpdateFanSpeed();
            UpdateLoad();
        }

        private void UpdateTemperature()
        {
            var sensor = FindGpuSensor(SensorType.Temperature);
            TemperatureValue = sensor?.Value ?? 0;
            Temperature = sensor?.Value.HasValue == true ? $"{sensor.Value.Value:F1} °C" : "N/A";
        }

        private void UpdateCoreClockSpeed()
        {
            var sensor = FindGpuSensor(SensorType.Clock, "GPU Core");
            CoreClockSpeed = sensor?.Value ?? 0;
        }

        private void UpdateMemoryClockSpeed()
        {
            var sensor = FindGpuSensor(SensorType.Clock, "GPU Memory");
            MemoryClockSpeed = sensor?.Value ?? 0;
        }

        private void UpdatePowerLoad()
        {
            var sensor = FindGpuSensor(SensorType.Power);
            PowerLoad = sensor?.Value ?? 0;
        }

        private void UpdateFanSpeed()
        {
            Fans.Clear();
            var gpu = GetGpuHardware();
            if (gpu == null) return;

            foreach (var sensor in gpu.Sensors)
            {
                if (sensor.SensorType == SensorType.Fan)
                {
                    Fans.Add(sensor.Value.HasValue ? $"{sensor.Value.Value:F1} RPM" : "N/A");
                }
            }
        }

        private void UpdateLoad()
        {
            var sensor = FindGpuSensor(SensorType.Load, "GPU Core");
            CoreLoad = sensor?.Value ?? 0;
        }
    }
}
