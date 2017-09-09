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
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Controller;
using SimpleZIP_UI.Presentation.View.Model;

namespace SimpleZIP_UI.Presentation.View
{
    public sealed partial class BrowseArchivePage
    {
        /// <summary>
        /// The aggregated control instance.
        /// </summary>
        private readonly BrowseArchivePageController _controller;

        /// <summary>
        /// Set consisting of selected models in view.
        /// </summary>
        private readonly ISet<BrowseArchivePageModel> _selectedModels;

        /// <summary>
        /// Used as a history when navigating back and to determine the currently active node.
        /// </summary>
        private readonly Stack<Node> _nodeStack = new Stack<Node>();

        /// <summary>
        /// Models bound to the list box in view.
        /// </summary>
        public ObservableCollection<BrowseArchivePageModel> ArchivePageModels { get; }

        public BrowseArchivePage()
        {
            InitializeComponent();
            _controller = new BrowseArchivePageController(this);
            _selectedModels = new HashSet<BrowseArchivePageModel>();
            ArchivePageModels = new ObservableCollection<BrowseArchivePageModel>();
            ProgressBar.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Invoked when the button to extract the whole archive has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void ExtractWholeArchiveButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            _controller.ExtractWholeArchiveButtonAction();
        }

        /// <summary>
        /// Invoked when the button to only extract the selected entries has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void ExtractSelectedEntriesButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            _controller.ExtractSelectedEntriesButtonAction(_selectedModels, _nodeStack.Peek());
        }

        /// <summary>
        /// Invoked when the selection in the list box has changed.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void ItemsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            // add items from selection
            foreach (var item in args.AddedItems)
            {
                if (item is BrowseArchivePageModel addItem)
                {
                    if (addItem.IsNode)
                    {
                        foreach (var child in _nodeStack.Peek().Children)
                        {
                            if (child.Name.Equals(addItem.DisplayName) && child.IsNode)
                            {
                                var nextNode = child as Node;
                                UpdateListContent(nextNode);
                                return;
                            }
                        }
                    }
                    _selectedModels.Add(addItem);
                }
            }
            // remove items from selection
            foreach (var item in args.RemovedItems)
            {
                if (item is BrowseArchivePageModel removeItem)
                {
                    _selectedModels.Remove(removeItem);
                }
            }
            // enable button if at least one item is selected and
            // archive does not consist of one file entry only
            ExtractSelectedEntriesButton.IsEnabled =
                _selectedModels.Count > 0 && !_controller.IsSingleFileEntryArchive();
        }

        /// <summary>
        /// Populates the list box with the content of the specified node.
        /// </summary>
        /// <param name="nextNode">The node whose content will be used 
        /// to populate the list box.</param>
        private void UpdateListContent(Node nextNode)
        {
            if (nextNode == null) return;

            ArchivePageModels.Clear();
            _selectedModels.Clear();
            _nodeStack.Push(nextNode);

            foreach (var child in nextNode.Children)
            {
                var isNode = child.IsNode;
                var model = new BrowseArchivePageModel(isNode)
                {
                    Symbol = isNode ? Symbol.Folder : Symbol.Preview,
                    DisplayName = child.Name
                };
                ArchivePageModels.Add(model);
            }
        }

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
                    // extension method already checks for null
                    archive = files.First() as StorageFile;
                }
            }
            else
            {
                archive = args.Parameter as StorageFile;
            }

            if (archive == null)
                throw new NullReferenceException("Cannot handle null parameter.");

            UpdateListContent(await _controller.ReadArchive(archive));
            ExtractWholeArchiveButton.IsEnabled = !_controller.IsEmptyArchive();
            ProgressBar.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // can only go back in history if stack holds at 
            // least two elements (since first node is root node)
            if (_nodeStack.Count > 1 && !_controller.IsNavigating)
            {
                e.Cancel = true;
                _nodeStack.Pop();
                UpdateListContent(_nodeStack.Pop());
            }
        }
    }
}
