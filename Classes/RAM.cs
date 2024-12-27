using System;
using System.Collections.Generic;
using System.Management;

namespace MoRoC.Classes
{
    public class RAM
    {
        public string[] Name { get; private set; }           // Назва виробника для кожного слота
        public string[] Slot { get; private set; }           // Номер слота
        public int[] Speed { get; private set; }             // Швидкість для кожного слота
        public double[] Volume { get; private set; }         // Обсяг для кожного слота
        public string[] PartNumber { get; private set; }     // Номер партії для кожного слота

        // Загальний обсяг оперативної пам'яті
        public string TotalVolume { get; private set; }

        public RAM()
        {
            LoadMemoryInfo();
        }

        private void LoadMemoryInfo()
        {
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Speed, Capacity, PartNumber, ConfiguredClockSpeed, DeviceLocator FROM Win32_PhysicalMemory");
                
                var nameList = new List<string>();
                var slotList = new List<string>();
                var speedList = new List<int>();
                var volumeList = new List<double>();
                var partNumberList = new List<string>();

                double totalVolume = 0;

                foreach (ManagementObject obj in searcher.Get())
                {
                    // Назва виробника
                    nameList.Add(obj["Manufacturer"]?.ToString() ?? "Невідомий виробник");

                    // Номер слота
                    slotList.Add(obj["DeviceLocator"]?.ToString() ?? "Невідомий слот");

                    // Швидкість
                    if (int.TryParse(obj["ConfiguredClockSpeed"]?.ToString(), out int speed))
                        speedList.Add(speed);
                    else
                        speedList.Add(0);

                    // Обсяг пам'яті у GB
                    if (obj["Capacity"] != null)
                    {
                        double volume = Math.Round(Convert.ToDouble(obj["Capacity"]) / (1024 * 1024 * 1024), 2);
                        volumeList.Add(volume);
                        totalVolume += volume; // Підрахунок загального обсягу
                    }
                    else
                    {
                        volumeList.Add(0);
                    }

                    // Номер партії
                    partNumberList.Add(obj["PartNumber"]?.ToString() ?? "Невідомий DRAM");
                }

                // Присвоюємо зібрані дані масивам
                Name = nameList.ToArray();
                Slot = slotList.ToArray();
                Speed = speedList.ToArray();
                Volume = volumeList.ToArray();
                PartNumber = partNumberList.ToArray();

                // Загальний обсяг
                TotalVolume = Convert.ToString(Math.Round(totalVolume, 2)) + " GB";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка отримання інформації про RAM: {ex.Message}");
            }
        }
    }
}
