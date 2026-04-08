using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MoRoC.ViewModels;

namespace MoRoC.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            #if DEBUG
            this.AttachDevTools();
            #endif

            // string executableDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // string iconPath = Path.Combine(executableDirectory, "MoRoC.png");
            // if (File.Exists(iconPath)) 
            // {
            //     this.Icon = new WindowIcon(iconPath);
            // }

            DataContext = new MainWindowViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
