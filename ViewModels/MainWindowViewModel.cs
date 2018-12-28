

using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using ReactiveUI;

namespace FuzzyComic.ViewModels
{
    public class MainWindowViewModel : Control
    {
        public MainWindowViewModel()
        {
            DoExit = ReactiveCommand.Create(RunExit);
            DoOpenComicFile = ReactiveCommand.CreateFromTask(RunOpenComicFile);
            DoNextPage = ReactiveCommand.CreateFromTask(RunNextPage);
            DoPreviousPage = ReactiveCommand.CreateFromTask(RunPreviousPage);

            CurrentComic = new ComicViewModel();

            this.DetachedFromLogicalTree += (object sender, LogicalTreeAttachmentEventArgs args) => CurrentComic.CloseStreams();
        }

        /// <summary> Opens a file dialog to pick a comic </summary>
        public ReactiveCommand<Unit, Unit> DoOpenComicFile { get; }

        /// <summary> Quits the application </summary>
        public ReactiveCommand<Unit, Unit> DoExit { get; }

        /// <summary> Turn the page </summary>
        public ReactiveCommand<Unit, Unit> DoNextPage { get; }

        /// <summary> Go back a page </summary>
        public ReactiveCommand<Unit, Unit> DoPreviousPage { get; }

        public ComicViewModel CurrentComic { get; set; }

        /// <summary>
        /// Button to open the menu
        /// This gets set by the window when it gets the data context
        /// </summary>
        public Button OpenComicButton { get; set; }

        /// <summary>
        /// Button to go to the previous page
        /// This gets set by the window when it gets the data context
        /// </summary>
        public Button PreviousPageButton { get; set; }

        /// <summary>
        /// Button to go to the next page
        /// This gets set by the window when it gets the data context
        /// </summary>
        public Button NextPageButton { get; set; }

        private void RunExit()
        {
            System.Environment.Exit(0);
        }

        private async Task RunOpenComicFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Pick a comic";
            dialog.AllowMultiple = false;
            dialog.Filters.Add(new FileDialogFilter() { Name = "Comic Book Archive", Extensions = { "cbz", "cbr" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "Image Archive", Extensions = { "zip", "rar" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });

            var result = await dialog.ShowAsync();
            if (result != null)
            {
                // open the chosen file
                var chosenPath = result[0];
                await CurrentComic.LoadArchive(chosenPath);

                // hide all of the buttons
                OpenComicButton.Classes.Add("invisible");
                PreviousPageButton.Classes.Add("invisible");
                NextPageButton.Classes.Add("invisible");
            }
        }

        /// <summary> Go to the next page </summary>
        private async Task RunNextPage()
        {
            await CurrentComic.GoToPage(CurrentComic.CurrentPageIndex + 1);
        }

        /// <summary> Go to the previous page </summary>
        private async Task RunPreviousPage()
        {
            await CurrentComic.GoToPage(CurrentComic.CurrentPageIndex - 1);
        }
    }
}