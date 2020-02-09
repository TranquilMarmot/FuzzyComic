using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using ImageMagick;
using SkiaSharp;

namespace FuzzyComic.ViewModels
{
    /// <summary>
    /// ViewModel for loading a PDF image-by-image
    /// </summary>
    public class PDFComicViewModel : BaseComicViewModel
    {
        /// <summary> Full path of PDF file </summary>
        private string FilePath;

        /// <summary> Total number of pages in the PDF </summary>
        private int NumberOfPages;

        /// <summary>
        /// Load the PDF from the given path, and load the first page
        /// </summary>
        /// <param name="filePath">Path of PDF to load</param>
        public async Task LoadPDF(string filePath)
        {
            FilePath = filePath;

            NumberOfPages = GetNumberOfPages(filePath);

            // load the first page
            await base.GoToPage(0);
        }

        /// <summary>
        /// Get the total number of pages in a PDF
        /// </summary>
        /// <param name="filePath">Path of PDF to get number of pages from</param>
        /// <returns>Number of pages in the PDF</returns>
        private int GetNumberOfPages(string filePath)
        {
            System.Console.WriteLine(System.AppContext.BaseDirectory);
            return 1;
        }

        /// <summary>
        /// Called when un-loading the ViewModel
        /// </summary>
        public override void CloseStreams() { }

        /// <summary>
        /// Load the given page from the PDF
        /// </summary>
        /// <param name="pageNumber">Page number to load</param>
        /// <returns>Bitmap representing page</returns>
        protected override async Task<Bitmap> LoadPage(int pageNumber)
        {
            using (var collection = new MagickImageCollection())
            {
                using (var stream = new MemoryStream())
                {
                    return await Task.Run(() =>
                    {
                        MagickReadSettings settings = new MagickReadSettings();

                        // Page to read
                        settings.FrameIndex = pageNumber;

                        // Number of pages to read 
                        settings.FrameCount = 1;

                        // This is currently just a "best guess" at the proper density to read the image at
                        // There wasn't an obvious way to get the actual density of the screen from Avalonia
                        settings.Density = new Density(200);

                        // Read only the given page of the pdf file
                        collection.Read(FilePath, settings);
                        var img = collection[0];

                        // Write a bitmap to a stream, then encode that as a skia image
                        img.Write(stream, MagickFormat.Bmp3);
                        stream.Position = 0;
                        var skiaImage = SKImage.FromEncodedData(stream.ToArray());
                        var encoded = skiaImage.Encode();
                        var bitmap = new Bitmap(encoded.AsStream());

                        // Clear the collection
                        collection.Clear();

                        return Task.FromResult(bitmap);
                    });
                }
            }

            throw new System.Exception("Error loading page from PDF " + FilePath);
        }

        /// <summary>
        /// Update the width of the progress bar at the bottom of the page
        /// </summary>
        protected override void UpdateProgressBarWidth()
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var windowWidth = desktop.MainWindow.Width;
                var percentDone = (double)CurrentPageIndex / (double)NumberOfPages;
                ProgressBarWidth = windowWidth * percentDone;
            }
        }
    }
}