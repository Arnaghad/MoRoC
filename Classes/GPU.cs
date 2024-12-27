using System;
using System.Collections.Generic;
using System.Management;
using LibreHardwareMonitor.Hardware;

namespace MoRoC.Classes
{
    public class GPU : HardwareMonitor, IDisposable
    {
        private Computer _computer;

        public string Name { get; private set; }
        public string Temperature { get; private set; }
        public float CoreClockSpeed { get; private set; }
        public float MemoryClockSpeed { get; private set; }
        public float PowerLoad { get; private set; }
        public float CoreLoad { get; private set; }
        public List<string> Fans { get; private set; } = new List<string>();

        public GPU() : base()
        {
            _computer = new Computer
            {
                IsGpuEnabled = true
            };
            _computer.Open();

            UpdateAllProperties();
        }

        private void UpdateAllProperties()
        {
            UpdateName();
            UpdateTemperature();
            UpdateCoreClockSpeed();
            UpdateMemoryClockSpeed();
            UpdatePowerLoad();
            UpdateFanSpeed();
            UpdateLoad();
        }

        private void UpdateName()
        {
            Name = string.Empty;
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    Name = obj["Caption"]?.ToString() ?? "Unknown GPU";
                }
            }
        }

        private void UpdateTemperature()
        {
            Temperature = "N/A";
            foreach (var hardwareItem in _computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAmd)
                {
                    hardwareItem.Update();
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            Temperature = sensor.Value.HasValue ? $"{sensor.Value.Value:F1} \u00b0C" : "N/A";
                            return;
                        }
                    }
                }
            }
        }

        private void UpdateCoreClockSpeed()
        {
            CoreClockSpeed = 0;
            foreach (var hardwareItem in _computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAmd)
                {
                    hardwareItem.Update();
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("GPU Core"))
                        {
                            CoreClockSpeed = sensor.Value ?? 0;
                            return;
                        }
                    }
                }
            }
        }

        private void UpdateMemoryClockSpeed()
        {
            MemoryClockSpeed = 0;
            foreach (var hardwareItem in _computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAmd)
                {
                    hardwareItem.Update();
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("GPU Memory"))
                        {
                            MemoryClockSpeed = sensor.Value ?? 0;
                            return;
                        }
                    }
                }
            }
        }

        private void UpdatePowerLoad()
        {
            PowerLoad = 0;
            foreach (var hardwareItem in _computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAmd)
                {
                    hardwareItem.Update();
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Power)
                        {
                            PowerLoad = sensor.Value ?? 0;
                            return;
                        }
                    }
                }
            }
        }

        private void UpdateFanSpeed()
        {
            Fans.Clear();
            foreach (var hardware in _computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Fan)
                        {
                            Fans.Add(sensor.Value.HasValue ? $"{sensor.Value.Value:F1} RPM" : "N/A");
                        }
                    }
                }
            }
        }

        private void UpdateLoad()
        {
            CoreLoad = 0;
            foreach (var hardwareItem in _computer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.GpuNvidia || hardwareItem.HardwareType == HardwareType.GpuAmd)
                {
                    hardwareItem.Update();
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("GPU Core"))
                        {
                            CoreLoad = sensor.Value ?? 0;
                            return;
                        }
                    }
                }
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
