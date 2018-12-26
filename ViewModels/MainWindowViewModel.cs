using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ReactiveUI;
using SharpCompress.Readers;
using SkiaSharp;

namespace FuzzyComic.ViewModels
{
    public class MainWindowViewModel : Window
    {
        public MainWindowViewModel()
        {
            DoOpenComicFile = ReactiveCommand.CreateFromTask(RunOpenComicFile);
            CurrentPage = new PageViewModel();
        }

        public ReactiveCommand<Unit, Unit> DoOpenComicFile { get; }

        public PageViewModel CurrentPage { get; private set; }

        async Task RunOpenComicFile()
        {
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

                using (var fileStream = File.OpenRead(chosenPath))
                using (var reader = ReaderFactory.Open(fileStream))
                {
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            var entryStream = reader.OpenEntryStream();

                            // SkiaSharp is the underlying image library that Avalonia uses, so we use that here
                            // First, we have to decode the image into a bitmap
                            // Then, re-encode that into an image (this is in case i.e. we have bmp or jpeg and need png)
                            // Then create a bitmap from the stream of that encoded image...
                            // This isn't the best or greatest thing ever, but it makes it so that we always have a compatible format
                            var skiaBitmap = SKBitmap.Decode(entryStream); // TODO this returns null on error
                            var skiaImage = SKImage.FromBitmap(skiaBitmap);
                            var encoded = skiaImage.Encode();

                            var bitmap = new Bitmap(encoded.AsStream());

                            CurrentPage.CurrentImage = bitmap;
                            break;
                        }
                    }
                }
            }
        }
    }
}