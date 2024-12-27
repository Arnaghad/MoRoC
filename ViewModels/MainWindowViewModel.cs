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
        public DebounceProperty<string> CpuTemp { get; }
        public DebounceProperty<string> GpuTemp { get; }
        public  DebounceProperty<string> MbTemp { get; }
        public DebounceProperty<string> StorageTemp { get; }
        public DebounceProperty<string> CpuFanSpeed { get; }
        public DebounceProperty<string> GpuFanSpeed { get; }
        public DebounceProperty<string> MbFanSpeed { get; }
        public DebounceProperty<string> CpuUsage { get; }
        public DebounceProperty<string> GpuLoad { get; }
        public DebounceProperty<string> CpuPowerUsage { get; }
        public DebounceProperty<string> GpuPowerLoad { get; }
        public DebounceProperty<string> CpuClockSpeed { get; }
        public DebounceProperty<string> CoresClockSpeed { get; }
        public DebounceProperty<string> GpuClockSpeed { get; }
        public DebounceProperty<string> GpuMemoryClockSpeed { get; }
        public MainWindowViewModel()
        {
            _cpu = new CPU();
            _gpu = new GPU();
            _motherboard = new MotherBoard();
            _storage = new Storage();
            _ram = new RAM();
            
            CpuName = _cpu.Name;
            CpuTemp = new DebounceProperty<string>(TimeSpan.FromSeconds(1));
            CpuFanSpeed = new DebounceProperty<string>(TimeSpan.FromSeconds(1));
            PhysicalCores = _cpu.PhysicalCores;
            LogicalCores = _cpu.LogicalCores;
            CpuUsage = new DebounceProperty<string>(TimeSpan.FromSeconds(1));
            CpuPowerUsage = new DebounceProperty<string>(TimeSpan.FromSeconds(1));
            CpuClockSpeed = new DebounceProperty<string>(TimeSpan.FromSeconds(1));
            CoresClockSpeed = new DebounceProperty<string>(TimeSpan.FromSeconds(1));

            GpuName = _gpu.Name;
            GpuTemp = new DebounceProperty<string>(TimeSpan.FromSeconds(1));
            GpuFanSpeed = new DebounceProperty<string>(TimeSpan.FromSeconds(1));
            GpuClockSpeed = new DebounceProperty<string>(TimeSpan.FromSeconds(1));
            GpuMemoryClockSpeed = new DebounceProperty<string>(TimeSpan.FromSeconds(1));
            GpuLoad = new DebounceProperty<string>(TimeSpan.FromSeconds(1));
            GpuPowerLoad = new DebounceProperty<string>(TimeSpan.FromSeconds(1));

            MbName = _motherboard.Name;
            MbTemp = new DebounceProperty<string>(TimeSpan.FromSeconds(1));
            MbFanSpeed = new DebounceProperty<string>(TimeSpan.FromSeconds(1));
            MbBiosName = _motherboard.BiosName;
            MbManufacturerBiosName = _motherboard.BiosManufacturer;
            MbManufacturer = _motherboard.Manufacturer;
            
            StorageNames = string.Join("\n", _storage.Names);
            StorageTemp = new DebounceProperty<string>(TimeSpan.FromSeconds(1));

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
                    // Update all components
                    _cpu.Refresh();
                    _gpu.Refresh();
                    _motherboard.Refresh();
                    _storage.Refresh();
                    
                    // Collect updated values
                    Dispatcher.UIThread.Post(() =>
                    {
                        CpuTemp.Input.Value = _cpu.Temperature;
                        GpuTemp.Input.Value = _gpu.Temperature;
                        MbTemp.Input.Value = _motherboard.Temperature;
                        StorageTemp.Input.Value = string.Join("\n", _storage.Temperatures);
                        CpuFanSpeed.Input.Value = string.Join("\n", _cpu.Fans);
                        GpuFanSpeed.Input.Value = string.Join("\n", _gpu.Fans);
                        MbFanSpeed.Input.Value = string.Join("\n", _motherboard.Fans);
                        CpuUsage.Input.Value = _cpu.CpuUsage;
                        CpuPowerUsage.Input.Value = _cpu.PowerUsage;
                        CpuClockSpeed.Input.Value = $"{_cpu.TotalClockSpeed} MHz";
                        CoresClockSpeed.Input.Value = string.Join("\n", _cpu.ClockSpeeds);
                        GpuClockSpeed.Input.Value = $"{_gpu.CoreClockSpeed} MHz";
                        GpuMemoryClockSpeed.Input.Value = $"{_gpu.MemoryClockSpeed} MHz";
                        GpuLoad.Input.Value = $"{_gpu.CoreLoad}%";
                        GpuPowerLoad.Input.Value = $"{_gpu.PowerLoad} W";
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
