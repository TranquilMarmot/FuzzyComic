using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using ReactiveUI;
using SharpCompress.Archives;
using SkiaSharp;

namespace FuzzyComic.ViewModels
{
    public class ComicViewModel : ReactiveObject
    {
        /// <summary> File stream for the currently open file </summary>
        private FileStream CurrentFileSteam { get; set; }

        /// <summary> Archive for the CurrentFileStream </summary>
        private IArchive CurrentArchive { get; set; }

        /// <summary> List of entries (pages) in the archive, sorted alphanumerically </summary>
        private List<IArchiveEntry> CurrentEntryList { get; set; }

        /// <summary> Index in the CurrentEntryList of the CurrentPage </summary>
        public int CurrentPageIndex { get; set; }

        /// <summary>Image of the current page being displayed</summary>
        private Bitmap currentPageBitmap;

        /// <summary>Image of the current page being displayed</summary>
        public Bitmap CurrentPage
        {
            get { return this.currentPageBitmap; }
            private set
            {
                if (this.currentPageBitmap != null)
                {
                    this.currentPageBitmap.Dispose();
                }

                this.RaiseAndSetIfChanged(ref this.currentPageBitmap, value);
            }
        }

        private double progressBarWidth;

        public double ProgressBarWidth
        {
            get { return this.progressBarWidth; }
            private set
            {
                this.RaiseAndSetIfChanged(ref this.progressBarWidth, value);
            }
        }

        public async Task GoToPage(int page)
        {
            CurrentPageIndex = page;
            CurrentPage = await LoadPage(CurrentPageIndex);

            UpdateProgressBarWidth();
        }

        public void UpdateProgressBarWidth()
        {
            var windowWidth = Application.Current.MainWindow.Width;
            var percentDone = (double)CurrentPageIndex / (double)CurrentEntryList.Count;
            ProgressBarWidth = windowWidth * percentDone;
        }

        /// <summary>
        /// Close the file and archive streams
        /// </summary>
        public void CloseStreams()
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

        public async Task LoadArchive(string filePath)
        {
            CloseStreams();

            // open the file and sort the entries
            CurrentFileSteam = File.OpenRead(filePath);
            CurrentArchive = ArchiveFactory.Open(CurrentFileSteam);
            CurrentEntryList = EntriesToSortedList(CurrentArchive.Entries);

            // load the first page
            await GoToPage(0);
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
                // skip directories for now...
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
