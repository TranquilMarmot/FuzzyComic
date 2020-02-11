using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ImageMagick;

namespace FuzzyComic.ViewModels.Comic
{
    /// <summary>
    /// ViewModel for loading a PDF image-by-image
    /// </summary>
    public class PDFComicViewModel : BaseComicViewModel
    {
        /// <summary>
        /// Folder that contains Ghostscript exe/dll for Windows machines.
        /// 
        /// These are copied to the `bin` directory via a `Content Include` tag in the root `.csproj` file.
        /// For Linux and macOS, we assume that the user has Ghostscript installed already.
        /// </summary>
        private static string GhostscriptDirectoryWindows = $"{System.AppContext.BaseDirectory}/Ghostscript";

        /// <summary> Full path of PDF file </summary>
        private string FilePath;

        private PDFComicViewModel(string filePath)
        {
            FilePath = filePath;
            TotalPages = GetNumberOfPages();
        }

        /// <summary>
        /// Load a PDF file
        /// </summary>
        /// <param name="filePath">Path of file to load</param>
        /// <returns>ViewModel that shows a PDF comic</returns>
        public static async Task<PDFComicViewModel> LoadPDF(string filePath)
        {
            // if we're on Windows, use the included .dll and .exe files
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                MagickNET.SetGhostscriptDirectory(GhostscriptDirectoryWindows);
            }

            var viewModel = new PDFComicViewModel(filePath);

            // Load the first page
            await viewModel.GoToPage(0);

            return viewModel;
        }

        /// <summary>
        /// Get the total number of pages in a PDF
        /// </summary>
        /// <param name="filePath">Path of PDF to get number of pages from</param>
        /// <returns>Number of pages in the PDF</returns>
        private int GetNumberOfPages()
        {
            // Run the Ghostscript executable to get the number of pages
            var process = new System.Diagnostics.Process();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // if we're on Windows, we want to use the included Ghostscript executable
                process.StartInfo.FileName = $"{GhostscriptDirectoryWindows}\\gswin64c.exe";
            }
            else
            {
                // otherwise, we have to assume the user has Ghostscript installed
                process.StartInfo.FileName = "gs";
            }

            // This Ghostscript command will (quickly) output the number of pages in the PDF
            process.StartInfo.Arguments = $"-q -dNODISPLAY -dNOSAFER -c \"({FilePath.Replace("\\", "/")}) (r) file runpdfbegin pdfpagecount = quit\"";

            // Set these so a new window doesn't pop up
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            // Set output of program to be written to process output stream
            process.StartInfo.RedirectStandardOutput = true;

            // Start the process
            try
            {
                process.Start();
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                // TODO show a message box here about how to install Ghostscript on Linux and macOS
                // fallback to an error and return 1...
                System.Console.Error.WriteLine($"Couldn't call out to Ghostscript executable to get number of pages; does the user have Ghostscript installed?\nError: {e.Message}");
                return 1;
            }

            // Get program output and wait for the process to finish
            string strOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            int output;
            if (int.TryParse(strOutput, out output))
            {
                return output;
            }
            else
            {
                // this will potentially be error output from Ghostscript
                System.Console.Error.WriteLine($"Error getting number of pages in PDF. Got output:\n{strOutput}");

                // Note: By default we do 1 instead of 0 to avoid divide by zero errors
                return 1;
            }
        }

        /// <summary>
        /// Called when un-loading the ViewModel
        /// </summary>
        public override void CloseStreams()
        {
            // Nothing to close here since LoadPage opens-and-closes the file every time :(
        }

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

                        // Write a PNG to a stream, then pass that to an Avalonia Bitmap
                        img.Write(stream, MagickFormat.Png);
                        stream.Position = 0;
                        var bitmap = new Bitmap(stream);

                        // Clear the collection
                        collection.Clear();

                        return Task.FromResult(bitmap);
                    });
                }
            }

            throw new System.Exception("Error loading page from PDF " + FilePath);
        }
    }
}
