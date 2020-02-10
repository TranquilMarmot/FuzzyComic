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
        /// <summary>
        /// Folder that contains GhostScript exe/dll.
        /// 
        /// These are copied to the `bin` directory via a `Content Include` tag in the root `.csproj` file.
        /// </summary>
        private static string GhostScriptDirectory = $"{System.AppContext.BaseDirectory}\\Ghostscript";

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
            // TODO Figure out how this works on Linux/macOS
            MagickNET.SetGhostscriptDirectory(GhostScriptDirectory);

            FilePath = filePath;

            // Load the number of pages
            NumberOfPages = GetNumberOfPages(filePath);

            // Load the first page
            await base.GoToPage(0);
        }

        /// <summary>
        /// Get the total number of pages in a PDF
        /// </summary>
        /// <param name="filePath">Path of PDF to get number of pages from</param>
        /// <returns>Number of pages in the PDF</returns>
        private int GetNumberOfPages(string filePath)
        {
            // Run the GhostScript executable
            // TODO: Figure out how to run this on Linux/macOS
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = $"{GhostScriptDirectory}\\gswin64c.exe";

            // This GhostScript command will (quickly) output the number of pages in the PDF
            process.StartInfo.Arguments = $"-q -dNODISPLAY -dNOSAFER -c \"({filePath.Replace("\\", "/")}) (r) file runpdfbegin pdfpagecount = quit\"";

            // Set these so a new window doesn't pop up
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            // Set output of program to be written to process output stream
            process.StartInfo.RedirectStandardOutput = true;

            // Start the process
            process.Start();

            // Get program output
            string strOutput = process.StandardOutput.ReadToEnd();

            // Wait for process to finish
            process.WaitForExit();

            int output;
            if (int.TryParse(strOutput, out output))
            {
                return output;
            }
            else
            {
                System.Console.Error.WriteLine($"Error getting number of pages in PDF. Got output:\n{strOutput}");
            }

            // Note: By default we do 1 instead of 0 to avoid divide by zero errors
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
                        var settings = new MagickReadSettings();

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