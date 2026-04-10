using System;
using LibreHardwareMonitor.Hardware;

namespace MoRoC.Classes
{
    public class HardwareMonitorService : IDisposable
    {
        private static HardwareMonitorService _instance;
        private static readonly object _lock = new object();
        private readonly Computer _computer;
        private bool _disposed;

        private HardwareMonitorService()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMotherboardEnabled = true,
                IsStorageEnabled = true,
                IsMemoryEnabled = true,
                IsNetworkEnabled = true,
                IsControllerEnabled = true
            };
            _computer.Open();
        }

        public static HardwareMonitorService Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new HardwareMonitorService();
                    }
                    return _instance;
                }
            }
        }

        public Computer Computer => _computer;

        public void UpdateAll()
        {
            _computer.Accept(new UpdateVisitor());
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed) return;
                _disposed = true;
                _computer?.Close();
                _instance = null;
            }
        }
    }
}
