using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Application.Compression.Reader;
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
        /// Models bound to the list box in view.
        /// </summary>
        public ObservableCollection<BrowseArchivePageModel> ArchivePageModels { get; }

        /// <summary>
        /// Set consisting of selected models in view.
        /// </summary>
        private readonly ISet<BrowseArchivePageModel> _selectedModels;

        /// <summary>
        /// Used as a history when navigating back and to determine currently active node.
        /// </summary>
        private readonly Stack<Node> _nodeStack;

        public BrowseArchivePage()
        {
            InitializeComponent();
            _control = new BrowseArchivePageControl(this);
            _selectedModels = new HashSet<BrowseArchivePageModel>();
            _nodeStack = new Stack<Node>();
            ArchivePageModels = new ObservableCollection<BrowseArchivePageModel>();
        }

        private void ConfirmSelection_Tap(object sender, TappedRoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void ItemsListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // add items from selection
            foreach (var item in e.AddedItems)
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
            foreach (var item in e.RemovedItems)
            {
                if (item is BrowseArchivePageModel removeItem)
                {
                    _selectedModels.Remove(removeItem);
                }
            }
            // enable button if at least one item is selected
            ConfirmSelectionButton.IsEnabled = _selectedModels.Count > 0;
        }

        private void UpdateListContent(Node nextNode)
        {
            ArchivePageModels.Clear();
            _selectedModels.Clear();
            _nodeStack.Push(nextNode);

            foreach (var child in nextNode.Children)
            {
                var isNode = child.IsNode;
                var model = new BrowseArchivePageModel(isNode)
                {
                    Symbol = isNode ? Symbol.OpenLocal : Symbol.Preview,
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
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (_nodeStack.Count > 1)
            {
                e.Cancel = true;
                _nodeStack.Pop();
                UpdateListContent(_nodeStack.Pop());
            }
        }
    }
}
