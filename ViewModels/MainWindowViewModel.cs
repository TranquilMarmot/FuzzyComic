using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using ReactiveUI;
using SharpCompress.Archives;
using SkiaSharp;

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

            CurrentPage = new PageViewModel();

            this.DetachedFromLogicalTree += (object sender, LogicalTreeAttachmentEventArgs args) => CloseStreams();
        }

        /// <summary> Opens a file dialog to pick a comic </summary>
        public ReactiveCommand<Unit, Unit> DoOpenComicFile { get; }

        /// <summary> Quits the application </summary>
        public ReactiveCommand<Unit, Unit> DoExit { get; }

        /// <summary> Turn the page </summary>
        public ReactiveCommand<Unit, Unit> DoNextPage { get; }

        /// <summary> Go back a page </summary>
        public ReactiveCommand<Unit, Unit> DoPreviousPage { get; }

        /// <summary> The current page being displayed </summary>
        public PageViewModel CurrentPage { get; private set; }

        /// <summary> File stream for the currently open file </summary>
        private FileStream CurrentFileSteam { get; set; }

        /// <summary> Archive for the CurrentFileStream </summary>
        private IArchive CurrentArchive { get; set; }

        /// <summary> List of entries (pages) in the archive, sorted alphanumerically </summary>
        private List<IArchiveEntry> CurrentEntryList { get; set; }

        /// <summary> Index in the CurrentEntryList of the CurrentPage </summary>
        private int CurrentPageIndex { get; set; }

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

        /// <summary>
        /// Close the file and archive streams
        /// </summary>
        private void CloseStreams()
        {
            if (CurrentArchive != null)
            {
                CurrentArchive.Dispose();
                CurrentArchive = null;
            }

            if (CurrentFileSteam != null)
            {
                CurrentFileSteam.Dispose();
                CurrentFileSteam = null;
            }
        }

        private void RunExit()
        {
            System.Environment.Exit(0);
        }

        private async Task RunOpenComicFile()
        {
            CloseStreams();

            var dialog = new OpenFileDialog();
            dialog.Title = "Pick a comic";
            dialog.AllowMultiple = false;
            dialog.Filters.Add(new FileDialogFilter() { Name = "Comic Book Archive", Extensions = { "cbz", "cbr" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "Image Archive", Extensions = { "zip", "rar" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });

            var result = await dialog.ShowAsync();
            if (result != null)
            {
                var chosenPath = result[0];

                // open the file and sort the entries
                CurrentFileSteam = File.OpenRead(chosenPath);
                CurrentArchive = ArchiveFactory.Open(CurrentFileSteam);
                CurrentEntryList = EntriesToSortedList(CurrentArchive.Entries);

                // load the first page
                CurrentPageIndex = 0;
                CurrentPage.CurrentImage = await LoadPage(CurrentPageIndex);

                // hide all of the buttons
                OpenComicButton.Classes.Add("invisible");
                PreviousPageButton.Classes.Add("invisible");
                NextPageButton.Classes.Add("invisible");
            }
        }

        /// <summary> Go to the next page </summary>
        private async Task RunNextPage()
        {
            CurrentPageIndex++;
            CurrentPage.CurrentImage = await LoadPage(CurrentPageIndex);
        }

        /// <summary> Go to the previous page </summary>
        private async Task RunPreviousPage()
        {
            CurrentPageIndex--;
            CurrentPage.CurrentImage = await LoadPage(CurrentPageIndex);
        }

        /// <summary>
        /// Load the page at the given index in the sorted entry list
        /// </summary>
        /// <param name="index">Index of page to load</param>
        /// <returns>Bitmap of image at index</returns>
        async Task<Bitmap> LoadPage(int index)
        {
            var entry = CurrentEntryList[index];
            using (var entryStream = entry.OpenEntryStream())
            {
                return await DecodeEntryStream(entryStream);
            }
        }

        /// <summary>
        /// Takes a list of entries in an archive and sorts them alphanumerically into a list
        /// </summary>
        /// <param name="entries">List of entries in arvhie</param>
        /// <returns>Sorted list of entries</returns>
        List<IArchiveEntry> EntriesToSortedList(IEnumerable<IArchiveEntry> entries)
        {
            var list = new List<IArchiveEntry>();
            foreach (var entry in entries)
            {
                if (!entry.IsDirectory)
                {
                    list.Add(entry);
                }
            }

            list.Sort((a, b) => a.Key.CompareTo(b.Key));
            return list;
        }

        /// <summary>
        /// Decodes a stream of an entry from an archive into a Bitmap that Avalonia can use
        /// </summary>
        /// <param name="entryStream">Stream to decode</param>
        /// <returns>Task with finished decoded Bitmap</returns>
        async Task<Bitmap> DecodeEntryStream(Stream entryStream)
        {
            return await Task.Run(() =>
            {
                // SkiaSharp is the underlying image library that Avalonia uses, so we use that here
                // First, we have to decode the image into a Skia bitmap
                // Then, re-encode that into a Skia image (this is in case i.e. we have bmp or jpeg and need png)
                // Then create a bitmap from the stream of that encoded image...
                // This isn't the most efficient thing ever, but it makes it so that we always have a compatible format
                var skiaBitmap = SKBitmap.Decode(entryStream); // TODO this returns null on error
                var skiaImage = SKImage.FromBitmap(skiaBitmap);
                var encoded = skiaImage.Encode();

                var bitmap = new Bitmap(encoded.AsStream());
                return Task.FromResult(bitmap);
            });
        }
    }
}