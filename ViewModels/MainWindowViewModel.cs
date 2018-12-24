using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using ReactiveUI;

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


        void RunTheThing()
        {
            System.Console.WriteLine("Open a file!");
        }
    }
}
