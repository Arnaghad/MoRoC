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

namespace MoRoC.Classes;
public class GpuClockSpeedsGraph : UserControl
{
    private ObservableCollection<ISeries> _series;
    private GPU _gpu;
    private CancellationTokenSource _cancellationTokenSource;
    private int _currentIndex = 0;

    public GpuClockSpeedsGraph()
    {
        // Ініціалізація об'єкта GPU
        _gpu = new GPU();

        // Створюємо серію з індексованими значеннями
        var values = new ObservableCollection<LiveChartsCore.Kernel.Coordinate>();
        
        var lineSeries = new LineSeries<LiveChartsCore.Kernel.Coordinate>
        {
            Values = values,
            Fill = null,
            Mapping = (coordinate, index) => coordinate
        };

        _series = new ObservableCollection<ISeries> { lineSeries };

        // Додаємо підписи до осей
        var chart = new CartesianChart
        {
            Series = _series,
            XAxes = new[]
            {
                new Axis
                {
                    Name = "Time",
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
                    Name = "Core Clock (MHz)",
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

        // Створюємо токен для відміни задачі
        _cancellationTokenSource = new CancellationTokenSource();

        // Запускаємо задачу для оновлення даних
        Task.Run(() => UpdateClockSpeedsAsync(_cancellationTokenSource.Token));
    }

    private async Task UpdateClockSpeedsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Оновлюємо об'єкт GPU
                _gpu.Refresh();

                // Отримуємо значення CoreClockSpeed
                double currentSpeed = _gpu.CoreClockSpeed;

                // Оновлення UI в основному потоці
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var values = ((LineSeries<LiveChartsCore.Kernel.Coordinate>)_series[0]).Values 
                        as ObservableCollection<LiveChartsCore.Kernel.Coordinate>;
                        
                    values.Add(new LiveChartsCore.Kernel.Coordinate(_currentIndex, currentSpeed));

                    if (values.Count > 20) // Лімітуємо кількість точок на графіку
                    {
                        values.RemoveAt(0);
                    }

                    _currentIndex++; // Збільшуємо індекс незалежно від видалення старих значень
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating GPU clock speeds: {ex.Message}");
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
        Task.Run(() => UpdateClockSpeedsAsync(_cancellationTokenSource.Token));
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _cancellationTokenSource?.Cancel();
    }
}