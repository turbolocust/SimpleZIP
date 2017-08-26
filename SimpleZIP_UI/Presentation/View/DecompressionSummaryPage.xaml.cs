// ==++==
// 
// Copyright (C) 2017 Matthias Fussenegger
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Operation.Event;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Controller;

namespace SimpleZIP_UI.Presentation.View
{
    public sealed partial class DecompressionSummaryPage : Page, IDisposable
    {
        /// <summary>
        /// The aggregated control instance.
        /// </summary>
        private readonly DecompressionSummaryPageController _controller;

        /// <summary>
        /// A list of selected files for decompression.
        /// </summary>
        private IReadOnlyList<ExtractableItem> _selectedItems;

        public DecompressionSummaryPage()
        {
            InitializeComponent();
            _controller = new DecompressionSummaryPageController(this);
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
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        private async void StartButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            var result = await InitOperation();
            _controller.CreateResultDialog(result).ShowAsync().AsTask().Forget();
            Frame.Navigate(typeof(MainPage));
        }

        /// <summary>
        /// Invoked when the button holding the output path has been tapped.
        /// As a result, the user can pick an output folder for the archive.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private async void OutputPathButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            if (ProgressBar.IsEnabled) return;
            var text = await _controller.PickOutputPath();
            if (!string.IsNullOrEmpty(text))
            {
                OutputPathButton.Content = text;
                StartButton.IsEnabled = true;
            }
            else
            {
                StartButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Initializes the archiving operation and waits for the result.
        /// </summary>
        private async Task<Result> InitOperation()
        {
            SetOperationActive(true);
            var infos = new List<DecompressionInfo>(_selectedItems.Count);
            var totalSize = await _controller.CheckFileSizes(_selectedItems);

            infos.AddRange(_selectedItems.Select(item => new DecompressionInfo(item, totalSize)));

            return await _controller.StartButtonAction(OnProgressUpdate, infos.ToArray());
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
            }
            else
            {
                ProgressBar.IsEnabled = false;
                ProgressBar.Visibility = Visibility.Collapsed;
                StartButton.IsEnabled = true;
                OutputPathButton.IsEnabled = true;
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
            if (_controller.ProgressManager.Exchange(progress).Equals(ProgressManager.Sentinel))
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var totalProgress = _controller.ProgressManager.Exchange(ProgressManager.Sentinel);
                    if (totalProgress > ProgressBar.Value)
                    {
                        ProgressBar.Value = totalProgress;
                    }
                }).AsTask().Forget();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            // check if file has been opened via file explorer
            var eventArgs = args.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;
            if (eventArgs?.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
            {
                var fileArgs = eventArgs as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
                var files = fileArgs?.Files;
                if (!files.IsNullOrEmpty())
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var itemList = new List<ExtractableItem>(files.Count);
                    itemList.AddRange(files.Select(file
                        => new ExtractableItem(file.Name, file as StorageFile)));
                    _selectedItems = itemList;
                }
            }
            // navigated from MainPage
            else if (args.Parameter is IReadOnlyList<ExtractableItem> items)
            {
                _selectedItems = items;
            }
            // populate list
            foreach (var item in _selectedItems)
            {
                // ReSharper disable once PossibleNullReferenceException
                ItemsListBox.Items.Add(new TextBlock { Text = item.DisplayName });
                if (!item.Entries.IsNullOrEmpty()) // add entries with indent as well
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var entry in item.Entries)
                    {
                        stringBuilder.Append("-> ").Append(entry.Name);
                        ItemsListBox.Items.Add(new TextBlock
                        {
                            Text = stringBuilder.ToString(),
                            FontStyle = FontStyle.Italic
                        });
                        stringBuilder.Clear();
                    }
                }
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            e.Cancel = _controller.Operation?.IsRunning ?? false;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            SetOperationActive(false);
            AbortButtonToolTip.IsOpen = false;
        }

        public void Dispose()
        {
            _controller.Dispose();
        }
    }
}
