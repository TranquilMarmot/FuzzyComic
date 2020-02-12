using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using SharpCompress.Archives;
using SkiaSharp;

namespace FuzzyComic.ViewModels.Comic
{
    /// <summary> ViewModel for loading a .cbz (zip) or .cbr (rar) file as a comic </summary>
    public class ArchiveComicViewModel : BaseComicViewModel
    {
        /// <summary> File stream for the currently open file </summary>
        private FileStream CurrentFileSteam { get; set; }

        /// <summary> Archive for the CurrentFileStream </summary>
        private IArchive CurrentArchive { get; set; }

        /// <summary> List of entries (pages) in the archive, sorted alphanumerically </summary>
        private List<IArchiveEntry> CurrentEntryList { get; set; }

        /// <summary> Close the file and archive streams </summary>
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

        /// <param name="filePath">Path of file to load</param>
        public ArchiveComicViewModel(string filePath) : base(filePath)
        {
            // open the file and sort the entries
            CurrentFileSteam = File.OpenRead(filePath);
            CurrentArchive = ArchiveFactory.Open(CurrentFileSteam);
            CurrentEntryList = EntriesToSortedList(CurrentArchive.Entries);

            base.TotalPages = CurrentEntryList.Count;
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
                    var bitmap = new Bitmap(SKData.Create(entryStream).AsStream());
                    return Task.FromResult(bitmap);
                });
            }

            throw new System.Exception("Error loading page from archive");
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
                // skip directories...
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
