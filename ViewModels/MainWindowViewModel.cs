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

            CurrentPage = new PageViewModel();

            this.DetachedFromLogicalTree += (object sender, LogicalTreeAttachmentEventArgs args) => CloseStreams();
        }

        public ReactiveCommand<Unit, Unit> DoOpenComicFile { get; }

        public ReactiveCommand<Unit, Unit> DoExit { get; }

        public ReactiveCommand<Unit, Unit> DoNextPage { get; }

        public PageViewModel CurrentPage { get; private set; }

        private FileStream CurrentFileSteam { get; set; }

        private IArchive CurrentArchive { get; set; }

        private int CurrentPageIndex { get; set; }

        private List<IArchiveEntry> CurrentEntryList { get; set; }

        public Button OpenComicButton { get; set; }

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
            dialog.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "Comic Book ZIP", Extensions = { "cbz" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "Comic Book RAR", Extensions = { "cbr" } });

            var result = await dialog.ShowAsync();
            if (result != null)
            {
                var chosenPath = result[0];

                CurrentFileSteam = File.OpenRead(chosenPath);
                CurrentArchive = ArchiveFactory.Open(CurrentFileSteam);
                CurrentEntryList = EntriesToSortedList(CurrentArchive.Entries);

                CurrentPageIndex = 0;
                var entry = CurrentEntryList[CurrentPageIndex];
                using (var entryStream = entry.OpenEntryStream())
                {
                    if (!entry.IsDirectory)
                    {
                        CurrentPage.CurrentImage = await DecodeEntryStream(entryStream);
                    }
                }
                OpenComicButton.Classes.Add("hidden");
            }
        }

        private async Task RunNextPage()
        {
            CurrentPageIndex++;
            var entry = CurrentEntryList[CurrentPageIndex];
            using (var entryStream = entry.OpenEntryStream())
            {
                if (!entry.IsDirectory)
                {
                    CurrentPage.CurrentImage = await DecodeEntryStream(entryStream);
                }
            }
        }

        List<IArchiveEntry> EntriesToSortedList(IEnumerable<IArchiveEntry> entries)
        {
            var list = new List<IArchiveEntry>();
            foreach (var entry in entries)
            {
                list.Add(entry);
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