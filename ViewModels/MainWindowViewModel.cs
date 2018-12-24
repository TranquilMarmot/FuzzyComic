using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Text;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ReactiveUI;
using SharpCompress.Readers;

namespace FuzzyComic.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            DoTheThing = ReactiveCommand.Create(RunTheThing);
        }
        public string Greeting => "Hello World!";

        public ReactiveCommand<Unit, Unit> DoTheThing { get; }

        public Bitmap CurrentImage { get; private set; }


        async void RunTheThing()
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
                            var bitmap = new Bitmap(entryStream);
                            CurrentImage = bitmap;
                            break;
                        }
                    }
                }
            }
        }
    }
}