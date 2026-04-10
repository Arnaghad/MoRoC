using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using ScottPlot;
using ScottPlot.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MoRoC.Classes
{
    /// <summary>
    /// Base class for all ScottPlot telemetry graphs.
    /// Provides a bindable <see cref="CurrentValue"/> property.
    /// A 1-second timer continuously adds the latest value and refreshes the plot.
    /// Shows a sliding 20-second window with X-axis labels relative to launch time.
    /// Subclasses only need to call <see cref="InitializePlot"/> with axis labels.
    /// </summary>
    public abstract class BaseGraph : UserControl
    {
        public static readonly StyledProperty<double> CurrentValueProperty =
            AvaloniaProperty.Register<BaseGraph, double>(nameof(CurrentValue), defaultValue: double.NaN);

        public double CurrentValue
        {
            get => GetValue(CurrentValueProperty);
            set => SetValue(CurrentValueProperty, value);
        }

        private AvaPlot? _avaPlot;
        private ScottPlot.Plottables.Scatter? _scatter;
        private DispatcherTimer? _timer;
        private double _lastValue = double.NaN;
        private readonly Stopwatch _stopwatch = new();
        private readonly List<double> _times = new();
        private readonly List<double> _values = new();

        /// <summary>
        /// Width of the visible time window in seconds.
        /// </summary>
        private const int WindowSeconds = 20;

        protected void InitializePlot(string xLabel, string yLabel)
        {
            _avaPlot = new AvaPlot { MinWidth = 200, MinHeight = 150 };

            _avaPlot.Plot.FigureBackground.Color = Color.FromHex(ThemeConstants.Background);
            _avaPlot.Plot.DataBackground.Color = Color.FromHex(ThemeConstants.Background);

            _avaPlot.Plot.Axes.Color(Color.FromHex(ThemeConstants.Border));

            _avaPlot.Plot.Axes.Bottom.Label.Text = xLabel;
            _avaPlot.Plot.Axes.Left.Label.Text = yLabel;

            var grid = _avaPlot.Plot.Grid;
            grid.MajorLineColor = Color.FromHex(ThemeConstants.Border).WithAlpha(50);

            // Initial axis limits — full 20-second window starting from 0
            _avaPlot.Plot.Axes.SetLimits(0, WindowSeconds, 0, 100);

            Content = _avaPlot;

            _stopwatch.Start();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            var val = CurrentValue;
            if (!double.IsNaN(val))
                _lastValue = val;

            if (double.IsNaN(_lastValue) || _avaPlot is null)
                return;

            double elapsed = _stopwatch.Elapsed.TotalSeconds;

            _times.Add(elapsed);
            _values.Add(_lastValue);

            // Trim data points that fell out of the visible window (with a small buffer)
            double cutoff = elapsed - WindowSeconds - 2;
            while (_times.Count > 0 && _times[0] < cutoff)
            {
                _times.RemoveAt(0);
                _values.RemoveAt(0);
            }

            // --- Update the scatter line ---
            if (_scatter is not null)
                _avaPlot.Plot.Remove(_scatter);

            if (_times.Count >= 2)
            {
                _scatter = _avaPlot.Plot.Add.ScatterLine(
                    _times.ToArray(), _values.ToArray());
                _scatter.LineStyle.Color = Color.FromHex(ThemeConstants.Primary);
                _scatter.LineStyle.Width = 3;
            }

            // --- X-axis: sliding window with real elapsed time ---
            // Before WindowSeconds has passed, the window is 0 → WindowSeconds.
            // After that it slides: (elapsed − WindowSeconds) → elapsed.
            double xRight = Math.Max(elapsed, WindowSeconds);
            double xLeft = xRight - WindowSeconds;

            // --- Y-axis: padded so the line never sits on the bottom axis ---
            double yMin = _values.Min();
            double yMax = _values.Max();
            double range = yMax - yMin;
            // At least 2 units of padding to keep the line visible
            double padding = Math.Max(range * 0.2, 2);

            _avaPlot.Plot.Axes.SetLimits(xLeft, xRight, yMin - padding, yMax + padding);

            _avaPlot.Refresh();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == CurrentValueProperty &&
                change.NewValue is double val &&
                !double.IsNaN(val))
            {
                _lastValue = val;
            }
        }
    }
}

