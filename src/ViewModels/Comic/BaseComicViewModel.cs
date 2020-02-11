using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace FuzzyComic.ViewModels.Comic
{
    /// <summary> Represents a comic that is being displayed </summary>
    public abstract class BaseComicViewModel : ReactiveObject
    {
        /// <summary>
        /// Load a specific page number and get back a bitmap image to display
        /// </summary>
        /// <param name="pageNumber">Page number to load</param>
        /// <returns>Bitmap of the page</returns>
        protected abstract Task<Bitmap> LoadPage(int pageNumber);

        /// <summary>
        /// Called when the ViewModel is destroyed; should clean up any open file handles, etc.
        /// </summary>
        public abstract void CloseStreams();

        /// <summary> Index of the page currently being displayed </summary>
        public int CurrentPageIndex { get; set; }

        /// <summary>Total number of pages in the comic</summary>
        public int TotalPages { get; set; }

        /// <summary>Image of the current page being displayed</summary>
        private Bitmap currentPageBitmap;

        /// <summary>Image of the current page being displayed</summary>
        public Bitmap CurrentPage
        {
            get { return this.currentPageBitmap; }
            protected set
            {
                if (this.currentPageBitmap != null)
                {
                    // clean up after ourselves...
                    this.currentPageBitmap.Dispose();
                }

                // this will tell ReactiveUI to reload the page
                this.RaiseAndSetIfChanged(ref this.currentPageBitmap, value);
            }
        }

        private double progressBarWidth;

        /// <summary>
        /// Width, in pixels, of the progress bar. Will be updated whenever the page changes.
        /// </summary>
        public double ProgressBarWidth
        {
            get { return this.progressBarWidth; }
            protected set
            {
                this.RaiseAndSetIfChanged(ref this.progressBarWidth, value);
            }
        }

        /// <summary>
        /// Go to a specific page
        /// </summary>
        /// <param name="page">Page number to go to</param>
        public async Task GoToPage(int page)
        {
            CurrentPageIndex = page;
            CurrentPage = await LoadPage(CurrentPageIndex);

            UpdateProgressBarWidth();
        }

        /// <summary>
        /// Update the width of the progress bar at the bottom of the page.
        /// 
        /// This should be called whenever the page changes.
        /// </summary>
        private void UpdateProgressBarWidth()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var windowWidth = desktop.MainWindow.Width; // TODO On some platforms this will be NaN at startup?
                var percentDone = (double)CurrentPageIndex / (double)TotalPages;
                ProgressBarWidth = windowWidth * percentDone;
            }
        }
    }
}
