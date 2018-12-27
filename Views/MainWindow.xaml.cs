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
            DataContextChanged += (object sender, EventArgs wat) =>
            {
                ((MainWindowViewModel)this.DataContext).OpenComicButton = openComicButton;
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}