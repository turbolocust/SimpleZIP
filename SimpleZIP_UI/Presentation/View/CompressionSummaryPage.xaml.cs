// ==++==
// 
// Copyright (C) 2018 Matthias Fussenegger
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// ==--==
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Application.Compression;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Operation.Event;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Controller;
using SimpleZIP_UI.Presentation.Handler;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    public sealed partial class CompressionSummaryPage : IDisposable
    {
        /// <summary>
        /// The aggregated controller instance.
        /// </summary>
        private readonly CompressionPageController _controller;

        /// <summary>
        /// A list of selected files for compression.
        /// </summary>
        private IReadOnlyList<StorageFile> _selectedFiles;

        /// <summary>
        /// Maps combo box items to file types for archives.
        /// </summary>
        private static readonly Dictionary<ComboBoxItem, string> FileTypesComboBoxItems;

        static CompressionSummaryPage()
        {
            FileTypesComboBoxItems = new Dictionary<ComboBoxItem, string>();
        }

        /// <inheritdoc />
        public CompressionSummaryPage()
        {
            InitializeComponent();

            // to indicate that some algorithms are not operating as fast as others
            var slowText = I18N.Resources.GetString("Slow/Text").ToLower();
            // to indicate that an algorithm does not use a compressor stream
            var uncompressedText = I18N.Resources.GetString("Uncompressed/Text").ToLower();

            Settings.TryGet(Settings.Keys.HideSomeArchiveTypesKey, out bool isHideSome);

            // ReSharper disable once PossibleNullReferenceException
            ArchiveTypeComboBox.Items.Add(CreateItemForComboBox("ZIP (.zip)", ".zip"));

            if (!isHideSome)
            {
                var tarText = "TAR (.tar) [" + uncompressedText + "]";
                ArchiveTypeComboBox.Items.Add(CreateItemForComboBox("GZIP (.gzip)", ".gzip"));
                ArchiveTypeComboBox.Items.Add(CreateItemForComboBox(tarText, ".tar"));
            }

            ArchiveTypeComboBox.Items.Add(CreateItemForComboBox("TAR+GZIP (.tgz)", ".tgz"));

            var tbz2Text = "TAR+BZIP2 (.tbz2) [" + slowText + "]";
            ArchiveTypeComboBox.Items.Add(CreateItemForComboBox(tbz2Text, ".tbz2"));

            if (!isHideSome)
            {
                var tlzText = "TAR+LZIP (.tlz) [" + slowText + "]";
                ArchiveTypeComboBox.Items.Add(CreateItemForComboBox(tlzText, ".tlz"));
            }

            ArchiveTypeComboBox.SelectedIndex = 0; // selected index on page launch
            _controller = new CompressionPageController(this);
        }

        private static ComboBoxItem CreateItemForComboBox(string content, string fileType)
        {
            var item = new ComboBoxItem { Content = content };
            FileTypesComboBoxItems.Add(item, fileType);
            return item;
        }

        private void FinishOperation()
        {
            SetOperationActive(false);
            AbortButtonToolTip.IsOpen = false;
            ArchiveTypeToolTip.IsOpen = false;
        }

        private async void PickOutputFolder()
        {
            if (ProgressBar.IsEnabled) return;
            var text = await _controller.PickOutputPath();
            if (!string.IsNullOrEmpty(text))
            {
                OutputPathButton.Content = text;
            }
        }

        /// <summary>
        /// Invoked when the abort button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void AbortButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            AbortButtonToolTip.IsOpen = true;
            _controller.AbortButtonAction();
        }

        /// <summary>
        /// Invoked when the start button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private async void StartButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            if (await _controller.CheckOutputFolder())
            {
                var selectedItem = (ComboBoxItem)ArchiveTypeComboBox.SelectedItem;
                var archiveName = ArchiveNameTextBox.Text;

                if (archiveName.Length > 0 && !FileUtils.ContainsIllegalChars(archiveName))
                {
                    // set the algorithm by archive file type
                    FileTypesComboBoxItems.TryGetValue(selectedItem, out var archiveType);
                    Archives.ArchiveFileTypes.TryGetValue(archiveType, out var value);

                    archiveName += archiveType;
                    var result = await InitOperation(value, archiveName);

                    if (result.StatusCode == Result.Status.Success)
                    {
                        ArchiveHistoryHandler.SaveToHistory(
                            _controller.OutputFolder, result.ArchiveNames);
                    }
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    _controller.CreateResultDialog(result).ShowAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    FinishOperation();
                    if (_controller.IsShareTargetActivated()
                        && result.StatusCode == Result.Status.Success)
                    {
                        // leave if share target activated and success
                        _controller.NavigateBackHome();
                    }
                }
            }
            else
            {
                PickOutputFolder();
            }
        }


        /// <summary>
        /// Invoked when the button holding the output path has been tapped.
        /// As a result, the user can pick an output folder for the archive.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void OutputPathButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            PickOutputFolder();
        }

        /// <summary>
        /// Invoked when combo box for choosing the archive type has been closed.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event object.</param>
        private void ArchiveTypeComboBox_DropDownClosed(object sender, object e)
        {
            if (_selectedFiles.Count <= 1) return;
            var selectedItem = (ComboBoxItem)ArchiveTypeComboBox.SelectedItem;

            if (FileTypesComboBoxItems.TryGetValue(selectedItem, out var value) && value.Equals(".gzip"))
            {
                ArchiveTypeToolTip.Content = I18N.Resources.GetString("OnlySingleFileCompression/Text")
                    + "\r\n" + I18N.Resources.GetString("SeparateArchive/Text");
                ArchiveTypeToolTip.IsOpen = true;
            }
        }

        /// <summary>
        /// Invoked when the text of the archive name input has beend modified.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void ArchiveNameTextBox_TextChanged(object sender, TextChangedEventArgs args)
        {
            var fileName = ArchiveNameTextBox.Text;

            if (fileName.Length < 1) // reset if empty
            {
                ArchiveNameTextBox.Text = I18N.Resources.GetString("ArchiveName/Text");
            }
            else if (FileUtils.ContainsIllegalChars(fileName))
            {
                var content = I18N.Resources.GetString("IllegalCharacters/Text")
                    + "\n" + string.Join(" ", FileUtils.IllegalChars);
                ArchiveNameToolTip.Content = content;
                ArchiveNameToolTip.IsOpen = true;
            }
            else
            {
                ArchiveNameToolTip.IsOpen = false;
            }
        }

        /// <summary>
        /// Invoked when the tooltip for "ArchiveTypeButton" has been opened.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void ArchiveTypeToolTip_OnOpened(object sender, RoutedEventArgs args)
        {
            var toolTip = (ToolTip)sender;
            if (!toolTip.IsOpen)
            {
                var timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 8) };
                timer.Tick += (s, evt) => // close tooltip after 8 seconds
                {
                    toolTip.IsOpen = false;
                    timer.Stop();
                };
                timer.Start();
            }
        }

        /// <summary>
        /// Initializes the archiving operation and waits for the result.
        /// </summary>
        /// <param name="key">The type of the archive.</param>
        /// <param name="archiveName">The name of the archive.</param>
        private async Task<Result> InitOperation(Archives.ArchiveType key, string archiveName)
        {
            SetOperationActive(true);
            var totalSize = await _controller.CheckFileSizes(_selectedFiles);
            var info = new CompressionInfo(key, totalSize)
            {
                ArchiveName = archiveName,
                SelectedFiles = _selectedFiles
            };
            return await _controller.StartButtonAction(OnProgressUpdate, info);
        }

        /// <summary>
        /// Sets the archiving operation as active. This results in the UI being busy.
        /// </summary>
        /// <param name="isActive">True to set operation as active, false to set it as inactive.</param>
        private void SetOperationActive(bool isActive)
        {
            if (isActive)
            {
                ProgressBar.IsEnabled = true;
                ProgressBar.Visibility = Visibility.Visible;
                StartButton.IsEnabled = false;
                OutputPathButton.IsEnabled = false;
                ArchiveNameTextBox.IsEnabled = false;
                ArchiveTypeComboBox.IsEnabled = false;
            }
            else
            {
                ProgressBar.IsEnabled = false;
                ProgressBar.Visibility = Visibility.Collapsed;
                StartButton.IsEnabled = true;
                OutputPathButton.IsEnabled = true;
                ArchiveNameTextBox.IsEnabled = true;
                ArchiveTypeComboBox.IsEnabled = true;
            }
        }

        /// <summary>
        /// Updates the progress bar with the updated progress.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">Consists of event parameters.</param>
        internal void OnProgressUpdate(object sender, ProgressUpdateEventArgs args)
        {
            var progress = _controller.ProgressManager.UpdateProgress(sender, args.Progress);
            if (_controller.ProgressManager.Exchange(progress).Equals(_controller.ProgressManager.Sentinel))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var totalProgress = _controller.ProgressManager.Exchange(_controller.ProgressManager.Sentinel);
                    if (totalProgress > ProgressBar.Value)
                    {
                        ProgressBar.Value = totalProgress;
                    }
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        /// <inheritdoc />
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            if (!(args.Parameter is NavigationArgs navigationArgs)) return;

            _selectedFiles = navigationArgs.StorageFiles;
            _controller.ShareOperation = navigationArgs.ShareOperation;

            if (_selectedFiles == null) return;

            foreach (var file in _selectedFiles) // populate list
            {
                ItemsListBox.Items?.Add(new TextBlock { Text = file.Name });
            }
        }

        /// <inheritdoc />
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            e.Cancel = _controller.Operation?.IsRunning ?? false;
        }

        /// <inheritdoc />
        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            FinishOperation();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _controller.Dispose();
        }
    }
}
