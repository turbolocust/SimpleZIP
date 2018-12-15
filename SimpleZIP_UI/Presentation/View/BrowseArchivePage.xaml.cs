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

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    public sealed partial class BrowseArchivePage : INavigation, IPasswordRequest, IDisposable
    {
        /// <summary>
        /// Models bound to the list box in view.
        /// </summary>
        public ObservableCollection<ArchiveEntryModel> ArchiveEntryModels { get; }

        /// <summary>
        /// Enables or disables the progress bar.
        /// </summary>
        public BooleanModel IsProgressBarEnabled { get; set; } = true;

        /// <summary>
        /// True for descending sort order, false for ascending.
        /// </summary>
        public BooleanModel IsSortOrderDescending { get; set; } = false;

        /// <summary>
        /// Initial capacity of data structures holding archive nodes.
        /// </summary>
        private const int InitialStackCapacity = 1 << 3;

        /// <summary>
        /// The aggregated controller instance.
        /// </summary>
        private readonly BrowseArchiveController _controller;

        /// <summary>
        /// Set consisting of selected models in view.
        /// </summary>
        private readonly ISet<ArchiveEntryModel> _selectedModels;

        /// <summary>
        /// History when navigating back and to determine the currently active node
        /// for each archive that is stored in <see cref="_rootNodeStack"/>.
        /// </summary>
        private readonly List<Stack<Node>> _nodeStackList;

        /// <summary>
        /// Root nodes of sub-archives opened through another archive.
        /// </summary>
        private readonly Stack<RootNode> _rootNodeStack;

        /// <summary>
        /// The currently opened archive's root node.
        /// </summary>
        private RootNode _curRootNode;

        /// <summary>
        /// Current position in <see cref="_rootNodeStack"/>. A negative
        /// value indicates that no archives are currently on the stack.
        /// </summary>
        private int _rootNodeStackPointer = -1;

        /// <summary>
        /// To determine the ordering of the elements in <see cref="ArchiveEntryModels"/>.
        /// </summary>
        private SortMode _sortMode = SortMode.None;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public BrowseArchivePage()
        {
            InitializeComponent();
            _controller = new BrowseArchiveController(this, this);
            _selectedModels = new HashSet<ArchiveEntryModel>();
            _nodeStackList = new List<Stack<Node>>(InitialStackCapacity);
            _rootNodeStack = new Stack<RootNode>(InitialStackCapacity);
            ArchiveEntryModels = new ObservableCollection<ArchiveEntryModel>();
        }

        private void ExtractWholeArchiveButton_OnTapped(
            object sender, TappedRoutedEventArgs args)
        {
            _controller.ExtractWholeArchiveButtonAction(_curRootNode);
        }

        private void ExtractSelectedEntriesButton_OnTapped(
            object sender, TappedRoutedEventArgs args)
        {
            _controller.ExtractSelectedEntriesButtonAction(_curRootNode,
                GetNodesForCurrentRoot().Peek(), _selectedModels.ToArray());
        }

        private void NavigateUpButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            if (Window.Current.Content is Frame frame && frame.CanGoBack)
            {
                args.Handled = true;
                frame.GoBack();
            }
        }

        private void SortOrderToggleMenuFlyoutItem_OnTapped(
            object sender, TappedRoutedEventArgs args)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (_sortMode)
            {
                case SortMode.Name:
                    SortArchiveEntryModelsByName();
                    break;
                case SortMode.Type:
                    SortArchiveEntryModelsByType();
                    break;
            }
        }

        private void SortByNameFlyoutItem_OnTapped(
            object sender, TappedRoutedEventArgs args)
        {
            SortArchiveEntryModelsByName();
        }

        private void SortByTypeFlyoutItem_OnTapped(
            object sender, TappedRoutedEventArgs args)
        {
            SortArchiveEntryModelsByType();
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
                        var curNode = GetNodesForCurrentRoot().Peek();
                        foreach (var child in curNode.Children)
                        {
                            if (child.Name.Equals(addItem.DisplayName) && child.IsBrowsable)
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
                !_controller.IsSingleFileEntryArchive(_curRootNode);
        }

        private async void ItemsListBox_OnDoubleTapped(
            object sender, DoubleTappedRoutedEventArgs args)
        {
            if (args.OriginalSource is FrameworkElement elem &&
                elem.DataContext is ArchiveEntryModel model)
            {
                if (model.EntryType == ArchiveEntryModel.ArchiveEntryModelType.Archive)
                {
                    try
                    {
                        IsProgressBarEnabled.IsTrue = true;
                        ModelsList.SelectedItem = model; // in case of multi-selection
                        var curNode = GetNodesForCurrentRoot().Peek();
                        var file = await _controller.ExtractSubArchive(_curRootNode, curNode, model);
                        if (file == null) // something went wrong
                        {
                            throw new FileNotFoundException(
                                "File not found. Extraction of sub archive failed.");
                        }
                        await LoadArchive(file); // will push first node onto nodeStack
                        ExtractWholeArchiveButton.IsEnabled
                            = !_controller.IsEmptyArchive(_curRootNode);
                    }
                    catch (Exception)
                    {
                        string errMsg = I18N.Resources.GetString("ErrorReadingArchive/Text");
                        var dialog = DialogFactory.CreateErrorDialog(errMsg);
                        await dialog.ShowAsync();
                    }
                }

                args.Handled = true;
            }
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

            await LoadArchive(archive); // load initial archive
            ExtractWholeArchiveButton.IsEnabled
                = !_controller.IsEmptyArchive(_curRootNode);
        }

        /// <inheritdoc />
        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs args)
        {
            // can only go back in history if stack holds at 
            // least two elements (since first node is root node)
            if (GetNodesForCurrentRoot().Count > 1 && !_controller.IsNavigating)
            {
                args.Cancel = true;
                GetNodesForCurrentRoot().Pop(); // remove current
                var next = GetNodesForCurrentRoot().Pop();
                IsProgressBarEnabled.IsTrue = true;
                await UpdateListContentAsync(next);
            }
            // or if sub-archives have been opened (archive within archive)
            else if (_rootNodeStack.Count > 1 && !_controller.IsNavigating)
            {
                args.Cancel = true;
                GetNodesForCurrentRoot().Clear();
                _rootNodeStack.Pop(); // remove current
                --_rootNodeStackPointer;
                _curRootNode = _rootNodeStack.Peek();
                var children = GetNodesForCurrentRoot();
                var next = children.IsNullOrEmpty()
                    ? _curRootNode : children.Pop();
                IsProgressBarEnabled.IsTrue = true;
                await UpdateListContentAsync(next);
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
            await Dispatcher.RunTaskAsync(dialog.ShowAsync().AsTask);
            return dialog.Password;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _controller.Dispose();
        }

        #region Private helper methods
        private async Task LoadArchive(StorageFile archive)
        {
            var rootNode = await _controller.ReadArchive(archive);
            _rootNodeStack.Push(rootNode);
            ++_rootNodeStackPointer; // will be zero if new archive

            if (_rootNodeStackPointer >= _nodeStackList.Count)
            {
                _nodeStackList.Add(new Stack<Node>(InitialStackCapacity));
            }

            _curRootNode = rootNode;
            await UpdateListContentAsync(rootNode);
        }

        private Stack<Node> GetNodesForCurrentRoot()
        {
            if (_rootNodeStackPointer >= 0 &&
                _rootNodeStackPointer < _nodeStackList.Count)
            {
                return _nodeStackList[_rootNodeStackPointer];
            }

            return new Stack<Node>(0);
        }

        private void SortArchiveEntryModelsByName()
        {
            var sorted = IsSortOrderDescending.IsTrue
                ? ArchiveEntryModels.OrderByDescending(model => model.DisplayName)
                : ArchiveEntryModels.OrderBy(model => model.DisplayName);
            SortArchiveEntryModels(sorted);
            _sortMode = SortMode.Name;
        }

        private void SortArchiveEntryModelsByType()
        {
            var sorted = IsSortOrderDescending.IsTrue
                ? ArchiveEntryModels.OrderByDescending(model => model.EntryType)
                : ArchiveEntryModels.OrderBy(model => model.EntryType);
            SortArchiveEntryModels(sorted);
            _sortMode = SortMode.Type;
        }

        private void SortArchiveEntryModels(IEnumerable<ArchiveEntryModel> sorted)
        {
            int newIndex = 0;
            foreach (var model in sorted)
            {
                int oldIndex = ArchiveEntryModels.IndexOf(model);
                ArchiveEntryModels.Move(oldIndex, newIndex++);
            }
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
                GetNodesForCurrentRoot().Push(nextNode);

                foreach (var child in nextNode.Children)
                {
                    var model = ArchiveEntryModel.Create(child);
                    ArchiveEntryModels.Add(model);
                }

                IsProgressBarEnabled.IsTrue = false;

                // checking stack count since root node name may be different
                AddressBar.Text = GetNodesForCurrentRoot().Count == 1 ? "/" : nextNode.Id;
            });
        }
        #endregion

        private enum SortMode
        {
            None, Name, Type
        }
    }
}
