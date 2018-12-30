using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FuzzyComic.ViewModels;

namespace FuzzyComic.Views
{
    public class OptionsWindow : Window
    {
        public OptionsWindow(OptionsViewModel dataContext)
        {
            this.DataContext = dataContext;
            this.InitializeComponent();
            this.Topmost = true;
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}