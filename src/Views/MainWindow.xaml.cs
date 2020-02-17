using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FuzzyComic.ViewModels;

namespace FuzzyComic.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContextChanged += this.InitializeDataContext;
            this.KeyUp += this.HandleKeyUp;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Handle keypresses on the main window
        /// (i.e. for changing pages)
        /// </summary>
        private async void HandleKeyUp(object sender, KeyEventArgs args)
        {
            if (this.DataContext != null)
            {
                MainWindowViewModel viewModel = (MainWindowViewModel)this.DataContext;

                switch (args.Key)
                {
                    // space/back always go forward/backward
                    case Key.Space:
                        await viewModel.RunNextPage();
                        break;
                    case Key.Back:
                        await viewModel.RunPreviousPage();
                        break;

                    // depending on manga mode, left/right arrows change how the page changes
                    case Key.Right:
                        if (viewModel.CurrentComic.MangaMode)
                        {
                            await viewModel.RunPreviousPage();
                        }
                        else
                        {
                            await viewModel.RunNextPage();
                        }
                        break;
                    case Key.Left:
                        if (viewModel.CurrentComic.MangaMode)
                        {
                            await viewModel.RunNextPage();
                        }
                        else
                        {
                            await viewModel.RunPreviousPage();
                        }
                        break;
                    case Key.Escape:
                        viewModel.RunToggleMainMenu();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary> Initialize the data context and link the required controls </summary>
        private void InitializeDataContext(object sender, EventArgs args)
        {
            var navigationButtonsContainer = this.FindControl<Grid>("navigationButtonsContainer");
            var mainMenuPanel = this.FindControl<Border>("mainMenu");
            var showMainMenuButton = this.FindControl<Button>("showMainMenuButton");
            var loadingSpinner = this.FindControl<Image>("loadingSpinner");

            MainWindowViewModel viewModel = (MainWindowViewModel)this.DataContext;

            // tell the view model about our buttons so it can manipulate their classes etc.
            viewModel.NavigationButtonsContainer = navigationButtonsContainer;
            viewModel.MainMenuPanel = mainMenuPanel;
            viewModel.LoadingSpinner = loadingSpinner;

            // apply the current user settings (loaded from file when application starts)
            viewModel.CurrentOptions.MainWindow = this;
            viewModel.CurrentOptions.ApplySettings(UserSettings.CurrentSettings);

            // we want this to open on double tap, so we add it here instead of binding in the XAML
            showMainMenuButton.DoubleTapped += (object doubleTappedSender, RoutedEventArgs doubleTappedArgs) =>
                {
                    viewModel.RunOpenMainMenu();
                };
        }
    }
}