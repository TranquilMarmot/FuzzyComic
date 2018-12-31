

using System;
using System.Reactive;
using Avalonia.Controls;
using ReactiveUI;

namespace FuzzyComic.ViewModels
{
    public class OptionsViewModel : ReactiveObject
    {
        public OptionsViewModel()
        {
            DoCancel = ReactiveCommand.Create(RunCancel);
            DoSave = ReactiveCommand.Create(RunSave);
        }

        /// <summary> Reference to the main window, for resizing etc. </summary>
        public Window MainWindow { get; set; }

        /// <summary> Action to call to close the options window </summary>
        public Action CloseOptionsWindow { get; set; }

        /// <summary> Cancel options changes </summary>
        public ReactiveCommand<Unit, Unit> DoCancel { get; }

        /// <summary> Save options changes </summary>
        public ReactiveCommand<Unit, Unit> DoSave { get; }

        private bool showWindowDecorations = true;

        /// <summary>
        /// Whether or not to show window decorations (i.e. close, minimixe, maximize)
        /// Note that this is not a setting itself, but is controlled by IsFullScreen
        /// </summary>
        public bool ShowWindowDecorations
        {
            get { return this.showWindowDecorations; }
            private set { this.RaiseAndSetIfChanged(ref this.showWindowDecorations, value); }
        }

        private bool isFullScreen = false;

        /// <summary>
        /// Whether or not the window should be full screen
        /// Note: At the time of writing, Avalonia does not have an actual "Fullscreen" window mode,
        ///  so instead this just maximized the window and hides the window decorations...
        /// </summary>
        public bool IsFullScreen
        {
            get { return this.isFullScreen; }
            set
            {
                this.RaiseAndSetIfChanged(ref this.isFullScreen, value);
                this.ShowWindowDecorations = this.isFullScreen ? false : true;

                // I tried binding this in the XAML but it seems like changing it didn't work
                // Setting it directly on the window, however, works fine?
                MainWindow.WindowState = this.isFullScreen ? WindowState.Maximized : WindowState.Normal;
            }
        }

        /// <summary>
        /// Given a Settings object, will apply all of its settings to this view model
        /// </summary>
        /// <param name="settings">Settings to apply</param>
        public void ApplySettings(Settings settings)
        {
            this.IsFullScreen = settings.isFullScreen;
        }

        /// <summary> Copy this view model out to a Settings object </summary>
        /// <returns> Settings object with the settings from this view model </returns>
        public Settings CopyToSettings()
        {
            var settings = new Settings();
            settings.isFullScreen = this.IsFullScreen;
            return settings;
        }

        /// <summary> Cancel options changes </summary>
        private void RunCancel()
        {
            // TODO restore previous settings
            CloseOptionsWindow();
        }

        /// <summary> Save options changes </summary>
        private void RunSave()
        {
            UserSettings.SaveToFile(this.CopyToSettings());
            CloseOptionsWindow();
        }
    }
}