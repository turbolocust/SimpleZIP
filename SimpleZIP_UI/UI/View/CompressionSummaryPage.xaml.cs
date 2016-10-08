using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Common.Validator;
using SimpleZIP_UI.UI.Factory;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SimpleZIP_UI.UI.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompressionSummaryPage
    {
        private readonly CompressionSummaryPageControl _control;

        private IReadOnlyList<StorageFile> _selectedFiles;

        public CompressionSummaryPage()
        {
            InitializeComponent();
            ArchiveNameTextBox.Focus(FocusState.Programmatic);
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
            var selectedItem = (ComboBoxItem)ArchiveTypeComboBox.SelectedItem;
            var archiveName = ArchiveNameTextBox.Text;
            var archiveType = selectedItem?.Content?.ToString();

            if (archiveType != null)
            {
                if (archiveName.Length > 0 && !FileValidator.ContainsIllegalChars(archiveName))
                {
                    BaseControl.Algorithm key; // the file type of the archive

                    BaseControl.AlgorithmFileTypes.TryGetValue(archiveType, out key);
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
            if (!ProgressRing.IsActive)
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
            if (_selectedFiles.Count > 1 && ArchiveTypeComboBox.SelectedIndex == 1)
            {
                ArchiveTypeToolTip.Content = "GZIP only allows the compression of one file.\n\n" +
                    "Please choose another algorithm, otherwise only the first file in the list will be packed.";
                ArchiveTypeToolTip.IsOpen = true;
                ArchiveTypeComboBox.SelectedIndex = 0; // reset selection
            }
        }

        /// <summary>
        /// Triggered when the text of the archive name input has beend modified.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
        private void ArchiveNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var fileName = ArchiveNameTextBox.Text;

            if (fileName.Length < 1) // reset if empty
            {
                ArchiveNameTextBox.Text = "myArchive";
            }
            else if (FileValidator.ContainsIllegalChars(fileName)) // check for illegal characters in file name
            {
                ArchiveNameToolTip.Content = "These characters are not allowed:\n" +
                                                  "\\ / | : * \" ? < >\n";
                ArchiveNameToolTip.IsOpen = true;
            }
            else
            {
                ArchiveNameToolTip.IsOpen = false;
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
                foreach (var file in _selectedFiles) // populate list
                {
                    ItemsListBox.Items?.Add(new TextBlock { Text = file.Name });
                }
            }
        }

        /// <summary>
        /// Initializes the archiving operation and waits for the result of it.
        /// </summary>
        /// <param name="key">The type of the archive.</param>
        /// <param name="archiveName">The name of the archive.</param>
        private async void InitializeOperation(BaseControl.Algorithm key, string archiveName)
        {
            SetOperationActive(true);
            var result = await _control.StartButtonAction(_selectedFiles, archiveName, key);
            var duration = result.ElapsedTime;

            // move focus to avoid accidential focus event on text block
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next);

            if (result.StatusCode >= 0) // operation succeeded
            {
                duration = Math.Round(Math.Pow(duration, -6), 2); // calculate seconds

                await
                    DialogFactory.CreateInformationDialog("Success", "Total duration: " + duration + " seconds.")
                        .ShowAsync();
            }
            else // operation failed
            {
                await DialogFactory.CreateInformationDialog("Error", result.Message).ShowAsync();
            }

            SetOperationActive(false);
            Frame.Navigate(typeof(MainPage));
        }

        /// <summary>
        /// Delegates the action to pick an output folder.
        /// Shows the name of the output folder in the UI after successful selection.
        /// </summary>
        private async void PickOutputPath()
        {
            var folder = await _control.OutputPathPanelAction();
            OutputPathTextBlock.Text = folder?.Name ?? "";
            StartButton.IsEnabled = OutputPathTextBlock.Text.Length > 0;
        }

        /// <summary>
        /// Sets the archiving operation as active. This means that the UI is in busy state.
        /// </summary>
        /// <param name="isActive">True to set operation as active.</param>
        private void SetOperationActive(bool isActive)
        {
            if (isActive)
            {
                ProgressRing.IsActive = true;
                ProgressRing.Visibility = Visibility.Visible;
                StartButton.IsEnabled = false;
                OutputPathTextBlock.IsTapEnabled = false;
                ArchiveNameTextBox.IsEnabled = false;
                ArchiveTypeComboBox.IsEnabled = false;
            }
            else
            {
                ProgressRing.IsActive = false;
                ProgressRing.Visibility = Visibility.Collapsed;
                StartButton.IsEnabled = true;
                OutputPathTextBlock.IsTapEnabled = true;
                ArchiveNameTextBox.IsEnabled = true;
                ArchiveTypeComboBox.IsEnabled = true;
            }
        }
    }
}
