using System.IO;
using System.Reactive;
using System.Threading.Tasks;
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

        /// <summary> Delegate to use when the page change </summary>
        public delegate void HandlePageChanged();

        /// <summary> Event called when the page is changed </summary>
        public event HandlePageChanged OnPageChanged;

        /// <summary> Path that this comic lives at </summary>
        public string FilePath { get; set; }

        /// <summary> Name of the file </summary>
        public string FileName { get; set; }

        /// <summary> Index of the current page in the list of pages </summary>
        public int CurrentPageIndex { get; set; }

        private int enteredPageIndex;

        /// <summary> Page number entered into the textbox of the main menu. Used to navigate to specific pages. </summary>
        public int EnteredPageIndex
        {
            get { return this.enteredPageIndex; }
            set
            {
                this.RaiseAndSetIfChanged(ref this.enteredPageIndex, value);
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

        /// <summary>
        /// After setting `CurrentPage`, the next page will be loaded in the background.
        /// This is mainly useful when using formats where loading pages can take a while (i.e. PDFs).
        /// 
        /// When the next page button is pressed, if this is not null, it will be set to be the `CurrentPage` bitmap.
        /// </summary>
        private Bitmap NextPage;

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
            FileName = Path.GetFileNameWithoutExtension(filePath);

            DoGoToPage = ReactiveCommand.Create(RunGoToCurrentlyEnteredPage);
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

                // setting manga mode from the settings will properly switch the buttons around
                MangaMode = currentInfo.MangaMode;
            }
            else
            {
                // fresh one, start at the beginning
                await GoToPage(0);
            }
        }

        /// <summary>
        /// Run when the "Go" to page button is clicked.
        /// This will go to whatever number the user has entered into the text box on the main menu.
        /// </summary>
        private async Task RunGoToCurrentlyEnteredPage()
        {
            // page index starts at 0, but we want to display it as starting at 1
            await GoToPage(EnteredPageIndex - 1);
        }

        /// <summary>
        /// Go to a specific page
        /// </summary>
        /// <param name="page">Page number to go to</param>
        public async Task GoToPage(int page)
        {
            System.Console.WriteLine($"Going to page {page}...");

            // can't go past the end or into the negatives
            if (page > TotalPages || page < 0)
            {
                System.Console.WriteLine($"{page} is at the beginning or end; doing nothing");
                return;
            }

            // if we have a next page and we're going forward by one (a page flip)
            // then we can just use the next page that we've preloaded
            if (NextPage != null && page == CurrentPageIndex + 1)
            {
                System.Console.WriteLine($"Page {page} already loaded, using it");
                CurrentPage = NextPage;
                NextPage = null;
            }
            else
            {
                System.Console.WriteLine($"Loading page {page}...");
                CurrentPage = await LoadPage(page);
            }

            EnteredPageIndex = page + 1; // update the text box that shows the current page
            CurrentPageIndex = page;

            // save the current page out to the settings file
            await SaveCurrentSettings();

            // if we're NOT at the end, load the next page in the background
            if (CurrentPageIndex < TotalPages - 1)
            {
                System.Console.WriteLine($"Fetching page {CurrentPageIndex + 1} in the background");
                NextPage = await LoadPage(CurrentPageIndex + 1);
            }

            System.Console.WriteLine("---");
            OnPageChanged();
        }

        /// <summary>
        /// Save the current settings for this comic to the settings file.
        /// </summary>
        /// <param name="page">Page to save into settings</param>
        private async Task SaveCurrentSettings()
        {
            ComicInfo currentInfo;
            if (!UserSettings.CurrentSettings.comicList.TryGetValue(FilePath, out currentInfo))
            {
                currentInfo = new ComicInfo();
            }

            currentInfo.PageNumber = CurrentPageIndex;
            currentInfo.MangaMode = MangaMode;
            UserSettings.CurrentSettings.comicList[FilePath] = currentInfo;
            await UserSettings.SaveToFile();
        }
    }
}
