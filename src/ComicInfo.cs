
namespace FuzzyComic
{
    /// <summary>
    /// Represents information about a specific comic.
    /// 
    /// One of these is stored for each comic opened (in a dictionary with the file name as a key)
    /// </summary>
    public struct ComicInfo
    {
        public int PageNumber { get; set; }

        public bool MangaMode { get; set; }
    }
}