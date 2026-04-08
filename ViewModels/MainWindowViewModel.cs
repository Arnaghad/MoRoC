using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Threading;
using ReactiveUI;
using MoRoC.Classes;

namespace MoRoC.ViewModels
{
    public class RamSlotInfo
    {
        public string Slot { get; set; }
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }
        public string Capacity { get; set; }
        public string Speed { get; set; }
        public string ConfiguredSpeed { get; set; }
    }
    public partial class MainWindowViewModel : ReactiveObject
    {
        private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1, 1);
        private readonly CPU _cpu;
        private readonly GPU _gpu;
        private readonly RAM _ram;
        private readonly MotherBoard _motherboard;
        private readonly Storage _storage;
        private readonly Task _updateTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private ObservableCollection<RamSlotInfo> _ramSlots;
        public ObservableCollection<RamSlotInfo> RamSlots
        {
            get => _ramSlots;
            set => this.RaiseAndSetIfChanged(ref _ramSlots, value);
        }
        
        private RamSlotInfo _selectedRamSlot;
        public RamSlotInfo SelectedRamSlot
        {
            get => _selectedRamSlot;
            set => this.RaiseAndSetIfChanged(ref _selectedRamSlot, value);
        }
        
        public string CpuName { get; }
        public string GpuName { get; }
        public string MbName { get; }
        public string MbManufacturer { get; }
        public string MbBiosName { get; }
        public string MbManufacturerBiosName { get; }
        public string StorageNames { get; }
        
        public string TotalRamVolume => _ram.TotalVolume;
        public int PhysicalCores { get; }
        public int LogicalCores { get; }
        
        private string _cpuTemp;
        public string CpuTemp { get => _cpuTemp; set => this.RaiseAndSetIfChanged(ref _cpuTemp, value); }

        private string _gpuTemp;
        public string GpuTemp { get => _gpuTemp; set => this.RaiseAndSetIfChanged(ref _gpuTemp, value); }
        
        private string _mbTemp;
        public string MbTemp { get => _mbTemp; set => this.RaiseAndSetIfChanged(ref _mbTemp, value); }
        
        private string _storageTemp;
        public string StorageTemp { get => _storageTemp; set => this.RaiseAndSetIfChanged(ref _storageTemp, value); }

        private string _cpuFanSpeed;
        public string CpuFanSpeed { get => _cpuFanSpeed; set => this.RaiseAndSetIfChanged(ref _cpuFanSpeed, value); }

        private string _gpuFanSpeed;
        public string GpuFanSpeed { get => _gpuFanSpeed; set => this.RaiseAndSetIfChanged(ref _gpuFanSpeed, value); }

        private string _mbFanSpeed;
        public string MbFanSpeed { get => _mbFanSpeed; set => this.RaiseAndSetIfChanged(ref _mbFanSpeed, value); }

        private string _cpuUsage;
        public string CpuUsage { get => _cpuUsage; set => this.RaiseAndSetIfChanged(ref _cpuUsage, value); }

        private string _gpuLoad;
        public string GpuLoad { get => _gpuLoad; set => this.RaiseAndSetIfChanged(ref _gpuLoad, value); }

        private string _cpuPowerUsage;
        public string CpuPowerUsage { get => _cpuPowerUsage; set => this.RaiseAndSetIfChanged(ref _cpuPowerUsage, value); }

        private string _gpuPowerLoad;
        public string GpuPowerLoad { get => _gpuPowerLoad; set => this.RaiseAndSetIfChanged(ref _gpuPowerLoad, value); }

        private string _cpuClockSpeed;
        public string CpuClockSpeed { get => _cpuClockSpeed; set => this.RaiseAndSetIfChanged(ref _cpuClockSpeed, value); }

        private string _coresClockSpeed;
        public string CoresClockSpeed { get => _coresClockSpeed; set => this.RaiseAndSetIfChanged(ref _coresClockSpeed, value); }

        private string _gpuClockSpeed;
        public string GpuClockSpeed { get => _gpuClockSpeed; set => this.RaiseAndSetIfChanged(ref _gpuClockSpeed, value); }

        private string _gpuMemoryClockSpeed;
        public string GpuMemoryClockSpeed { get => _gpuMemoryClockSpeed; set => this.RaiseAndSetIfChanged(ref _gpuMemoryClockSpeed, value); }

        public MainWindowViewModel()
        {
            _cpu = new CPU();
            _gpu = new GPU();
            _motherboard = new MotherBoard();
            _storage = new Storage();
            _ram = new RAM();
            
            CpuName = _cpu.Name;
            PhysicalCores = _cpu.PhysicalCores;
            LogicalCores = _cpu.LogicalCores;

            GpuName = _gpu.Name;

            MbName = _motherboard.Name;
            MbBiosName = _motherboard.BiosName;
            MbManufacturerBiosName = _motherboard.BiosManufacturer;
            MbManufacturer = _motherboard.Manufacturer;
            
            StorageNames = string.Join("\n", _storage.Names);

            InitializeRamSlots();
            // Start background update task
            _updateTask = Task.Run(UpdateValuesAsync, _cancellationTokenSource.Token);
        }
        
        private void InitializeRamSlots()
        {
            RamSlots = new ObservableCollection<RamSlotInfo>();
            
            for (int i = 0; i < _ram.Name.Length; i++)
            {
                var ramSlot = new RamSlotInfo
                {
                    Slot = _ram.Slot[i],
                    Manufacturer = _ram.Name[i],
                    PartNumber = _ram.PartNumber[i],
                    Capacity = $"{_ram.Volume[i]} GB",
                    Speed = $"{_ram.Speed[i]} MHz",
                    ConfiguredSpeed = $"{_ram.Speed[i]} MHz"
                };
                RamSlots.Add(ramSlot);
            }

            if (RamSlots.Count > 0)
            {
                SelectedRamSlot = RamSlots[0];
            }
        }
        
        private async Task UpdateValuesAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                await _updateLock.WaitAsync();
                try
                {
                    // Update all components hardware sensors once
                    HardwareMonitorService.Instance.UpdateAll();

                    _cpu.Refresh();
                    _gpu.Refresh();
                    _motherboard.Refresh();
                    _storage.Refresh();
                    
                    // Collect updated values
                    Dispatcher.UIThread.Post(() =>
                    {
                        CpuTemp = _cpu.Temperature;
                        GpuTemp = _gpu.Temperature;
                        MbTemp = _motherboard.Temperature;
                        StorageTemp = string.Join("\n", _storage.Temperatures);
                        CpuFanSpeed = string.Join("\n", _cpu.Fans);
                        GpuFanSpeed = string.Join("\n", _gpu.Fans);
                        MbFanSpeed = string.Join("\n", _motherboard.Fans);
                        CpuUsage = _cpu.CpuUsage;
                        CpuPowerUsage = _cpu.PowerUsage;
                        CpuClockSpeed = _cpu.TotalClockSpeed;
                        CoresClockSpeed = string.Join("\n", _cpu.ClockSpeeds);
                        GpuClockSpeed = $"{_gpu.CoreClockSpeed} MHz";
                        GpuMemoryClockSpeed = $"{_gpu.MemoryClockSpeed} MHz";
                        GpuLoad = $"{_gpu.CoreLoad}%";
                        GpuPowerLoad = $"{_gpu.PowerLoad} W";
                    });
                }
                finally
                {
                    _updateLock.Release();
                }

                await Task.Delay(1000, _cancellationTokenSource.Token);
            }
        }

        public void Cleanup()
        {
            _cancellationTokenSource.Cancel();
            _updateTask.Wait(TimeSpan.FromSeconds(1));
            _updateLock.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}
