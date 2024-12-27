using Avalonia.Controls;
using Avalonia.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.Defaults;

namespace MoRoC.Classes;
public class GpuTemperatureGraph : UserControl
{
    private ObservableCollection<ISeries> _series;
    private LineSeries<ObservablePoint> _lineSeries;
    private GPU _gpu;
    private CancellationTokenSource _cancellationTokenSource;
    private int _timeCounter = 0;
    private ObservableCollection<ObservablePoint> _temperatureData;

    public GpuTemperatureGraph()
    {
        _gpu = new GPU();

        _temperatureData = new ObservableCollection<ObservablePoint>();

        _lineSeries = new LineSeries<ObservablePoint>
        {
            Values = _temperatureData,
            Fill = null,
            Mapping = (point, index) => new LiveChartsCore.Kernel.Coordinate(
                point.X ?? 0, 
                point.Y ?? 0
            )
        };

        _series = new ObservableCollection<ISeries> { _lineSeries };

        var chart = new CartesianChart
        {
            Series = _series,
            XAxes = new[]
            {
                new Axis
                {
                    Name = "Time (s)",
                    NamePadding = new LiveChartsCore.Drawing.Padding(0, 15),
                    NamePaint = new SolidColorPaint
                    {
                        Color = new SKColor(255, 255, 255)
                    },
                    LabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255)),
                    TextSize = 12
                }
            },
            YAxes = new[]
            {
                new Axis
                {
                    Name = "Temperature (°C)",
                    NamePadding = new LiveChartsCore.Drawing.Padding(15, 0),
                    NamePaint = new SolidColorPaint
                    {
                        Color = new SKColor(255, 255, 255)
                    },
                    LabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255)),
                    TextSize = 12
                }
            },
            Width = 400,
            Height = 350
        };

        Content = chart;

        _cancellationTokenSource = new CancellationTokenSource();
        Task.Run(() => UpdateTemperaturesAsync(_cancellationTokenSource.Token));
    }

    private async Task UpdateTemperaturesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _gpu.Refresh();

                if (!string.IsNullOrEmpty(_gpu.Temperature) && 
                    double.TryParse(_gpu.Temperature.Replace(" °C", ""), out double currentTemperature))
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _temperatureData.Add(new ObservablePoint(_timeCounter, currentTemperature));

                        if (_temperatureData.Count > 20)
                        {
                            _temperatureData.RemoveAt(0);
                        }
                        
                        _timeCounter++;
                    });
                }
                else
                {
                    Console.WriteLine("GPU temperature data is not available.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating GPU temperatures: {ex.Message}");
            }

            try
            {
                await Task.Delay(1000, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _cancellationTokenSource = new CancellationTokenSource();
        Task.Run(() => UpdateTemperaturesAsync(_cancellationTokenSource.Token));
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _cancellationTokenSource?.Cancel();
    }
}