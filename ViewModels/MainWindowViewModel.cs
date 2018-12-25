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
            DoTheThing = ReactiveCommand.CreateFromTask(RunTheThing);
            // DoTheThing = ReactiveCommand.Create(RunTheThing);

            Greeting = "Hello";
            CurrentImage = new PageViewModel();
        }
        public string Greeting { get; private set; }

        public ReactiveCommand<Unit, Unit> DoTheThing { get; }

        public PageViewModel CurrentImage { get; private set; }

        async Task RunTheThing()
        {
            Greeting = "Load that shit";
            var dialog = new OpenFileDialog();
            dialog.Title = "Pick a comic";
            dialog.AllowMultiple = false;
            dialog.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "Comic Book ZIP", Extensions = { "cbz" } });
            dialog.Filters.Add(new FileDialogFilter() { Name = "Comic Book RAR", Extensions = { "cbr" } });

            var result = await dialog.ShowAsync();
            if (result != null)
            {
                System.Console.WriteLine("hello ello");
                var chosenPath = result[0];

                using (var fileStream = File.OpenRead(chosenPath))
                using (var reader = ReaderFactory.Open(fileStream))
                {
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            var entryStream = reader.OpenEntryStream();

                            // TODO this returns null on error
                            var skiaBitmap = SKBitmap.Decode(entryStream);
                            var skiaImage = SKImage.FromBitmap(skiaBitmap);
                            var encoded = skiaImage.Encode();

                            var bitmap = new Bitmap(encoded.AsStream());

                            CurrentImage.CurrentPage = bitmap;
                            break;
                        }
                    }
                }
            }
        }
    }
}