using System;
using System.Linq;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Media;
using FuzzyComic.Views;
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

        private string selectedBackgroundColorName;

        /// <summary>
        /// Index in the ComboBox of the selected color.
        /// This is only really used to read the selected color from the Settings.
        /// </summary>
        public string SelectedBackgroundColorName
        {
            get { return this.selectedBackgroundColorName; }
            set { this.RaiseAndSetIfChanged(ref this.selectedBackgroundColorName, value); }
        }

        public Color selectedBackgroundColor;

        /// <summary>
        /// Selected background color for the main window
        /// </summary>
        public Color SelectedBackgroundColor
        {
            get { return this.selectedBackgroundColor; }
            set { this.RaiseAndSetIfChanged(ref this.selectedBackgroundColor, value); }
        }

        /// <summary>
        /// Given a Settings object, will apply all of its settings to this view model
        /// </summary>
        /// <param name="settings">Settings to apply</param>
        public void ApplySettings(Settings settings)
        {
            this.IsFullScreen = settings.isFullScreen.Value;
            this.SelectedBackgroundColorName = settings.backgroundColor;
            this.SelectedBackgroundColor = OptionsWindow.BackgroundColors[this.SelectedBackgroundColorName];
        }

        /// <summary> Cancel options changes </summary>
        private void RunCancel()
        {
            // re-apply the un-saved settings we had before opening the window
            this.ApplySettings(UserSettings.CurrentSettings);
            CloseOptionsWindow();
        }

        /// <summary> Save options changes </summary>
        private async void RunSave()
        {
            UserSettings.CurrentSettings.isFullScreen = this.IsFullScreen;
            UserSettings.CurrentSettings.backgroundColor = this.SelectedBackgroundColorName;
            await UserSettings.SaveToFile();
            CloseOptionsWindow();
        }
    }
}