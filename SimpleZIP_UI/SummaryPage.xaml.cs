using System;
using System.Collections.Generic;
using Windows.Media.Devices;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Common.Validator;
using SimpleZIP_UI.UI;
using SimpleZIP_UI.UI.Factory;
using static SimpleZIP_UI.UI.Control;

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
            this.ArchiveNameTextBox.Focus(FocusState.Programmatic);
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
            SetOperationActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="ArgumentOutOfRangeException">May only be thrown on fatal error.</exception>
        private async void StartButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            var selectedIndex = this.ArchiveTypeComboBox.SelectedIndex;
            var archiveName = this.ArchiveNameTextBox.Text;

            if (archiveName.Length > 0 && !FileValidator.ContainsIllegalChars(archiveName))
            {
                Algorithm key; // the file type of the archive

                switch (selectedIndex) // check selected algorithm
                {
                    case 0: // zip
                        archiveName += ".zip";
                        key = Algorithm.Zip;
                        break;

                    case 1: // gzip
                        archiveName += ".gz";
                        key = Algorithm.Gzip;
                        break;

                    case 2: // tar.gz
                        archiveName += ".tar.gz";
                        key = Algorithm.TarGz;
                        break;

                    case 3: // tar.bz2
                        archiveName += ".tar.bz2";
                        key = Algorithm.TarBz2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(selectedIndex), selectedIndex, null);
                }

                // start the operation
                SetOperationActive(true);
                var duration = await _control.StartButtonAction(_selectedFiles, archiveName, key);
                // move focus to avoid accidential focus event on text block
                FocusManager.TryMoveFocus(FocusNavigationDirection.Next);

                if (duration > 0) // success
                {
                    await DialogFactory.CreateInformationDialog("Success", "Total duration: " + duration).ShowAsync();
                }
                else switch (duration) // an error occurred
                    {
                        case -1:
                            await DialogFactory.CreateInformationDialog("Oops!",
                                 "Operation successfully canceled.").ShowAsync();
                            break;
                        case -2:
                            await DialogFactory.CreateInformationDialog("Oops!",
                                "Looks like we do not have access to those files.").ShowAsync();
                            break;
                        default:
                            await DialogFactory.CreateInformationDialog("Oops!",
                                "Looks like something went wrong.").ShowAsync();
                            break;
                    }

                SetOperationActive(false);
                this.Frame.Navigate(typeof(MainPage));
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutputPathPanel_Tap(object sender, TappedRoutedEventArgs e)
        {
            PickOutputFolder();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutputPathTextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!this.ProgressRing.IsActive)
            {
                PickOutputFolder();
                FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArchiveTypeComboBox_DropDownClosed(object sender, object e)
        {
            if (_selectedFiles.Count > 1 && this.ArchiveTypeComboBox.SelectedIndex == 1)
            {
                this.ArchiveTypeToolTip.Content = "GZIP only allows the compression of one file.\n\n" +
                    "Please choose another algorithm, otherwise only the first file in the list will be packed.";
                this.ArchiveTypeToolTip.IsOpen = true;
                this.ArchiveTypeComboBox.SelectedIndex = 0; // reset selection
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="textChangedEventArgs"></param>
        private void ArchiveNameTextBox_TextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            var fileName = this.ArchiveNameTextBox.Text;

            if (fileName.Length < 1) // reset if empty
            {
                this.ArchiveNameTextBox.Text = "myArchive";
            }
            else if (FileValidator.ContainsIllegalChars(fileName)) // check for illegal characters in file name
            {
                this.ArchiveNameToolTip.Content = "These characters are not allowed:\n" +
                                                  "\\ / | : * \" ? < >\n";
                this.ArchiveNameToolTip.IsOpen = true;
            }
            else
            {
                this.ArchiveNameToolTip.IsOpen = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolTip_Opened(object sender, RoutedEventArgs e)
        {
            var toolTip = (ToolTip)sender;

            // use timer to close tool tip after 8 seconds
            var timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 8) };
            timer.Tick += (s, evt) =>
            {
                toolTip.IsOpen = false;
                timer.Stop();
            };
            timer.Start();
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
        private async void PickOutputFolder()
        {
            var folder = await _control.OutputPathPanelAction();
            this.OutputPathTextBlock.Text = folder?.Name ?? "";
            this.StartButton.IsEnabled = this.OutputPathTextBlock.Text.Length > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isActive"></param>
        private void SetOperationActive(bool isActive)
        {
            if (isActive)
            {
                this.ProgressRing.IsActive = true;
                this.ProgressRing.Visibility = Visibility.Visible;
                this.StartButton.IsEnabled = false;
                this.OutputPathTextBlock.IsTapEnabled = false;
                this.ArchiveNameTextBox.IsEnabled = false;
                this.ArchiveTypeComboBox.IsEnabled = false;
            }
            else
            {
                this.ProgressRing.IsActive = false;
                this.ProgressRing.Visibility = Visibility.Collapsed;
                this.StartButton.IsEnabled = true;
                this.OutputPathTextBlock.IsTapEnabled = true;
                this.ArchiveNameTextBox.IsEnabled = true;
                this.ArchiveTypeComboBox.IsEnabled = true;
            }
        }
    }
}
