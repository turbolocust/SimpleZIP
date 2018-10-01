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

using SimpleZIP_UI.Application;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Controller;
using SimpleZIP_UI.Presentation.Factory;
using SimpleZIP_UI.Presentation.View.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    public sealed partial class BrowseArchivePage : INavigation, IPasswordRequest, IDisposable
    {
        /// <summary>
        /// The aggregated controller instance.
        /// </summary>
        private readonly BrowseArchiveController _controller;

        /// <summary>
        /// Set consisting of selected models in view.
        /// </summary>
        private readonly ISet<ArchiveEntryModel> _selectedModels;

        /// <summary>
        /// History when navigating back and to determine the currently active node.
        /// </summary>
        private readonly Stack<Node> _nodeStack = new Stack<Node>();

        /// <summary>
        /// Nodes of any sub-archives opened through another archive.
        /// </summary>
        private readonly Stack<Node> _archivesStack = new Stack<Node>();

        /// <summary>
        /// Models bound to the list box in view.
        /// </summary>
        public ObservableCollection<ArchiveEntryModel> ArchiveEntryModels { get; }

        /// <summary>
        /// Enables or disables the progress bar.
        /// </summary>
        public BooleanModel IsProgressBarEnabled { get; set; } = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public BrowseArchivePage()
        {
            InitializeComponent();
            _controller = new BrowseArchiveController(this, this);
            _selectedModels = new HashSet<ArchiveEntryModel>();
            ArchiveEntryModels = new ObservableCollection<ArchiveEntryModel>();
        }

        private void ExtractWholeArchiveButton_OnTapped(
            object sender, TappedRoutedEventArgs args)
        {
            _controller.ExtractWholeArchiveButtonAction();
        }

        private void ExtractSelectedEntriesButton_OnTapped(
            object sender, TappedRoutedEventArgs args)
        {
            _controller.ExtractSelectedEntriesButtonAction(
                _nodeStack.Peek(), _selectedModels.ToArray());
        }

        private void NavigateUpButton_OnTapped(
            object sender, TappedRoutedEventArgs args)
        {
            if (Window.Current.Content is Frame frame && frame.CanGoBack)
            {
                args.Handled = true;
                frame.GoBack();
            }
        }

        private async void ItemsListBox_OnSelectionChanged(
            object sender, SelectionChangedEventArgs args)
        {
            // add items from selection
            foreach (var item in args.AddedItems)
            {
                if (item is ArchiveEntryModel addItem)
                {
                    if (addItem.EntryType == ArchiveEntryModel.ArchiveEntryModelType.Node)
                    {
                        foreach (var child in _nodeStack.Peek().Children)
                        {
                            if (child.Name.Equals(addItem.DisplayName))
                            {
                                IsProgressBarEnabled.IsTrue = true;
                                await UpdateListContentAsync(child as Node);
                                return; // since node is a new folder
                            }
                        }
                    }
                    _selectedModels.Add(addItem);
                }
            }
            // remove items from selection
            foreach (var item in args.RemovedItems)
            {
                if (item is ArchiveEntryModel removeItem)
                {
                    _selectedModels.Remove(removeItem);
                }
            }
            // enable button if at least one item is selected and
            // archive does not only consist of a single file entry
            ExtractSelectedEntriesButton.IsEnabled =
                _selectedModels.Count > 0 &&
                !_controller.IsSingleFileEntryArchive();
        }

        private async void ItemsListBox_OnDoubleTapped(
            object sender, DoubleTappedRoutedEventArgs args)
        {
            if (sender is ListBox list &&
                args.OriginalSource is Rectangle rect &&
                rect.DataContext is ArchiveEntryModel model)
            {
                if (model.EntryType == ArchiveEntryModel.ArchiveEntryModelType.Archive)
                {
                    try
                    {
                        list.SelectedItem = model; // in case of multi-selection
                        var file = await _controller.ExtractSubArchive(_nodeStack.Peek(), model);
                        if (file == null) // something went wrong
                        {
                            throw new FileNotFoundException("File not found. Extraction of sub archive failed.");
                        }
                        _nodeStack.Clear(); // since we're loading a new archive
                        await LoadArchive(file); // will push first node onto nodeStack
                        ExtractWholeArchiveButton.IsEnabled = !_controller.IsEmptyArchive();
                    }
                    catch (Exception)
                    {
                        string errMsg = I18N.Resources.GetString("ErrorReadingArchive/Text");
                        var dialog = DialogFactory.CreateErrorDialog(errMsg);
                        await dialog.ShowAsync();
                    }
                }
            }

            args.Handled = true;
        }

        private async Task<Node> LoadArchive(StorageFile archive)
        {
            var rootNode = await _controller.ReadArchive(archive);
            await UpdateListContentAsync(rootNode);
            _archivesStack.Push(rootNode);
            return rootNode;
        }

        private async Task UpdateListContentAsync(Node nextNode)
        {
            if (nextNode == null) return;

            await Task.Delay(50); // allow visual updates in UI

            // dispatch for non-blocking UI
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ArchiveEntryModels.Clear();
                _selectedModels.Clear();
                _nodeStack.Push(nextNode);

                foreach (var child in nextNode.Children)
                {
                    var model = ArchiveEntryModel.Create(child);
                    ArchiveEntryModels.Add(model);
                }

                IsProgressBarEnabled.IsTrue = false;

                // checking stack count since root node name may be different
                AddressBar.Text = _nodeStack.Count == 1 ? "/" : nextNode.Id;
            });
        }

        /// <inheritdoc />
        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            StorageFile archive = null;
            // check for file activated event (opened via file explorer)
            var eventArgs = args.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;
            if (eventArgs?.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
            {
                var fileArgs = eventArgs as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
                var files = fileArgs?.Files;
                if (!files.IsNullOrEmpty())
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    // since extension method already checks for null
                    archive = files.First() as StorageFile;
                    // add main page to back stack to allow navigation via back button
                    Frame.BackStack.Add(new PageStackEntry(typeof(MainPage), null,
                        new CommonNavigationTransitionInfo()));
                }
            }
            else
            {
                archive = args.Parameter as StorageFile;
            }

            if (archive == null)
                throw new NullReferenceException("Cannot handle null parameter.");

            await LoadArchive(archive);
            ExtractWholeArchiveButton.IsEnabled = !_controller.IsEmptyArchive();
        }

        /// <inheritdoc />
        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs args)
        {
            // can only go back in history if stack holds at 
            // least two elements (since first node is root node)
            if (_nodeStack.Count > 1 && !_controller.IsNavigating)
            {
                args.Cancel = true;
                _nodeStack.Pop(); // remove current
                IsProgressBarEnabled.IsTrue = true;
                await UpdateListContentAsync(_nodeStack.Pop());
            }
            else if (_archivesStack.Count > 0 && !_controller.IsNavigating)
            {
                args.Cancel = true;
                _archivesStack.Pop(); // remove current
                IsProgressBarEnabled.IsTrue = true;
                await UpdateListContentAsync(_archivesStack.Peek());
            }
            else
            {
                _controller.CancelReadArchive();
            }
        }

        /// <inheritdoc />
        public void Navigate(Type destinationPageType, object parameter = null)
        {
            if (parameter == null)
            {
                Frame.Navigate(destinationPageType);
            }
            else
            {
                Frame.Navigate(destinationPageType, parameter);
            }
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
        public void Dispose()
        {
            _controller.Dispose();
        }
    }
}
