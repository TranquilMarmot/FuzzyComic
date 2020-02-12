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
            new FileDialogFilter() { Name = "Supported Formats", Extensions = { "cbz", "cbr", "zip", "rar", "pdf" } },
            new FileDialogFilter() { Name = "Comic Book Archive", Extensions = { "cbz", "cbr" } },
            new FileDialogFilter() { Name = "Image Archive", Extensions = { "zip", "rar" } },
            new FileDialogFilter() { Name = "PDF", Extensions = { "pdf" } },
            new FileDialogFilter() { Name = "All", Extensions = { "*" } }
        });

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

        /// <summary> Close the main meunu </summary>
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

        /// <summary>
        /// Container with the navigation buttons in it
        /// Opacity of this is set to 0 when a comic is opened
        /// </summary>
        public Grid NavigationButtonsContainer { get; set; }

        /// <summary>
        /// Container for the main menu items
        /// </summary>
        public Border MainMenuPanel { get; set; }

        public OptionsViewModel CurrentOptions { get; set; }

        /// <summary> Exits the application </summary>
        private void RunExit()
        {
            // TODO show confirmation dialog
            System.Environment.Exit(0);
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

                    await CurrentComic.Open();

                    // hide all of the buttons; we do this via opacity 0 via styles
                    // so that they can still be hit
                    NavigationButtonsContainer.Classes.Add("invisible");

                    // also, make sure the main menu is closed
                    this.RunCloseMainMenu();
                }
            }
        }

        /// <summary> Go to the next page </summary>
        public async Task RunNextPage()
        {
            await CurrentComic.GoToPage(CurrentComic.CurrentPageIndex + 1);
        }

        /// <summary> Go to the previous page </summary>
        public async Task RunPreviousPage()
        {
            await CurrentComic.GoToPage(CurrentComic.CurrentPageIndex - 1);
        }

        /// <summary> Open the main manu </summary>
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
    }
}