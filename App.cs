using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using MoRoC.ViewModels;
using MoRoC.Views;

namespace MoRoC
{
    public class App : Application
    {
        public override void Initialize()
        {
            var fluentTheme = new FluentTheme();
            Styles.Add(fluentTheme);

            RequestedThemeVariant = ThemeVariant.Dark;

            Resources.Add("AppBackground", Avalonia.Media.SolidColorBrush.Parse("#192432"));
            Resources.Add("AppPrimary", Avalonia.Media.SolidColorBrush.Parse("#94573B"));
            Resources.Add("AppBorder", Avalonia.Media.SolidColorBrush.Parse("#C8936E"));
            Resources.Add("AppCardBackground", Avalonia.Media.SolidColorBrush.Parse("#1E2B3C"));
            Resources.Add("AppHighlight", Avalonia.Media.SolidColorBrush.Parse("#EBE2CD"));

            // Gradient
            var gradientStops = new Avalonia.Media.GradientStops
            {
                new Avalonia.Media.GradientStop(Avalonia.Media.Color.Parse("#B47556"), 0),
                new Avalonia.Media.GradientStop(Avalonia.Media.Color.Parse("#7A442E"), 1)
            };
            var linearGradientBrush = new Avalonia.Media.LinearGradientBrush
            {
                StartPoint = new Avalonia.RelativePoint(0, 0, Avalonia.RelativeUnit.Relative),
                EndPoint = new Avalonia.RelativePoint(0, 1, Avalonia.RelativeUnit.Relative),
                GradientStops = gradientStops
            };
            Resources.Add("AppButtonGradient", linearGradientBrush);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}