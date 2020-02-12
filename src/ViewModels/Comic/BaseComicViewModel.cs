using System.Reactive;
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
        /// <summary> Column in grid of the main left button (default previous button placement) </summary>
        private static readonly int LeftMainButtonColumn = 0;

        /// <summary> Column in grid of the main right button (default next button placement) </summary>
        private static readonly int RightMainButtonColumn = 2;

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

        /// <summary> Path that this comic lives at </summary>
        public string FilePath { get; set; }

        private int currentPageIndex;

        /// <summary> Index of the page currently being displayed </summary>
        public int CurrentPageIndex
        {
            get { return this.currentPageIndex; }
            set
            {
                this.RaiseAndSetIfChanged(ref this.currentPageIndex, value);
            }
        }

        private int totalPages;

        /// <summary>Total number of pages in the comic</summary>
        public int TotalPages
        {
            get { return this.totalPages; }
            set
            {
                this.RaiseAndSetIfChanged(ref this.totalPages, value);
            }
        }


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

        private int previousPageColumn = LeftMainButtonColumn;

        /// <summary>
        /// Grid column that previous page button is in.
        /// Swapped with next page column for manga mode.
        /// </summary>
        public int PreviousPageColumn
        {
            get { return this.previousPageColumn; }
            set
            {
                this.RaiseAndSetIfChanged(ref this.previousPageColumn, value);
            }
        }

        private int nextPageColumn = RightMainButtonColumn;

        /// <summary>
        /// Grid column that next page button is in.
        /// Swapped with previous page column for manga mode.
        /// </summary>
        public int NextPageColumn
        {
            get { return this.nextPageColumn; }
            set
            {
                this.RaiseAndSetIfChanged(ref this.nextPageColumn, value);
            }
        }

        private bool mangaMode;

        /// <summary>
        /// Whether or not to swap the previous/next buttons
        /// </summary>
        public bool MangaMode
        {
            get { return this.mangaMode; }
            set
            {
                this.RaiseAndSetIfChanged(ref this.mangaMode, value);

                PreviousPageColumn = value ? RightMainButtonColumn : LeftMainButtonColumn;
                NextPageColumn = value ? LeftMainButtonColumn : RightMainButtonColumn;

            }
        }

        /// <summary> Go to a specified page </summary>
        public ReactiveCommand<Unit, Task> DoGoToPage { get; }

        /// <summary>
        /// Create a new comic view model.
        /// 
        /// After this is called, the `Open` function should be called afterward to actually load the comic.
        /// </summary>
        /// <param name="filePath"></param>
        protected BaseComicViewModel(string filePath)
        {
            FilePath = filePath;
            DoGoToPage = ReactiveCommand.Create(RunGoToCurrentPage);
        }

        /// <summary>
        /// Will open a comic to the last page it was on, or at the beginning if it's never been opened before.
        /// </summary>
        public async Task Open()
        {
            ComicInfo currentInfo;
            if (UserSettings.CurrentSettings.comicList.TryGetValue(FilePath, out currentInfo))
            {
                // we've opened this comic before; go to the page we stopped on
                await GoToPage(currentInfo.PageNumber);
            }
            else
            {
                // fresh one, start at the beginning
                await GoToPage(0);
            }

            MangaMode = currentInfo.MangaMode;
        }

        /// <summary> Run when the "Go" to page button is clicked </summary>
        private async Task RunGoToCurrentPage()
        {
            await GoToPage(CurrentPageIndex);
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

            ComicInfo currentInfo;
            if (!UserSettings.CurrentSettings.comicList.TryGetValue(FilePath, out currentInfo))
            {
                currentInfo = new ComicInfo();
            }

            currentInfo.PageNumber = page;
            currentInfo.MangaMode = MangaMode;
            UserSettings.CurrentSettings.comicList[FilePath] = currentInfo;
            await UserSettings.SaveToFile();
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
