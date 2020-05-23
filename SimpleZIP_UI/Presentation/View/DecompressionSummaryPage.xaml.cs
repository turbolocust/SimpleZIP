// ==++==
// 
// Copyright (C) 2020 Matthias Fussenegger
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

using SimpleZIP_UI.Application;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Operation.Event;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Controller;
using SimpleZIP_UI.Presentation.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Presentation.Cache;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    public sealed partial class DecompressionSummaryPage : INavigation, IPasswordRequest, IDisposable
    {
        /// <summary>
        /// The aggregated controller instance.
        /// </summary>
        private readonly DecompressionController _controller;

        /// <summary>
        /// A list of selected files for decompression.
        /// </summary>
        private IReadOnlyList<ExtractableItem> _selectedItems;

        /// <inheritdoc />
        public DecompressionSummaryPage()
        {
            InitializeComponent();
            _controller = new DecompressionController(this, this);
            _selectedItems = new List<ExtractableItem>(0);
        }

        private void CheckLockNavigation()
        {
            if (Frame.Parent is NavigationView)
            {
                NavigationLock.Instance.IsLocked = true;
            }
        }

        private void FinishOperation()
        {
            SetOperationActive(false);
            AbortButtonToolTip.IsOpen = false;
            NavigationLock.Instance.IsLocked = false;
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

        private void AbortButtonAction()
        {
            AbortButtonToolTip.IsOpen = true;
            _controller.AbortAction();
        }

        private void AbortButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            AbortButtonAction();
        }

        private void AbortButton_OnKeyDown(object sender, KeyRoutedEventArgs args)
        {
            if (args.Key == VirtualKey.Enter) AbortButtonAction();
        }

        private async Task StartButtonAction()
        {
            if (await _controller.CheckOutputFolder())
            {
                var result = await InitOperation();
                RootNodeCache.CheckInitialize();

                if (_controller.IsShareTargetActivated() &&
                    result.StatusCode != Result.Status.Success)
                {
                    // allow error dialog to be displayed
                    await _controller.CreateResultDialog(result).ShowAsync();
                }
                else
                {
                    await _controller.CreateResultDialog(result).ShowAsync();
                }

                _controller.NavigateBackHome();
            }
            else
            {
                PickOutputFolder();
            }
        }

        private async void StartButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            await StartButtonAction();
        }

        private async void StartButton_OnKeyDown(object sender, KeyRoutedEventArgs args)
        {
            if (args.Key == VirtualKey.Enter) await StartButtonAction();
        }

        private void OutputPathButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            PickOutputFolder();
        }

        /// <summary>
        /// Converts an enumerable of <see cref="StorageFile"/>
        /// to a list consisting of <see cref="ExtractableItem"/>.
        /// </summary>
        /// <param name="files">The list to be converted.</param>
        /// <returns>A list consisting of <see cref="ExtractableItem"/>.</returns>
        private static IReadOnlyList<ExtractableItem> ConvertFiles(IEnumerable<StorageFile> files)
        {
            return files.Select(file => new ExtractableItem(file.Name, file)).ToList();
        }

        /// <summary>
        /// Initializes the archiving operation and waits for the result.
        /// </summary>
        private async Task<Result> InitOperation()
        {
            CheckLockNavigation();
            SetOperationActive(true);

            var infos = new List<DecompressionInfo>(_selectedItems.Count);
            var totalSize = await _controller.CheckFileSizes(_selectedItems);

            infos.AddRange(_selectedItems.Select(item =>
                new DecompressionInfo(item, totalSize)
                {
                    Encoding = Encoding.UTF8
                }));

            return await _controller.StartAction(OnProgressUpdate, infos.ToArray());
        }

        /// <summary>
        /// Sets the archiving operation as active.
        /// This results in the UI being busy.
        /// </summary>
        /// <param name="isActive">True to set operation as active,
        /// false to set it as inactive.</param>
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
            // check if file has been opened via file explorer
            var eventArgs = args.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;
            if (eventArgs?.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
            {
                var fileArgs = eventArgs as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
                var files = fileArgs?.Files;
                if (!files.IsNullOrEmpty())
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    var storageFiles = files.Select(file => file as StorageFile).ToList();
                    _selectedItems = ConvertFiles(storageFiles);
                }
            }
            // navigated from MainPage
            else if (args.Parameter is FilesNavigationArgs navigationArgs)
            {
                _selectedItems = ConvertFiles(navigationArgs.StorageFiles);
                _controller.ShareOperation = navigationArgs.ShareOperation;
            }
            // navigated from Archive Browser
            else if (args.Parameter is ExtractableItem extractableItem)
            {
                _selectedItems = new[] { extractableItem };
            }
            // populate list
            foreach (var item in _selectedItems)
            {
                // ReSharper disable once PossibleNullReferenceException
                ItemsListBox.Items.Add(new TextBlock { Text = item.Name });
                if (!item.Entries.IsNullOrEmpty()) // add entries with indent as well
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var entry in item.Entries)
                    {
                        stringBuilder.Append("-> ").Append(entry.Key);
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

        /// <inheritdoc />
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs args)
        {
            args.Cancel = _controller.Operation?.IsRunning ?? false;
        }

        /// <inheritdoc />
        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            FinishOperation();
        }

        /// <inheritdoc />
        public async Task<string> RequestPassword(string fileName)
        {
            var dialog = DialogFactory.CreateRequestPasswordDialog(fileName);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await dialog.ShowAsync();
            });

            return dialog.Password;
        }

        /// <inheritdoc />
        public void Navigate(Type destinationPageType, object parameter = null)
        {
            if (parameter == null)
            {
                var args = new PageNavigationArgs(typeof(DecompressionSummaryPage));
                Frame.Navigate(destinationPageType, args);
            }
            else
            {
                Frame.Navigate(destinationPageType, parameter);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _controller.Dispose();
        }
    }
}
