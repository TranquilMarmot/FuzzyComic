using System;
using Avalonia;
using Avalonia.Controls;
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
            DataContextChanged += (object sender, EventArgs wat) =>
            {
                MainWindowViewModel viewModel = (MainWindowViewModel)this.DataContext;

                // tell the view model about our buttons so it can manipulate their classes
                viewModel.OpenComicButton = openComicButton;
                viewModel.PreviousPageButton = previousPageButton;
                viewModel.NextPageButton = nextPageButton;
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}