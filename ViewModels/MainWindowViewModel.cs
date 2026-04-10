using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Threading;
using ReactiveUI;
using MoRoC.Classes;
using MoRoC.Models;

namespace MoRoC.ViewModels
{
    public class MainWindowViewModel : ReactiveObject, IDisposable
    {
        // ── Fields ───────────────────────────────────────────────
        private readonly SemaphoreSlim _updateLock = new(1, 1);
        private readonly CPU _cpu;
        private readonly GPU _gpu;
        private readonly RAM _ram;
        private readonly MotherBoard _motherboard;
        private readonly Storage _storage;
        private readonly Task _updateTask;
        private readonly CancellationTokenSource _cts = new();
        private bool _disposed;

        // ── Static properties (set once in constructor) ─────────
        public string CpuName { get; }
        public string GpuName { get; }
        public string GpuBoardManufacturer { get; }
        public string MbName { get; }
        public string MbManufacturer { get; }
        public string MbBiosName { get; }
        public string MbManufacturerBiosName { get; }
        public string StorageNames { get; }
        public string TotalRamVolume => _ram.TotalVolume;
        public int PhysicalCores { get; }
        public int LogicalCores { get; }

        // ── RAM ──────────────────────────────────────────────────
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

        // ── Dynamic string properties (UI text display) ─────────
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

        // ── Graph value properties (double, for BaseGraph binding) ──
        private double _cpuTempValue = double.NaN;
        public double CpuTempValue
        {
            get => _cpuTempValue;
            set { _cpuTempValue = value; this.RaisePropertyChanged(); }
        }

        private double _cpuClockValue = double.NaN;
        public double CpuClockValue
        {
            get => _cpuClockValue;
            set { _cpuClockValue = value; this.RaisePropertyChanged(); }
        }

        private double _gpuTempValue = double.NaN;
        public double GpuTempValue
        {
            get => _gpuTempValue;
            set { _gpuTempValue = value; this.RaisePropertyChanged(); }
        }

        private double _gpuClockValue = double.NaN;
        public double GpuClockValue
        {
            get => _gpuClockValue;
            set { _gpuClockValue = value; this.RaisePropertyChanged(); }
        }

        private double _mbTempValue = double.NaN;
        public double MbTempValue
        {
            get => _mbTempValue;
            set { _mbTempValue = value; this.RaisePropertyChanged(); }
        }

        // ── Constructor ──────────────────────────────────────────
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
            GpuBoardManufacturer = _gpu.BoardManufacturer;

            MbName = _motherboard.Name;
            MbBiosName = _motherboard.BiosName;
            MbManufacturerBiosName = _motherboard.BiosManufacturer;
            MbManufacturer = _motherboard.Manufacturer;
            
            StorageNames = string.Join("\n", _storage.Names);

            InitializeRamSlots();

            // Single background polling loop for all telemetry
            _updateTask = Task.Run(UpdateValuesAsync);
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
        
        /// <summary>
        /// Single polling loop that refreshes all hardware and pushes data to the UI.
        /// Replaces 6 separate uncoordinated loops (1 in ViewModel + 5 in graph controls).
        /// </summary>
        private async Task UpdateValuesAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await _updateLock.WaitAsync(_cts.Token);
                try
                {
                    // Single hardware sensor refresh for all components
                    HardwareMonitorService.Instance.UpdateAll();

                    _cpu.Refresh();
                    _gpu.Refresh();
                    _motherboard.Refresh();
                    _storage.Refresh();
                    
                    // Push all values to UI thread in one batch
                    Dispatcher.UIThread.Post(() =>
                    {
                        // String values for text display
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

                        // Raw numeric values for graph controls
                        CpuTempValue = _cpu.TemperatureValue;
                        CpuClockValue = _cpu.TotalClockSpeedGHz;
                        GpuTempValue = _gpu.TemperatureValue;
                        GpuClockValue = _gpu.CoreClockSpeed;
                        MbTempValue = _motherboard.TemperatureValue;
                    });
                }
                finally
                {
                    _updateLock.Release();
                }

                try
                {
                    await Task.Delay(1000, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cts.Cancel();
            try { _updateTask?.Wait(TimeSpan.FromSeconds(2)); } catch { }
            _updateLock.Dispose();
            _cts.Dispose();
            HardwareMonitorService.Instance.Dispose();
        }
    }
}
