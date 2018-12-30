using System;
using Avalonia;
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
            var navigationButtonsContainer = this.FindControl<Grid>("navigationButtonsContainer");
            var mainMenuPanel = this.FindControl<Border>("mainMenu");
            this.DataContextChanged += (object sender, EventArgs wat) =>
            {
                MainWindowViewModel viewModel = (MainWindowViewModel)this.DataContext;

                // tell the view model about our buttons so it can manipulate their classes
                viewModel.NavigationButtonsContainer = navigationButtonsContainer;
                viewModel.MainMenuPanel = mainMenuPanel;
            };

            // we want this to open on double tap, so we add it here instead of binding in the XAML
            var showMainMenuButton = this.FindControl<Button>("showMainMenuButton");
            showMainMenuButton.DoubleTapped += (object sender, RoutedEventArgs wat) =>
                {
                    if (this.DataContext != null)
                    {
                        ((MainWindowViewModel)this.DataContext).RunOpenMainMenu();
                    }
                };

            this.KeyUp += async (object sender, KeyEventArgs args) =>
            {
                if (this.DataContext != null)
                {
                    MainWindowViewModel viewModel = (MainWindowViewModel)this.DataContext;

                    switch (args.Key)
                    {
                        case Key.Right:
                        case Key.Space:
                            await viewModel.RunNextPage();
                            break;
                        case Key.Left:
                        case Key.Back:
                            await viewModel.RunPreviousPage();
                            break;
                        case Key.Escape:
                            viewModel.RunToggleMainMenu();
                            break;
                        default:
                            break;
                    }
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}