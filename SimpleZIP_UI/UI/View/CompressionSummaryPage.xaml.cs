using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Common.Util;
using SimpleZIP_UI.UI.Factory;

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
        /// <param name="e">The event that invoked this method.</param>
        private void AbortButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            _control.AbortButtonAction();
        }

        /// <summary>
        /// Invoked when the start button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
        private async void StartButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)ArchiveTypeComboBox.SelectedItem;
            var archiveName = ArchiveNameTextBox.Text;
            var archiveType = selectedItem?.Content?.ToString();

            if (archiveType != null)
            {
                if (archiveName.Length > 0 && !FileValidator.ContainsIllegalChars(archiveName))
                {
                    archiveType = ParseArchiveType(archiveType); // parse actual type of selection

                    try
                    {
                        BaseControl.Algorithm key; // set the algorithm by archive type
                        BaseControl.AlgorithmFileTypes.TryGetValue(archiveType, out key);

                        archiveName += archiveType;
                        InitOperation(key, archiveName);
                    }
                    catch (ArgumentNullException)
                    {
                        Frame.Navigate(typeof(MainPage));
                        await DialogFactory.CreateErrorDialog("Archive type not recognized.").ShowAsync();
                    }
                }
            }
        }


        /// <summary>
        /// Invoked when the panel containing the output path has been tapped.
        /// Lets the user then pick an output folder for the archive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The event that invoked this method.</param>
        private void OutputPathPanel_Tap(object sender, TappedRoutedEventArgs e)
        {
            PickOutputPath();
        }

        /// <summary>
        /// Invoked when output path text block got focus.
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
        /// Invoked when combo box for choosing the archive type has been closed.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
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
        /// Invoked when any tooltip has been opened.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
        private void ToolTip_Opened(object sender, RoutedEventArgs e)
        {
            var toolTip = (ToolTip)sender;

            // use timer to close tooltip after 8 seconds
            var timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 8) };
            timer.Tick += (s, evt) =>
            {
                toolTip.IsOpen = false;
                timer.Stop();
            };
            timer.Start();
        }

        /// <summary>
        /// Invoked after navigating to this page.
        /// </summary>
        /// <param name="args">The arguments of the navigation event.</param>
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            _selectedFiles = args.Parameter as IReadOnlyList<StorageFile>;

            if (_selectedFiles != null)
            {
                foreach (var file in _selectedFiles) // populate list
                {
                    ItemsListBox.Items?.Add(new TextBlock { Text = file.Name });
                }
            }
        }

        /// <summary>
        /// Invoked after navigating from this page.
        /// </summary>
        /// <param name="args">The arguments of the navigation event.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            SetOperationActive(false);
        }

        /// <summary>
        /// Initializes the archiving operation and waits for the result.
        /// </summary>
        /// <param name="key">The type of the archive.</param>
        /// <param name="archiveName">The name of the archive.</param>
        private async void InitOperation(BaseControl.Algorithm key, string archiveName)
        {
            SetOperationActive(true);
            var result = await _control.StartButtonAction(_selectedFiles, archiveName, key);

            // move focus to avoid accidential focus event on text block
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next);

            if (result.StatusCode >= 0) // success
            {
                var durationText = _control.BuildDurationText(result.ElapsedTime);

                await DialogFactory.CreateInformationDialog(
                    "Success", durationText).ShowAsync();
            }
            else // error
            {
                await DialogFactory.CreateErrorDialog(result.Message).ShowAsync();
            }

            Frame.Navigate(typeof(MainPage));
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
        /// Parses the file type of the specified string.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <returns>The parsed string.</returns>
        private static string ParseArchiveType(string s)
        {
            int startIndex = s.IndexOf('.'),
                length = (s.Length - 1) - startIndex;
            return s.Substring(startIndex, length);
        }

        /// <summary>
        /// Sets the archiving operation as active. This results in the UI being busy.
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
