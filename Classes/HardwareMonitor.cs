using LibreHardwareMonitor.Hardware;

namespace MoRoC.Classes
{
    public class HardwareMonitor
    {
        protected readonly Computer computer;

        public HardwareMonitor()
        {
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = true
            };
            computer.Open();
        }

        ~HardwareMonitor()
        {
            computer.Close();
        }
    }
}