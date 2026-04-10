using System;
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

            // DataContext is set by App.OnFrameworkInitializationCompleted().
            // Do NOT create a second MainWindowViewModel here — that was
            // doubling all hardware instances and background threads.
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
            base.OnClosed(e);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
