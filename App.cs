using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Material.Colors;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;
using MoRoC.ViewModels;
using MoRoC.Views;

namespace MoRoC
{
    public class App : Application
    {
        public override void Initialize()
        {
            var materialTheme = new MaterialTheme(null)
            {
                BaseTheme = BaseThemeMode.Dark,
                PrimaryColor = PrimaryColor.Grey
            };

            Styles.Add(materialTheme);
            RequestedThemeVariant = ThemeVariant.Default;
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