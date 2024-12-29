using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using LibreHardwareMonitor.Hardware;

namespace MoRoC.Classes
{
    public class CPU : IDisposable
    {
        private Computer _computer;

        public string Name { get; private set;}
        public string Temperature { get; private set; }
        public List<string> Fans { get; private set; }
        public int LogicalCores { get; }
        public int PhysicalCores { get; }
        public int MaxClockSpeed { get; }
        public string PowerUsage { get; private set; }
        public string CpuUsage { get; private set; }
        public List<string> ClockSpeeds { get; private set; }
        public string TotalClockSpeed { get; private set; }
        public float CoreLoad { get; private set; }

        public CPU() : base()
        {
            // Initialize static properties once
            LogicalCores = GetLogicalCores();
            PhysicalCores = GetPhysicalCores();
            MaxClockSpeed = GetMaxClockSpeed();
            
            // Initialize collections
            Fans = new List<string>();
            ClockSpeeds = new List<string>();

            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsMotherboardEnabled = true
            };
            _computer.Open();

            UpdateAllProperties();
        }

        private void UpdateAllProperties()
        {
            UpdateName();
            UpdateTemperature();
            UpdatePowerUsage();
            UpdateCpuUsage();
            UpdateFanSpeed();
            UpdateClockSpeeds();
            UpdateTotalClockSpeed();
            UpdateLoad();
        }

        private void UpdateName()
        {
            Name = string.Empty;
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    Name = obj["Name"]?.ToString() ?? "Unknown CPU";
                }
            }
        }

        private void UpdateTemperature()
        {
            Temperature = "N/A";
            foreach (var hardwareItem in _computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.Cpu)
                {
                    hardwareItem.Update();
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("CPU Package"))
                        {
                            Temperature = sensor.Value.HasValue ? $"{sensor.Value.Value:F1} °C" : "N/A";
                            return;
                        }
                    }
                }
            }
        }

        private void UpdatePowerUsage()
        {
            PowerUsage = "N/A";
            foreach (var hardwareItem in _computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.Cpu)
                {
                    hardwareItem.Update();
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("CPU Package"))
                        {
                            PowerUsage = sensor.Value.HasValue ? $"{sensor.Value.Value:F1} W" : "N/A";
                            return;
                        }
                    }
                }
            }
        }

        private void UpdateCpuUsage()
        {
            CpuUsage = "N/A";
            foreach (var hardwareItem in _computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.Cpu)
                {
                    hardwareItem.Update();
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("CPU Total"))
                        {
                            CpuUsage = sensor.Value.HasValue ? $"{sensor.Value.Value:F1} %" : "N/A";
                            return;
                        }
                    }
                }
            }
        }

        private void UpdateFanSpeed()
        {
            Fans.Clear();
            void UpdateAndCollectSensors(IHardware hardware, List<ISensor> sensors)
            {
                hardware.Update();
                sensors.AddRange(hardware.Sensors);

                // Обробка вкладених підкомпонентів
                foreach (var subHardware in hardware.SubHardware)
                {
                    UpdateAndCollectSensors(subHardware, sensors);
                }
            }

            // Список усіх сенсорів вентиляторів
            var allFanSensors = new List<ISensor>();
            foreach (var hardware in _computer.Hardware)
            {
                UpdateAndCollectSensors(hardware, allFanSensors);
            }

            // Фільтруємо вентилятори
            var fanSensors = allFanSensors
                .Where(s => s.SensorType == SensorType.Fan)
                .OrderBy(s => s.Name);
            
            Fans = fanSensors
                .Take(2) // Беремо тільки перші два елементи
                .Select(s => $"{s.Value?.ToString("F2") ?? "N/A"} RPM") // Форматуємо значення і додаємо "RPM"
                .ToList(); // Конвертуємо в List<string>
        }

        private void UpdateClockSpeeds()
        {
            ClockSpeeds.Clear();
            foreach (var cpu in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Cpu))
            {
                cpu.Update();
                var speeds = cpu.Sensors
                    .Where(s => s.SensorType == SensorType.Clock && s.Value.HasValue)
                    .Select(s => $"{s.Name}: {s.Value.Value} MHz");
                ClockSpeeds.AddRange(speeds);
            }
        }

        private void UpdateTotalClockSpeed()
        {
            double totalSpeed = 0;
            int coreCount = 0;

            foreach (var cpu in _computer.Hardware.Where(h => h.HardwareType == HardwareType.Cpu))
            {
                cpu.Update();
                var coreSpeeds = cpu.Sensors
                    .Where(s => s.SensorType == SensorType.Clock 
                                && s.Value.HasValue 
                                && s.Name != "Bus Speed")
                    .Select(s => s.Value.Value);

                totalSpeed += coreSpeeds.Sum();
                coreCount += coreSpeeds.Count();
            }

            // Розрахунок середньої швидкості
            double averageSpeed = coreCount > 0 ? totalSpeed / coreCount : 0;
            TotalClockSpeed = $"{(averageSpeed / 1000):F2} GHz"; // Ділимо на 1000 для конвертації в GHz
        }

        private void UpdateLoad()
        {
            foreach (var hardwareItem in _computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.Cpu)
                {
                    hardwareItem.Update();
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("CPU Total"))
                        {
                            CoreLoad = sensor.Value ?? 0;
                            return;
                        }
                    }
                }
            }
        }

        private static int GetLogicalCores()
        {
            try
            {
                return Environment.ProcessorCount;
            }
            catch
            {
                return -1;
            }
        }

        private static int GetPhysicalCores()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT NumberOfCores FROM Win32_Processor");
                return searcher.Get().Cast<ManagementObject>()
                    .Sum(item => int.Parse(item["NumberOfCores"].ToString()));
            }
            catch
            {
                return -1;
            }
        }

        private static int GetMaxClockSpeed()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT MaxClockSpeed FROM Win32_Processor");
                return searcher.Get().Cast<ManagementObject>()
                    .Sum(item => int.Parse(item["MaxClockSpeed"].ToString()));
            }
            catch
            {
                return -1;
            }
        }

        public void Refresh()
        {
            UpdateAllProperties();
        }

        public void Dispose()
        {
            _computer?.Close();
            _computer = null;
        }
    }
}