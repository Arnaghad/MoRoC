using System;
using System.IO;
using Avalonia;
using Avalonia.ReactiveUI;

namespace MoRoC
{
    sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw; // Re-throw the exception after logging it
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();

        // Log unhandled exceptions
        private static void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                LogException(exception);
            }
        }

        // Log exceptions to a file
        private static void LogException(Exception ex)
        {
            string logFilePath = "error_log.txt";
            string logMessage = $"[{DateTime.Now}] {ex}\n";
            File.AppendAllText(logFilePath, logMessage);
        }
    }
}