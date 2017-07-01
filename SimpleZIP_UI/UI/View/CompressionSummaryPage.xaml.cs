using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Common.Model;
using SimpleZIP_UI.Common.Util;
using SimpleZIP_UI.UI.Factory;
using static SimpleZIP_UI.UI.BaseControl;

namespace SimpleZIP_UI.UI.View
{
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
        /// Invoked when the abort button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private void AbortButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            _control.AbortButtonAction();
        }

        /// <summary>
        /// Invoked when the start button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private async void StartButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            var selectedItem = (ComboBoxItem)ArchiveTypeComboBox.SelectedItem;
            var archiveName = ArchiveNameTextBox.Text;
            var archiveType = selectedItem?.Content?.ToString();

            if (archiveType != null && archiveName.Length > 0 && !archiveName.ContainsIllegalChars())
            {
                archiveType = ParseArchiveType(archiveType); // parse actual type of selection
                try
                {
                    Algorithm value; // set the algorithm by archive type
                    AlgorithmFileTypes.TryGetValue(archiveType, out value);

                    archiveName += archiveType;
                    await InitOperation(value, archiveName);
                }
                catch (ArgumentNullException)
                {
                    await DialogFactory.CreateErrorDialog("Archive type not recognized.").ShowAsync();
                }
                finally
                {
                    Frame.Navigate(typeof(MainPage));
                }
            }
        }


        /// <summary>
        /// Invoked when the panel containing the output path has been tapped.
        /// As a result, the user can pick an output folder for the archive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">Arguments that may have been passed.</param>
        private void OutputPathPanel_Tap(object sender, TappedRoutedEventArgs args)
        {
            PickOutputPath();
        }

        /// <summary>
        /// Invoked when output path text block got focus.
        /// As a result, the user can pick an output folder for the archive.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private void OutputPathTextBlock_GotFocus(object sender, RoutedEventArgs args)
        {
            if (!ProgressRing.IsActive)
            {
                PickOutputPath();
                FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
            }
        }

        /// <summary>
        /// Invoked when combo box for choosing the archive type has been closed.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event object.</param>
        private void ArchiveTypeComboBox_DropDownClosed(object sender, object e)
        {
            if (_selectedFiles.Count > 1)
            {
                var selectedItem = (ComboBoxItem)ArchiveTypeComboBox.SelectedItem;
                var archiveType = selectedItem?.Content?.ToString();

                if (archiveType != null && archiveType.Contains("gzip"))
                {
                    ArchiveTypeToolTip.Content = "GZIP only allows the compression of a single file.\r\n" +
                    "Therefore, each file will be put into a separate archive.";
                    ArchiveTypeToolTip.IsOpen = true;
                }
            }
        }

        /// <summary>
        /// Invoked when the text of the archive name input has beend modified.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private void ArchiveNameTextBox_TextChanged(object sender, TextChangedEventArgs args)
        {
            var fileName = ArchiveNameTextBox.Text;

            if (fileName.Length < 1) // reset if empty
            {
                ArchiveNameTextBox.Text = "myArchive";
            }
            else if (fileName.ContainsIllegalChars()) // check for illegal characters in file name
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
        /// Invoked when any tooltip has been opened.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private void ToolTip_Opened(object sender, RoutedEventArgs args)
        {
            var toolTip = (ToolTip)sender;

            // use timer to close tooltip after 8 seconds
            var timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 5) };
            timer.Tick += (s, evt) =>
            {
                toolTip.IsOpen = false;
                timer.Stop();
            };
            timer.Start();
        }

        /// <summary>
        /// Initializes the archiving operation and waits for the result.
        /// </summary>
        /// <param name="key">The type of the archive.</param>
        /// <param name="archiveName">The name of the archive.</param>
        private async Task<bool> InitOperation(Algorithm key, string archiveName)
        {
            SetOperationActive(true);
            var archiveInfo = new ArchiveInfo(_selectedFiles, ArchiveInfo.CompressionMode.Compress)
            {
                ArchiveName = archiveName,
                Key = key
            };
            var result = await _control.StartButtonAction(archiveInfo);

            // move focus to avoid accidental focus event on text block
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next);

            await _control.CreateResultDialog(result).ShowAsync();
            return result.StatusCode == Result.Status.Success;
        }

        /// <summary>
        /// Delegates the action to pick an output folder. 
        /// Displays the name of the output folder afterwards.
        /// </summary>
        private async void PickOutputPath()
        {
            var folder = await _control.OutputPathPanelAction();
            OutputPathTextBlock.Text = folder?.Name ?? "";
            StartButton.IsEnabled = OutputPathTextBlock.Text.Length > 0;
        }

        /// <summary>
        /// Parses the file type of the specified string from combo box.
        /// </summary>
        /// <param name="s">The string from the combo box to be parsed.</param>
        /// <returns>The file type as string.</returns>
        private static string ParseArchiveType(string s)
        {
            int startIndex = s.IndexOf('.'),
                length = s.Length - 1 - startIndex;
            return s.Substring(startIndex, length);
        }

        /// <summary>
        /// Sets the archiving operation as active. This results in the UI being busy.
        /// </summary>
        /// <param name="isActive">True to set operation as active, false to set it as inactive.</param>
        private void SetOperationActive(bool isActive)
        {
            if (isActive)
            {
                ProgressRing.IsActive = true;
                ProgressRing.Visibility = Visibility.Visible;
                StartButton.IsEnabled = false;
                OutputPathTextBlock.IsEnabled = false;
                ArchiveNameTextBox.IsEnabled = false;
                ArchiveTypeComboBox.IsEnabled = false;
            }
            else
            {
                ProgressRing.IsActive = false;
                ProgressRing.Visibility = Visibility.Collapsed;
                StartButton.IsEnabled = true;
                OutputPathTextBlock.IsEnabled = true;
                ArchiveNameTextBox.IsEnabled = true;
                ArchiveTypeComboBox.IsEnabled = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            _selectedFiles = args.Parameter as IReadOnlyList<StorageFile>;

            if (_selectedFiles == null) return;

            foreach (var file in _selectedFiles) // populate list
            {
                ItemsListBox.Items?.Add(new TextBlock { Text = file.Name });
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            SetOperationActive(false);
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            e.Cancel = _control.IsRunning || _control.IsCancelRequest;
            await _control.ConfirmCancellationRequest();
        }
    }
}
