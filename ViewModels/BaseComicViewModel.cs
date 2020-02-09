using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace FuzzyComic.ViewModels
{
    /// <summary>
    /// Represents a comic that is being displayed
    /// </summary>
    public abstract class BaseComicViewModel : ReactiveObject
    {
        /// <summary>
        /// Load a specific page number and get back a bitmap image to display
        /// </summary>
        /// <param name="pageNumber">Page number to load</param>
        /// <returns>Bitmap of the page</returns>
        protected abstract Task<Bitmap> LoadPage(int pageNumber);

        /// <summary>
        /// Called whenever the current page number changes;
        /// </summary>
        protected abstract void UpdateProgressBarWidth();

        /// <summary>
        /// Called when the ViewModel is destoroyed; should clean up any open file handles, etc.
        /// </summary>
        public abstract void CloseStreams();

        /// <summary> Index in the CurrentEntryList of the CurrentPage </summary>
        public int CurrentPageIndex { get; set; }

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
    }
}