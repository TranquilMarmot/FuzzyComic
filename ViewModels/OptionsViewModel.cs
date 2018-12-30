

using System;
using Avalonia.Controls;
using ReactiveUI;

namespace FuzzyComic.ViewModels
{
    public class OptionsViewModel : ReactiveObject
    {
        private bool showWindowDecorations = true;

        public bool ShowWindowDecorations
        {
            get { return this.showWindowDecorations; }
            private set { this.RaiseAndSetIfChanged(ref this.showWindowDecorations, value); }
        }

        private bool isFullScreen = false;
        public bool IsFullScreen
        {
            get { return this.isFullScreen; }
            set
            {
                this.RaiseAndSetIfChanged(ref this.isFullScreen, value);
                this.ShowWindowDecorations = this.isFullScreen ? false : true;

                // I tried binding this in the XAML but it seems like changing it didn't work
                // Setting it directly on the window, however, works fine?
                Avalonia.Application.Current.MainWindow.WindowState = this.isFullScreen ? WindowState.Maximized : WindowState.Normal;
            }
        }
    }
}