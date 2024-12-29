using System;
using System.Collections.Generic;
using System.Linq;
using LibreHardwareMonitor.Hardware;
using System.Management;

namespace MoRoC.Classes
{
    public class Storage : IDisposable
    {
        // Властивості
        public List<string> Names { get; private set; } = new List<string>();
        public List<string> Temperatures { get; private set; } = new List<string>();
        
        private Computer _computer;
        // Конструктор
        public Storage() : base()
        {
            _computer = new Computer
            {
                IsStorageEnabled = true
            };
            _computer.Open();
            UpdateAllProperties();
        }

        // Оновлення всіх властивостей
        private void UpdateAllProperties()
        {
            UpdateNames();
            UpdateTemperatures();
        }

        private void UpdateNames()
        {
            Names.Clear();
            using (var modelSearcher = new ManagementObjectSearcher("SELECT Model FROM Win32_DiskDrive"))
            {
                foreach (ManagementObject disk in modelSearcher.Get())
                {
                    string model = disk["Model"]?.ToString() ?? "Unknown Storage";
                    Names.Add(model);
                }
            }
        }

        private void UpdateTemperatures()
        {
            Temperatures.Clear();
            foreach (var hardware in _computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Storage)
                {
                    hardware.Update();
                    var tempSensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature);
                    if (tempSensor != null && tempSensor.Value.HasValue)
                    {
                        Temperatures.Add($"{tempSensor.Value.Value:F1} °C");
                    }
                    else
                    {
                        Temperatures.Add("N/A");
                    }
                }
            }
        }

        // Метод для оновлення всіх властивостей
        public void Refresh()
        {
            UpdateAllProperties();
        }

        public void Dispose()
        {
            _computer?.Close();
        }
    }
}
