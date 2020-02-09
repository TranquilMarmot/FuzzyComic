using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using SharpCompress.Archives;
using SkiaSharp;

namespace FuzzyComic.ViewModels
{
    /// <summary>
    /// ViewModel for loading a .cbz (zip) or .cbr (rar) file as a comic
    /// </summary>
    public class ArchiveComicViewModel : BaseComicViewModel
    {
        /// <summary> File stream for the currently open file </summary>
        private FileStream CurrentFileSteam { get; set; }

        /// <summary> Archive for the CurrentFileStream </summary>
        private IArchive CurrentArchive { get; set; }

        /// <summary> List of entries (pages) in the archive, sorted alphanumerically </summary>
        private List<IArchiveEntry> CurrentEntryList { get; set; }

        /// <summary>
        /// Close the file and archive streams
        /// </summary>
        public override void CloseStreams()
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

        /// <summary>
        /// Load an archive (.zip or .rar) file
        /// </summary>
        /// <param name="filePath">Path of file to load</param>
        public async Task LoadArchive(string filePath)
        {
            CloseStreams();

            // open the file and sort the entries
            CurrentFileSteam = File.OpenRead(filePath);
            CurrentArchive = ArchiveFactory.Open(CurrentFileSteam);
            CurrentEntryList = EntriesToSortedList(CurrentArchive.Entries);

            // load the first page
            await base.GoToPage(0);
        }

        /// <summary>
        /// Load the page at the given index in the sorted entry list
        /// </summary>
        /// <param name="index">Index of page to load</param>
        /// <returns>Bitmap of image at index</returns>
        protected override async Task<Bitmap> LoadPage(int index)
        {
            var entry = CurrentEntryList[index];
            using (var entryStream = entry.OpenEntryStream())
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

            throw new System.Exception("Error loading page from archive");
        }

        protected override void UpdateProgressBarWidth()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var windowWidth = desktop.MainWindow.Width;
                var percentDone = (double)CurrentPageIndex / (double)CurrentEntryList.Count;
                ProgressBarWidth = windowWidth * percentDone;
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
    }
}
