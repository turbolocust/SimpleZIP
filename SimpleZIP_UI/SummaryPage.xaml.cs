using System;
using System.Collections.Generic;
using System.Threading;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.UI;
using Control = SimpleZIP_UI.UI.Control;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SimpleZIP_UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SummaryPage : Page
    {
        private readonly SummaryPageControl _control;

        private IReadOnlyList<StorageFile> _selectedFiles;

        public SummaryPage()
        {
            this.InitializeComponent();
            _control = new SummaryPageControl(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AbortButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            _control.AbortButtonAction();
            SetProgressRingActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void StartButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            var selectedIndex = this.ArchiveTypeComboBox.SelectedIndex;
            var archiveName = this.ArchiveNameTextBox.Text;
            UI.Control.Algorithm algorithmType;

            switch (selectedIndex)
            {
                case 0: // zip
                    archiveName += ".zip";
                    algorithmType = Control.Algorithm.Zip;
                    break;

                case 1: // gzip
                    archiveName += ".gz";
                    algorithmType = Control.Algorithm.Gzip;
                    break;

                case 2: // tar.gz
                    archiveName += ".tar.gz";
                    algorithmType = Control.Algorithm.TarGz;
                    break;

                case 3: // tar.bz2
                    archiveName += ".tar.bz2";
                    algorithmType = Control.Algorithm.TarBz2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selectedIndex), selectedIndex, null);
            }
            //TODO

            SetProgressRingActive(true);
            _control.StartButtonAction(_selectedFiles, archiveName);
            SetProgressRingActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _selectedFiles = e.Parameter as IReadOnlyList<StorageFile>;

            if (_selectedFiles != null)
            {
                foreach (var f in _selectedFiles) // populate list
                {
                    this.ItemsListBox.Items?.Add(new TextBlock() { Text = f.Name });
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isActive"></param>
        private void SetProgressRingActive(bool isActive)
        {
            if (isActive)
            {
                this.ProgressRing.IsActive = true;
                this.ProgressRing.Visibility = Visibility.Visible;
            }
            else
            {
                this.ProgressRing.IsActive = false;
                this.ProgressRing.Visibility = Visibility.Collapsed;
            }
        }
    }
}
