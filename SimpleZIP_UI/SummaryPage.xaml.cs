using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
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
        private readonly SummaryPageControl _control;

        private IList<StorageFile> _selectedFiles;

        public SummaryPage()
        {
            this.InitializeComponent();
            _control = new SummaryPageControl(this.Frame);
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
            if (e.SourcePageType == typeof(MainPage))
            {
                var selectedFiles = e.Parameter as IList<StorageFile>;
                _selectedFiles = selectedFiles;

                foreach (var f in selectedFiles)
                {
                    this.ItemsListBox.Items.Add(f.Name);
                }
            }
        }
    }
}
