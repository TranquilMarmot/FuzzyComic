using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FuzzyComic.ViewModels;

namespace FuzzyComic.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var openComicButton = this.FindControl<Button>("openComicButton");
            var previousPageButton = this.FindControl<Button>("previousPageButton");
            var nextPageButton = this.FindControl<Button>("nextPageButton");
            this.DataContextChanged += (object sender, EventArgs wat) =>
            {
                MainWindowViewModel viewModel = (MainWindowViewModel)this.DataContext;

                // tell the view model about our buttons so it can manipulate their classes
                viewModel.OpenComicButton = openComicButton;
                viewModel.PreviousPageButton = previousPageButton;
                viewModel.NextPageButton = nextPageButton;
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