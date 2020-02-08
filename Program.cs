using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using FuzzyComic.Views;

namespace FuzzyComic
{
    class Program
    {
        // The entry point. Things aren't ready yet, so at this point
        // you shouldn't use any Avalonia types or anything that expects
        // a SynchronizationContext to be ready
        public static void Main(string[] args)
        {
            UserSettings.CurrentSettings = UserSettings.LoadFromFile();

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToDebug();
    }
}
