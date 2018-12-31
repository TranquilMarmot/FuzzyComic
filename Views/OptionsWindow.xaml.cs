using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FuzzyComic.ViewModels;

namespace FuzzyComic.Views
{
    public class OptionsWindow : Window
    {
        /// <summary>
        /// List of background color options.
        /// These MUST match the DropdownItems in the XAML.
        /// The key is the "Name" of the DropdownItem, the value is the corresponding color value
        /// </summary>
        public static Dictionary<string, Color> BackgroundColors = new Dictionary<string, Color> {
            { "backgroundColorBlack", Colors.Black },
            { "backgroundColorWhite", Colors.White }
        };

        public OptionsWindow(OptionsViewModel dataContext)
        {
            this.DataContext = dataContext;
            this.InitializeComponent();

            // when changing between maximized/unmaximized, the main window will take focus
            // setting this window to be topmost makes it stay in focus, but also makes it so
            // the user can't select any other windows so.....
            // this.Topmost = true;
            dataContext.CloseOptionsWindow = this.Close;

            // attach the handler for the background color dropdown
            var backgroundColorDropdown = this.FindControl<DropDown>("backgroundColorDropdown");
            backgroundColorDropdown.SelectionChanged += OnBackgroundColorDropdowmSelectionChanges;
        }

        /// <summary>
        /// Handles changes to  the background color dropdown and sets the appropriate values in the view model
        /// </summary>
        private void OnBackgroundColorDropdowmSelectionChanges(object sender, SelectionChangedEventArgs args)
        {
            // this actually gets called twice, once with RemovedItems of the previous one
            // and once with AddedItems with the new one...
            // since you can only have one selected at a time, we only care about added items
            if (args.AddedItems.Count == 1)
            {
                var item = (DropDownItem)args.AddedItems[0];

                // grab the corresponding color
                var color = BackgroundColors[item.Name];

                // set the selected index and color
                var dataContext = (OptionsViewModel)this.DataContext;
                dataContext.SelectedBackgroundColorIndex = BackgroundColors.Keys.ToList().IndexOf(item.Name);
                dataContext.SelectedBackgroundColor = color;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}