using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using FuzzyComic.ViewModels;
using FuzzyComic.Views;

namespace FuzzyComic
{
    class Program
    {
        static void Main(string[] args)
        {
            UserSettings.CurrentSettings = UserSettings.LoadFromFile();

            BuildAvaloniaApp().Start<MainWindow>(() => new MainWindowViewModel());
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToDebug();
    }
}
