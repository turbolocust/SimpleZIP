using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Presentation.Control;
using SimpleZIP_UI.Presentation.View.Model;

namespace SimpleZIP_UI.Presentation.View
{
    public sealed partial class BrowseArchivePage : Page
    {
        /// <summary>
        /// The aggregated control instance.
        /// </summary>
        private readonly BrowseArchivePageControl _control;

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
            _control = new BrowseArchivePageControl(this);
            _selectedModels = new HashSet<BrowseArchivePageModel>();
            ArchivePageModels = new ObservableCollection<BrowseArchivePageModel>();
            ScrollViewerToolTip.IsOpen = true;
            ScrollViewerToolTip.IsEnabled = true;
        }

        /// <summary>
        /// Invoked when the button to extract the whole archive has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that have been passed.</param>
        private void ExtractWholeArchiveButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            _control.ExtractWholeArchiveButtonAction();
        }

        /// <summary>
        /// Invoked when the button to only extract the selected entries has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that have been passed.</param>
        private void ExtractSelectedEntriesButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            _control.ExtractSelectedEntriesButtonAction(_selectedModels, _nodeStack.Peek());
        }

        /// <summary>
        /// Invoked when the selection in the list box has changed.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that have been passed.</param>
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
            // enable button if at least one item is selected
            ExtractSelectedEntriesButton.IsEnabled = _selectedModels.Count > 0;
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
            var archive = args.Parameter as StorageFile;
            if (archive == null) throw new NullReferenceException("Cannot handle null parameter.");
            UpdateListContent(await _control.ReadArchive(archive));
            ExtractWholeArchiveButton.IsEnabled = _nodeStack.Any();
            ScrollViewerToolTip.IsOpen = false;
            ScrollViewerToolTip.IsEnabled = false;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // can only go back in history if stack holds at 
            // least two elements, as first one is the root node
            if (_nodeStack.Count > 1 && !_control.IsNavigating)
            {
                e.Cancel = true;
                _nodeStack.Pop();
                UpdateListContent(_nodeStack.Pop());
            }
        }
    }
}
