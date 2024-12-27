using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;

public abstract class HardwareMonitoringGraph : UserControl
{
    protected ObservableCollection<ISeries> _series;
    protected LineSeries<ObservablePoint> _lineSeries;
    protected CancellationTokenSource _cancellationTokenSource;
    protected int _timeCounter = 0;
    protected ObservableCollection<ObservablePoint> _temperatureData;
    protected int _errorCount = 0;
    protected const int MAX_ERROR_COUNT = 3;

    protected abstract string GetCurrentValue();
    protected abstract string YAxisLabel { get; }
    protected abstract double ParseValue(string value);
    protected abstract void RefreshHardware();
    protected abstract bool IsHardwareInitialized { get; }

    protected virtual async Task UpdateValuesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!IsHardwareInitialized)
                {
                    _errorCount++;
                    if (_errorCount >= MAX_ERROR_COUNT)
                    {
                        Console.WriteLine("Hardware initialization failed multiple times. Stopping monitoring.");
                        break;
                    }

                    await Task.Delay(2000, cancellationToken);
                    continue;
                }

                RefreshHardware();
                var currentValue = GetCurrentValue();

                if (string.IsNullOrEmpty(currentValue))
                {
                    await Task.Delay(500, cancellationToken);
                    continue;
                }

                var parsedValue = ParseValue(currentValue);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _temperatureData.Add(new ObservablePoint(_timeCounter, parsedValue));
                    if (_temperatureData.Count > 20) _temperatureData.RemoveAt(0);
                    _timeCounter++;
                });

                _errorCount = 0; // Reset error count on successful update
            }
            catch (Exception ex)
            {
                _errorCount++;
                Console.WriteLine($"Error updating values: {ex.Message}\nStack trace: {ex.StackTrace}");

                if (_errorCount >= MAX_ERROR_COUNT)
                {
                    Console.WriteLine("Too many errors occurred. Stopping monitoring.");
                    break;
                }

                await Task.Delay(1000, cancellationToken);
            }

            try
            {
                await Task.Delay(500, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}