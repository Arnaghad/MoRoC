using System;
using System.Collections.Generic;
using System.Linq;
using LibreHardwareMonitor.Hardware;
using System.Management;

namespace MoRoC.Classes
{
    public class Storage : HardwareMonitor, IDisposable
    {
        // Властивості
        public List<string> Names { get; private set; } = new List<string>();
        public List<string> Temperatures { get; private set; } = new List<string>();
        
        // Конструктор
        public Storage() : base()
        {
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
            foreach (var hardware in computer.Hardware)
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
            computer?.Close();
        }
    }
}
