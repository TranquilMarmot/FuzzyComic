using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace FuzzyComic.ViewModels
{
    public class PageViewModel : ReactiveObject
    {
        private Bitmap bitmap;

        public Bitmap CurrentPage
        {
            get { return bitmap; }
            set { this.RaiseAndSetIfChanged(ref this.bitmap, value); }
        }
    }
}
