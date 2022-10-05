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
using SimpleZIP_UI.Application.Compression.TreeBuilder;
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
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Serilog;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    public sealed partial class BrowseArchivePage : INavigable, IPasswordRequest, IDisposable
    {
        private readonly ILogger _logger = Log.ForContext<BrowseArchivePage>();

        /// <summary>
        /// Models bound to the list box in view.
        /// </summary>
        public ObservableCollection<ArchiveEntryModel> EntryModels { get; }

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
        /// Root nodes of sub-archives opened through another archive.
        /// </summary>
        private readonly Stack<ArchiveTreeRoot> _rootNodeStack;

        /// <summary>
        /// History when navigating back and to determine the currently active node
        /// (folder) for each archive that is stored in <see cref="_rootNodeStack"/>.
        /// </summary>
        private readonly List<Stack<NavNode>> _nodeStackList;

        /// <summary>
        /// Convenience property which returns the currently active
        /// node in the currently opened archive. When browsing
        /// an archive this would be the currently displayed folder.
        /// </summary>
        private NavNode ActiveNode => GetNodesForCurrentRoot().Peek();

        /// <summary>
        /// The currently opened archive's root node.
        /// </summary>
        private ArchiveTreeRoot _curRootNode;

        /// <summary>
        /// Current position in <see cref="_rootNodeStack"/>. A negative
        /// value indicates that no archives are currently on the stack.
        /// </summary>
        private int _rootNodeStackPointer = -1;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public BrowseArchivePage()
        {
            InitializeComponent();
            _controller = new BrowseArchiveController(this, this);
            _selectedModels = new HashSet<ArchiveEntryModel>();
            _nodeStackList = new List<Stack<NavNode>>(InitialStackCapacity);
            _rootNodeStack = new Stack<ArchiveTreeRoot>(InitialStackCapacity);
            EntryModels = new ObservableCollection<ArchiveEntryModel>();
        }

        private async Task<bool> NavigateUp()
        {
            bool navigateSuccess = false;
            // can only go back in history if stack holds at 
            // least two elements (since first node is root node)
            if (GetNodesForCurrentRoot().Count > 1 && !_controller.IsNavigating)
            {
                navigateSuccess = true;
                GetNodesForCurrentRoot().Pop(); // remove current
                var next = GetNodesForCurrentRoot().Pop();
                IsProgressBarEnabled.IsTrue = true;
                await UpdateListContentAsync(next.Node, next.Sorting);
            }
            // or if sub-archives have been opened (archive within archive)
            else if (_rootNodeStack.Count > 1 && !_controller.IsNavigating)
            {
                navigateSuccess = true;
                GetNodesForCurrentRoot().Clear();
                _rootNodeStack.Pop(); // remove current
                --_rootNodeStackPointer;
                _curRootNode = _rootNodeStack.Peek();
                var children = GetNodesForCurrentRoot();

                IsProgressBarEnabled.IsTrue = true;
                if (children.IsNullOrEmpty())
                {
                    await UpdateListContentAsync(_curRootNode);
                }
                else
                {
                    // consider sorting
                    var navNode = children.Pop();
                    await UpdateListContentAsync(
                        navNode.Node, navNode.Sorting);
                }
            }
            else
            {
                _controller.CancelReadArchive();
            }

            return navigateSuccess;
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
                GetNodesForCurrentRoot().Peek().Node, _selectedModels.ToArray());
        }

        private async void NavigateUpButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            args.Handled = true;

            if (!await NavigateUp())
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
                else
                {
                    Frame.Navigate(typeof(HomePage));
                }
            }
        }

        private void SortOrderToggleMenuFlyoutItem_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            SortArchiveEntryModels();
        }
        private void SortOrderToggleMenuFlyoutItem_OnClick(object sender, RoutedEventArgs args)
        {
            SortArchiveEntryModels();
        }

        private void SortByNameFlyoutItem_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            SortArchiveEntryModelsByName(IsSortOrderDescending.IsTrue);
        }

        private void SortByNameFlyoutItem_OnClick(object sender, RoutedEventArgs args)
        {
            SortArchiveEntryModelsByName(IsSortOrderDescending.IsTrue);
        }

        private void SortByTypeFlyoutItem_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            SortArchiveEntryModelsByType(IsSortOrderDescending.IsTrue);
        }

        private void SortByTypeFlyoutItem_OnClick(object sender, RoutedEventArgs args)
        {
            SortArchiveEntryModelsByType(IsSortOrderDescending.IsTrue);
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
                        foreach (var child in curNode.Node.Children)
                        {
                            if (child.Name.Equals(addItem.DisplayName, StringComparison.Ordinal) && child.IsBrowsable)
                            {
                                IsProgressBarEnabled.IsTrue = true;
                                await UpdateListContentAsync(child as ArchiveTreeNode);
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
                _selectedModels.Count > 0 && !BrowseArchiveController.IsSingleFileEntryArchive(_curRootNode);
        }

        private async void ItemsListBox_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs args)
        {
            if (args.OriginalSource is FrameworkElement elem &&
                elem.DataContext is ArchiveEntryModel model)
            {
                string errResource = string.Empty;

                try
                {
                    if (model.EntryType == ArchiveEntryModel.ArchiveEntryModelType.Archive)
                    {
                        errResource = "ErrorReadingArchive/Text";
                        await OpenArchive(model);
                    }
                    else if (model.EntryType == ArchiveEntryModel.ArchiveEntryModelType.File)
                    {
                        errResource = "ErrorReadingFile/Text";
                        await OpenFile(model);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Archive or file {ModelName} could not be read.", model.DisplayName);

                    IsProgressBarEnabled.IsTrue = false;
                    string errMsg = !string.IsNullOrEmpty(errResource)
                        ? I18N.Resources.GetString(errResource) : ex.Message;
                    var dialog = DialogFactory.CreateErrorDialog(errMsg);
                    await dialog.ShowAsync();
                }

                args.Handled = true;
            }
        }

        /// <inheritdoc />
        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            StorageFile archive = null;
            // check for file activated event (opened via file explorer)
            var eventArgs = args?.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;
            if (eventArgs?.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
            {
                var fileArgs = eventArgs as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
                var files = fileArgs?.Files;
                if (!files.IsNullOrEmpty())
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    // since extension method already checks for null
                    archive = files?[0] as StorageFile;
                    // add main page to back stack to allow navigation via back button
                    Frame.BackStack.Add(new PageStackEntry(typeof(HomePage),
                        null, new CommonNavigationTransitionInfo()));
                }
            }
            else
            {
                archive = args?.Parameter as StorageFile;
            }

            if (archive == null)
            {
                throw new NullReferenceException(I18N.Resources
                    .GetString("NullReferenceMessage/Text", nameof(archive)));
            }

            await LoadArchive(archive); // load initial archive
            ExtractWholeArchiveButton.IsEnabled = !BrowseArchiveController.IsEmptyArchive(_curRootNode);
        }

        /// <inheritdoc />
        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            if (GetNodesForCurrentRoot().Count > 1 && !_controller.IsNavigating ||
                _rootNodeStack.Count > 1 && !_controller.IsNavigating)
            {
                args.Cancel = true;
            }

            await NavigateUp();
        }

        /// <inheritdoc />
        public void Navigate(Type destinationPageType, object parameter = null)
        {
            if (parameter == null)
            {
                var args = new PageNavigationArgs(typeof(BrowseArchivePage));
                Frame.Navigate(destinationPageType, args);
            }
            else
            {
                Frame.Navigate(destinationPageType, parameter);
            }
        }

        /// <inheritdoc />
        public async Task<string> RequestPassword(string fileName)
        {
            return await Dispatcher.RunTaskAsync(async () =>
            {
                var dialog = DialogFactory.CreateRequestPasswordDialog(fileName);
                await dialog.ShowAsync();
                return dialog.Password;
            });
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
            if (rootNode == null) return;

            _rootNodeStack.Push(rootNode);
            ++_rootNodeStackPointer; // will be zero if new archive

            if (_rootNodeStackPointer >= _nodeStackList.Count)
            {
                _nodeStackList.Add(new Stack<NavNode>(InitialStackCapacity));
            }

            _curRootNode = rootNode;
            await UpdateListContentAsync(rootNode);
        }

        private async Task OpenFile(ArchiveEntryModel model)
        {
            IsProgressBarEnabled.IsTrue = true;
            ModelsList.SelectedItem = model; // in case of multi-selection
            var curNode = GetNodesForCurrentRoot().Peek();
            var file = await _controller.ExtractSubEntry(
                _curRootNode, curNode.Node, model);

            if (file == null)
            {
                throw new FileNotFoundException(I18N.Resources.GetString("SubEntryExtractionFailed/Text"));
            }

            var options = new LauncherOptions { DisplayApplicationPicker = true };
            await Launcher.LaunchFileAsync(file, options);
            IsProgressBarEnabled.IsTrue = false;
        }

        private async Task OpenArchive(ArchiveEntryModel model)
        {
            IsProgressBarEnabled.IsTrue = true;
            ModelsList.SelectedItem = model; // in case of multi-selection
            var curNode = GetNodesForCurrentRoot().Peek();
            var file = await _controller.ExtractSubEntry(_curRootNode, curNode.Node, model);

            if (file == null)
            {
                throw new FileNotFoundException(I18N.Resources.GetString("SubArchiveExtractionFailed/Text"));
            }

            await LoadArchive(file); // pushes first node onto nodeStack and disables progress bar
            ExtractWholeArchiveButton.IsEnabled = !BrowseArchiveController.IsEmptyArchive(_curRootNode);
        }

        private Stack<NavNode> GetNodesForCurrentRoot()
        {
            if (_rootNodeStackPointer >= 0 &&
                _rootNodeStackPointer < _nodeStackList.Count)
            {
                return _nodeStackList[_rootNodeStackPointer];
            }

            return new Stack<NavNode>(0);
        }

        private void SortArchiveEntryModelsByName(bool sortDescending)
        {
            var sorted = sortDescending
                ? EntryModels.OrderByDescending(model => model.DisplayName)
                : EntryModels.OrderBy(model => model.DisplayName);
            SortArchiveEntryModels(sorted);
            ActiveNode.Sorting = new Sorting(SortMode.Name, sortDescending);
        }

        private void SortArchiveEntryModelsByType(bool sortDescending)
        {
            var sorted = sortDescending
                ? EntryModels.OrderByDescending(model => model.EntryType)
                : EntryModels.OrderBy(model => model.EntryType);
            SortArchiveEntryModels(sorted);
            ActiveNode.Sorting = new Sorting(SortMode.Type, sortDescending);
        }

        private void SortArchiveEntryModels(Sorting sorting)
        {
            switch (sorting.SortMode)
            {
                case SortMode.Name:
                    SortArchiveEntryModelsByName(sorting.IsDescending);
                    break;
                case SortMode.Type:
                    SortArchiveEntryModelsByType(sorting.IsDescending);
                    break;
                case SortMode.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sorting));
            }
        }

        private void SortArchiveEntryModels(IEnumerable<ArchiveEntryModel> sorted)
        {
            int newIndex = 0;
            foreach (var model in sorted)
            {
                int oldIndex = EntryModels.IndexOf(model);
                EntryModels.Move(oldIndex, newIndex++);
            }
        }

        private void SortArchiveEntryModels()
        {
            var sorting = new Sorting(
                ActiveNode.Sorting.SortMode,
                IsSortOrderDescending.IsTrue);
            SortArchiveEntryModels(sorting);
        }

        private async Task UpdateListContentAsync(ArchiveTreeNode next, Sorting sorting = null)
        {
            if (next == null) return;

            NavigationLock.Instance.IsLocked = true;
            await Task.Delay(50); // allow for visual updates in UI

            // dispatch for non-blocking UI
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                EntryModels.Clear();
                _selectedModels.Clear();
                var navNode = new NavNode(next);
                GetNodesForCurrentRoot().Push(navNode);

                foreach (var child in next.Children)
                {
                    var model = ArchiveEntryModel.Create(child);
                    EntryModels.Add(model);
                }

                if (sorting != null)
                {
                    SortArchiveEntryModels(sorting);
                    IsSortOrderDescending.IsTrue = sorting.IsDescending;
                }

                // checking stack count since root node name may be different
                AddressBar.Text = GetNodesForCurrentRoot().Count == 1 ? "/" : next.Id;

                IsProgressBarEnabled.IsTrue = false;
                NavigationLock.Instance.IsLocked = false;
            });
        }

        #endregion

        private enum SortMode
        {
            None, Name, Type
        }

        private sealed class Sorting
        {
            internal SortMode SortMode { get; }

            internal bool IsDescending { get; }

            public Sorting()
            {
                SortMode = SortMode.None;
            }

            internal Sorting(SortMode mode, bool descending)
            {
                SortMode = mode;
                IsDescending = descending;
            }
        }

        /// <summary>
        /// Wraps a <see cref="Node"/> and holds information
        /// about sorting (see <see cref="Sorting"/>).
        /// </summary>
        private sealed class NavNode
        {
            internal ArchiveTreeNode Node { get; }

            internal Sorting Sorting { get; set; }

            internal NavNode(ArchiveTreeNode archiveNode)
            {
                Node = archiveNode;
                Sorting = new Sorting();
            }
        }
    }
}
