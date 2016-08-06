using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Control;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SimpleZIP_UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SummaryPage : Page
    {
        private SummaryPageControl _control;

        private IReadOnlyList<StorageFile> _selectedFiles;

        public SummaryPage()
        {
            this.InitializeComponent();
            Loaded += OnSummaryPageLoaded;
        }

        private void AbortButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void StartButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _selectedFiles = e.Parameter as IReadOnlyList<StorageFile>;

            if (_selectedFiles != null)
                foreach (var f in _selectedFiles) // populate list
                {
                    var textBlock = new TextBlock() { Text = f.Name };
                    this.ItemsListBox.Items?.Add(textBlock);
                }
        }

        private void OnSummaryPageLoaded(object sender, RoutedEventArgs e)
        {
            _control = new SummaryPageControl(this.Frame);
        }
    }
}
