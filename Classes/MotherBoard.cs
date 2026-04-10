using System.Collections.Generic;
using System.Linq;
using System.Management;
using LibreHardwareMonitor.Hardware;

namespace MoRoC.Classes
{
    public class MotherBoard
    {
        private Computer _computer => HardwareMonitorService.Instance.Computer;

        public string Name { get; private set; }
        public string Manufacturer { get; private set; }
        public string Temperature { get; private set; }
        public float TemperatureValue { get; private set; }
        public string BiosName { get; private set; }
        public string BiosManufacturer { get; private set; }
        public List<string> Fans { get; private set; } = new List<string>();

        public MotherBoard()
        {
            UpdateStaticInfo();
            Refresh();
        }

        /// <summary>
        /// Fetches Name + Manufacturer in a single WMI query (was two separate queries).
        /// Also fetches BIOS info.
        /// </summary>
        private void UpdateStaticInfo()
        {
            // Single WMI query for baseboard (was two separate queries for Name and Manufacturer)
            Name = "Unknown";
            Manufacturer = "Unknown";
            using (var searcher = new ManagementObjectSearcher("SELECT Product, Manufacturer FROM Win32_BaseBoard"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    Name = obj["Product"]?.ToString() ?? "Unknown Motherboard";
                    Manufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown";
                }
            }

            // BIOS info
            BiosName = "Unknown";
            BiosManufacturer = "Unknown";
            using (var searcher = new ManagementObjectSearcher("SELECT Name, Manufacturer FROM Win32_BIOS"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    BiosName = obj["Name"]?.ToString() ?? "Unknown";
                    BiosManufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown";
                }
            }
        }

        public void Refresh()
        {
            UpdateTemperature();
            UpdateFanSpeed();
        }

        private void UpdateTemperature()
        {
            var sensor = _computer.Hardware
                .Where(h => h.HardwareType == HardwareType.Motherboard)
                .SelectMany(h => h.SubHardware)
                .SelectMany(sh => sh.Sensors)
                .FirstOrDefault(s => s.SensorType == SensorType.Temperature);

            TemperatureValue = sensor?.Value ?? 0;
            Temperature = sensor?.Value.HasValue == true ? $"{sensor.Value.Value:F1} °C" : "N/A";
        }

        private void UpdateFanSpeed()
        {
            Fans.Clear();

            var fans = _computer.Hardware
                .Where(hardware => hardware.HardwareType == HardwareType.Motherboard)
                .SelectMany(hardware => hardware.SubHardware)
                .SelectMany(subHardware => subHardware.Sensors)
                .Where(sensor => sensor.SensorType == SensorType.Fan)
                .Select(sensor => sensor.Value.HasValue ? $"{sensor.Value.Value:F0} RPM" : "N/A");

            Fans.AddRange(fans);
        }
    }
}
