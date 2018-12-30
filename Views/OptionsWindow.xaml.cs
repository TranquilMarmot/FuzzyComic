using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FuzzyComic.ViewModels;

namespace FuzzyComic.Views
{
    public class OptionsWindow : Window
    {
        public OptionsWindow(OptionsWindowViewModel dataContext)
        {
            this.DataContext = dataContext;
            this.InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}