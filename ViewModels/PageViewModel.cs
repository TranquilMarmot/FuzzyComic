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

        public Bitmap CurrentImage
        {
            get { return bitmap; }
            set
            {
                if (this.bitmap != null)
                {
                    this.bitmap.Dispose();
                }

                this.RaiseAndSetIfChanged(ref this.bitmap, value);
            }
        }
    }
}
