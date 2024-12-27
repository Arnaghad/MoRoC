using System;
using System.Collections.Generic;
using System.Management;
using LibreHardwareMonitor.Hardware;
using System.Linq;

namespace MoRoC.Classes
{
    public class MotherBoard : HardwareMonitor, IDisposable
    {
        private Computer _computer;

        public string Name { get; private set; }
        public string Manufacturer { get; private set; }
        public string Temperature { get; private set; }
        public string BiosName { get; private set; }
        public string BiosManufacturer { get; private set; }
        public List<string> Fans { get; private set; } = new List<string>();

        public MotherBoard() : base()
        {
            _computer = new Computer
            {
                IsMotherboardEnabled = true
            };
            _computer.Open();

            UpdateAllProperties();
        }

        private void UpdateAllProperties()
        {
            UpdateName();
            UpdateMotherboardManufacturer();
            UpdateTemperature();
            UpdateBiosInfo();
            UpdateFanSpeed();
        }

        private void UpdateName()
        {
            Name = "Unknown";
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    Name = obj["Product"]?.ToString() ?? "Unknown Motherboard";
                }
            }
        }

        private void UpdateMotherboardManufacturer()
        {
            Manufacturer = "Unknown";
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    Manufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown";
                }
            }
        }

        private void UpdateTemperature()
        {
            Temperature = _computer.Hardware
                .Where(hardwareItem => hardwareItem.HardwareType == HardwareType.Motherboard)
                .SelectMany(hardwareItem =>
                {
                    hardwareItem.Update();
                    return hardwareItem.SubHardware.Select(subHardware =>
                    {
                        subHardware.Update();
                        return subHardware.Sensors;
                    }).SelectMany(sensors => sensors);
                })
                .Where(sensor => sensor.SensorType == SensorType.Temperature)
                .Select(sensor => sensor.Value.HasValue ? $"{sensor.Value.Value:F1} \u00b0C" : "N/A")
                .Skip(1) // Пропускаємо перше знайдене значення
                .FirstOrDefault() ?? "N/A"; // Беремо друге знайдене значення
        }

        private void UpdateBiosInfo()
        {
            BiosName = "Unknown";
            BiosManufacturer = "Unknown";

            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    BiosName = obj["Name"]?.ToString() ?? "Unknown";
                    BiosManufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown";
                }
            }
        }

        private void UpdateFanSpeed()
        {
            Fans.Clear();

            var fans = _computer.Hardware
                .Where(hardware => hardware.HardwareType == HardwareType.Motherboard)
                .SelectMany(hardware => {
                    hardware.Update();
                    return hardware.SubHardware;
                })
                .SelectMany(subHardware => {
                    subHardware.Update();
                    return subHardware.Sensors;
                })
                .Where(sensor => sensor.SensorType == SensorType.Fan)
                .Skip(2)
                .Select(sensor => sensor.Value.HasValue ? $"{sensor.Value.Value:F0} RPM" : "N/A");

            Fans.AddRange(fans);
        }

        public void Refresh()
        {
            UpdateTemperature();
            UpdateFanSpeed();
        }

        public void Dispose()
        {
            _computer?.Close();
            _computer = null;
        }
    }
}
