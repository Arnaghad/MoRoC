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
using MoRoC.Classes;
using LiveChartsCore.Defaults;

namespace MoRoC.Classes;

public class CpuTemperatureGraph : UserControl
{
    private ObservableCollection<ISeries> _series;
    private LineSeries<ObservablePoint> _lineSeries;
    private CPU _cpu;
    private CancellationTokenSource _cancellationTokenSource;
    private int _timeCounter = 0;
    private ObservableCollection<ObservablePoint> _temperatureData;

    public CpuTemperatureGraph()
    {
        _cpu = new CPU();

        _temperatureData = new ObservableCollection<ObservablePoint>();

        _lineSeries = new LineSeries<ObservablePoint>
        {
            Values = _temperatureData,
            Fill = null,
            Stroke = new SolidColorPaint(SKColor.Parse("#94573B")) { StrokeThickness = 3 },
            GeometryFill = new SolidColorPaint(SKColor.Parse("#192432")),
            GeometryStroke = new SolidColorPaint(SKColor.Parse("#C8936E")) { StrokeThickness = 2 },
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
                        Color = SKColor.Parse("#C8936E")
                    },
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#EBE2CD")),
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
                        Color = SKColor.Parse("#C8936E")
                    },
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#EBE2CD")),
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
                _cpu.Refresh();

                if (double.TryParse(_cpu.Temperature?.Replace(" °C", ""), out double currentTemperature))
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
                    Console.WriteLine("Temperature data is not available.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating temperatures: {ex.Message}");
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