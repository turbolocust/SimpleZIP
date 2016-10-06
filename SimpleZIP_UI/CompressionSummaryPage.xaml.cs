using System;
using System.Collections.Generic;
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
    public sealed partial class CompressionSummaryPage : Page
    {
        private readonly CompressionSummaryPageControl _control;

        private IReadOnlyList<StorageFile> _selectedFiles;

        public CompressionSummaryPage()
        {
            this.InitializeComponent();
            this.ArchiveNameTextBox.Focus(FocusState.Programmatic);
            _control = new CompressionSummaryPageControl(this);
        }

        /// <summary>
        /// Triggered when the abort button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
        private void AbortButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            _control.AbortButtonAction();
            SetOperationActive(false);
        }

        /// <summary>
        /// Triggered when the start button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
        /// <exception cref="ArgumentOutOfRangeException">May only be thrown on fatal error.</exception>
        private void StartButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)this.ArchiveTypeComboBox.SelectedItem;
            var archiveName = this.ArchiveNameTextBox.Text;
            var archiveType = selectedItem?.Content?.ToString();

            if (archiveType != null)
            {
                if (archiveName.Length > 0 && !FileValidator.ContainsIllegalChars(archiveName))
                {
                    Algorithm key; // the file type of the archive

                    AlgorithmFileTypes.TryGetValue(archiveType, out key);
                    archiveName += archiveType;

                    InitializeOperation(key, archiveName);
                }
            }
        }


        /// <summary>
        /// Triggered when the panel containing the output path has been tapped.
        /// Lets the user then pick an output folder for the archive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The event that invoked this method.</param>
        private void OutputPathPanel_Tap(object sender, TappedRoutedEventArgs e)
        {
            PickOutputPath();
        }

        /// <summary>
        /// Triggered when output path text block got focus.
        /// Lets the user then pick an output folder for the archive.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
        private void OutputPathTextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!this.ProgressRing.IsActive)
            {
                PickOutputPath();
                FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
            }
        }

        /// <summary>
        /// Triggered when combo box for choosing the archive type has been closed.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
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
        /// Triggered when the text of the archive name input has beend modified.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
        private void ArchiveNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
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
        /// Triggered when any tooltip has been opened.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
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
        /// Triggered after navigating to this page.
        /// </summary>
        /// <param name="e">The event that invoked this method.</param>
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
        /// Initializes the archiving operation and waits for the result of it.
        /// </summary>
        /// <param name="key">The type of the archive.</param>
        /// <param name="archiveName">The name of the archive.</param>
        private async void InitializeOperation(Algorithm key, string archiveName)
        {
            SetOperationActive(true);
            var duration = await _control.StartButtonAction(_selectedFiles, archiveName, key);

            // move focus to avoid accidential focus event on text block
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next);

            if (duration > 0) // success
            {
                await
                    DialogFactory.CreateInformationDialog("Success", "Total duration: " + duration + " seconds.")
                        .ShowAsync();
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

        /// <summary>
        /// Delegates the action to pick an output folder.
        /// Shows the name of the output folder in the UI after successful selection.
        /// </summary>
        private async void PickOutputPath()
        {
            var folder = await _control.OutputPathPanelAction();
            this.OutputPathTextBlock.Text = folder?.Name ?? "";
            this.StartButton.IsEnabled = this.OutputPathTextBlock.Text.Length > 0;
        }

        /// <summary>
        /// Sets the archiving operation as active. This means that the UI is in busy state.
        /// </summary>
        /// <param name="isActive">True to set operation as active.</param>
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
