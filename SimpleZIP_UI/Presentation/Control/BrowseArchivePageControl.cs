using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Factory;
using SimpleZIP_UI.Presentation.View;
using SimpleZIP_UI.Presentation.View.Model;

namespace SimpleZIP_UI.Presentation.Control
{
    internal class BrowseArchivePageControl : BaseControl
    {
        /// <summary>
        /// The associated archive. Will hold a reference to a storage file 
        /// once the <see cref="ReadArchive"/> method has been invoked.
        /// </summary>
        private static StorageFile _archiveFile;

        /// <summary>
        /// Static reference to the root node. Works like a cache and 
        /// allows faster resumption when navigating back to this page.
        /// </summary>
        private static Node _rootNode;

        /// <summary>
        /// True if navigation is about to be executed, false otherwise.
        /// </summary>
        internal bool IsNavigating { get; private set; }

        internal BrowseArchivePageControl(Page parent) : base(parent)
        {
        }

        /// <summary>
        /// Reads the specified archive and returns its root node.
        /// </summary>
        /// <param name="archive">The archive to be read.</param>
        /// <returns>The root node of the archive.</returns>
        internal async Task<Node> ReadArchive(StorageFile archive)
        {
            if (_archiveFile != null && _archiveFile.IsEqual(archive)) return _rootNode;

            _archiveFile = archive;
            using (var reader = new ArchiveReader())
            {
                Node rootNode = null;
                try
                {
                    rootNode = await reader.Read(archive);
                }
                catch (IOException)
                {
                    var dialog = DialogFactory.CreateErrorDialog(
                        I18N.Resources.GetString("ErrorReadingArchive/Text"));
                    dialog.ShowAsync().AsTask().Forget();
                }
                return _rootNode = rootNode;
            }
        }

        /// <summary>
        /// Navigates to <see cref="DecompressionSummaryPage"/> with the archive 
        /// file (<see cref="_archiveFile"/>) as a parameter.
        /// </summary>
        public void ExtractWholeArchiveButtonAction()
        {
            IsNavigating = true;
            var item = new ExtractableItem(_archiveFile.Name, _archiveFile);
            ParentPage.Frame.Navigate(typeof(DecompressionSummaryPage), new[] { item });
        }

        /// <summary>
        /// Converts each model from the specified collection to <see cref="ExtractableItem"/> 
        /// and navigates to <see cref="DecompressionSummaryPage"/> afterwards.
        /// </summary>
        /// <param name="models">The models to be converted.</param>
        /// <param name="node">The currently active node that holds equivalents of the models.</param>
        internal void ExtractSelectedEntriesButtonAction(ICollection<BrowseArchivePageModel> models, Node node)
        {
            var entries = new List<FileEntry>(models.Count);
            foreach (var model in models)
            {
                foreach (var child in node.Children)
                {
                    if (child.Name.Equals(model.DisplayName))
                    {
                        if (child is FileEntry entry)
                        {
                            entries.Add(entry);
                            break;
                        }
                    }
                }
            }
            if (entries.Count > 0)
            {
                IsNavigating = true;
                var item = new ExtractableItem(_archiveFile.Name, _archiveFile, entries);
                ParentPage.Frame.Navigate(typeof(DecompressionSummaryPage), new[] { item });
            }
        }
    }
}
