using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using FuzzyComic.ViewModels.Comic;
using FuzzyComic.Views;
using ReactiveUI;

namespace FuzzyComic.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        /// <summary> List of options that get passed to the file open dialog </summary>
        private static readonly List<FileDialogFilter> FileFilterList = new List<FileDialogFilter>(new[] {
            new FileDialogFilter() { Name = "Supported Formats", Extensions = { "cbz", "cbr", "pdf" } },
            new FileDialogFilter() { Name = "Comic Book Archive", Extensions = { "cbz", "cbr" } },
            new FileDialogFilter() { Name = "Image Archive", Extensions = { "zip", "rar" } },
            new FileDialogFilter() { Name = "PDF", Extensions = { "pdf" } },
            new FileDialogFilter() { Name = "All", Extensions = { "*" } }
        });

        private static readonly string FuzzyComic = "Fuzzy Comic";

        public MainWindowViewModel()
        {
            DoExit = ReactiveCommand.Create(RunExit);
            DoCloseMainMenu = ReactiveCommand.Create(RunCloseMainMenu);
            DoShowOptionsMenu = ReactiveCommand.CreateFromTask(RunShowOptionsMenu);
            DoOpenComicFile = ReactiveCommand.CreateFromTask(RunOpenComicFile);
            DoNextPage = ReactiveCommand.CreateFromTask(RunNextPage);
            DoPreviousPage = ReactiveCommand.CreateFromTask(RunPreviousPage);

            CurrentOptions = new OptionsViewModel();
        }

        /// <summary> Opens a file dialog to pick a comic </summary>
        public ReactiveCommand<Unit, Unit> DoOpenComicFile { get; }

        /// <summary> Quits the application </summary>
        public ReactiveCommand<Unit, Unit> DoExit { get; }

        /// <summary> Turn the page </summary>
        public ReactiveCommand<Unit, Unit> DoNextPage { get; }

        /// <summary> Go back a page </summary>
        public ReactiveCommand<Unit, Unit> DoPreviousPage { get; }

        /// <summary> Close the main menu </summary>
        public ReactiveCommand<Unit, Unit> DoCloseMainMenu { get; }

        /// <summary> Open the options dialog </summary>
        public ReactiveCommand<Unit, Unit> DoShowOptionsMenu { get; }

        private BaseComicViewModel currentComic;

        /// <summary>
        /// Handles opening, streaming, reading, etc.false image archives
        /// Also keeps track of the current page
        /// </summary>
        public BaseComicViewModel CurrentComic
        {
            get { return this.currentComic; }
            set
            {
                this.RaiseAndSetIfChanged(ref this.currentComic, value);
            }
        }

        private string windowTitle = FuzzyComic;

        /// <summary> The current title of the main window </summary>
        public string WindowTitle
        {
            get { return this.windowTitle; }
            set
            {
                this.RaiseAndSetIfChanged(ref this.windowTitle, value);
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
        /// Container with the navigation buttons in it
        /// Opacity of this is set to 0 when a comic is opened
        /// </summary>
        public Grid NavigationButtonsContainer { get; set; }

        /// <summary>
        /// Container for the main menu items
        /// </summary>
        public Border MainMenuPanel { get; set; }

        /// <summary> Spinning image to represent loading </summary>
        public Image LoadingSpinner { get; set; }

        /// <summary>
        /// Current options
        /// </summary>
        public OptionsViewModel CurrentOptions { get; set; }

        /// <summary> Exits the application </summary>
        private void RunExit()
        {
            System.Environment.Exit(0);
        }

        /// <summary>
        /// Show/hide the loading spinner
        /// </summary>
        /// <param name="visible">Whether or not the spinner is visible</param>
        private void SetLoadingSpinnerVisible(bool visible)
        {
            if (visible)
            {
                LoadingSpinner.Classes.Remove("invisible");
            }
            else
            {
                LoadingSpinner.Classes.Add("invisible");
            }
        }

        /// <summary>
        /// Opens a file browser to choose a comic, then handles opening the chosen file
        /// </summary>
        private async Task RunOpenComicFile()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var dialog = new OpenFileDialog();
                dialog.Title = "Pick a comic";
                dialog.AllowMultiple = false;
                dialog.Filters.AddRange(FileFilterList);

                var result = await dialog.ShowAsync(desktop.MainWindow);
                if (result != null && result.Length == 1)
                {
                    // open the chosen file
                    var chosenPath = result[0];

                    if (CurrentComic != null)
                    {
                        CurrentComic.CloseStreams();
                        currentComic = null;
                    }

                    // hide all of the buttons; we do this via opacity 0 via styles
                    // so that they can still be hit
                    NavigationButtonsContainer.Classes.Add("invisible");

                    // also, make sure the main menu is closed
                    this.RunCloseMainMenu();

                    // Create the proper view model for the chosen file
                    if (chosenPath.EndsWith(".cbz") || chosenPath.EndsWith(".cbr") || chosenPath.EndsWith(".zip") || chosenPath.EndsWith(".rar"))
                    {
                        CurrentComic = new ArchiveComicViewModel(chosenPath);
                    }
                    else if (chosenPath.EndsWith(".pdf"))
                    {
                        CurrentComic = new PDFComicViewModel(chosenPath);
                    }
                    else
                    {
                        System.Console.Error.WriteLine($"Unsupported format for file {chosenPath}!");
                    }

                    // whenever the page changes, we want to update the window title and progress bar
                    CurrentComic.OnPageChanged += UpdateWindowTitle;
                    CurrentComic.OnPageChanged += UpdateProgressBarWidth;

                    SetLoadingSpinnerVisible(true);
                    await CurrentComic.Open();
                    SetLoadingSpinnerVisible(false);
                }
            }
        }

        /// <summary> Update the title of the window to reflect the current state of the application </summary>
        public void UpdateWindowTitle()
        {
            if (CurrentComic != null)
            {
                WindowTitle = $"{CurrentComic.FileName} | {CurrentComic.CurrentPageIndex + 1} of {CurrentComic.TotalPages} | {FuzzyComic}";
            }
            else
            {
                WindowTitle = FuzzyComic;
            }
        }

        /// <summary> Go to the next page </summary>
        public async Task RunNextPage()
        {
            SetLoadingSpinnerVisible(true);
            await CurrentComic.GoToPage(CurrentComic.CurrentPageIndex + 1);
            SetLoadingSpinnerVisible(false);
        }

        /// <summary> Go to the previous page </summary>
        public async Task RunPreviousPage()
        {
            SetLoadingSpinnerVisible(true);
            await CurrentComic.GoToPage(CurrentComic.CurrentPageIndex - 1);
            SetLoadingSpinnerVisible(false);
        }

        /// <summary> Open the main menu </summary>
        public void RunOpenMainMenu()
        {
            MainMenuPanel.IsVisible = true;
        }

        /// <summary> Close the main menu </summary>
        public void RunCloseMainMenu()
        {
            MainMenuPanel.IsVisible = false;
        }

        /// <summary>
        /// If the main menu is visible, hides it
        /// If it is hidden, shows it.
        /// </summary>
        public void RunToggleMainMenu()
        {
            MainMenuPanel.IsVisible = !MainMenuPanel.IsVisible;
        }

        /// <summary>
        /// Show the options menu
        /// Task finishes when the window is closed
        /// </summary>
        public async Task RunShowOptionsMenu()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var optionsWindow = new OptionsWindow(CurrentOptions);
                await optionsWindow.ShowDialog(desktop.MainWindow);
            }
        }

        /// <summary> Update the width of the progress bar at the bottom of the page. </summary>
        private void UpdateProgressBarWidth()
        {
            if (CurrentComic != null && Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var windowWidth = desktop.MainWindow.Width; // TODO On some platforms this will be NaN at startup?
                var percentDone = (double)(CurrentComic.CurrentPageIndex + 1) / (double)CurrentComic.TotalPages;
                ProgressBarWidth = windowWidth * percentDone;
            }
        }
    }
}