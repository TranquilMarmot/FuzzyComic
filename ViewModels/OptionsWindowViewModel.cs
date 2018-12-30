

using Avalonia.Controls;
using ReactiveUI;

namespace FuzzyComic.ViewModels
{
    public class OptionsWindowViewModel : ReactiveObject
    {
        private bool showSystemDecorations = false;
        public bool ShowSystemDecorations
        {
            get { return this.showSystemDecorations; }
            set
            {
                this.RaiseAndSetIfChanged(ref this.showSystemDecorations, value);
            }
        }
    }
}