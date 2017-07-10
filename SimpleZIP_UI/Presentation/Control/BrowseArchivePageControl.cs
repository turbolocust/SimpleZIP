using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Reader;
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
        private StorageFile _archiveFile;

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
            _archiveFile = archive;
            using (var reader = new ArchiveReader())
            {
                await reader.OpenArchiveAsync(archive, false);
                return reader.Read();
            }
        }

        /// <summary>
        /// Converts each model from the specified collection to <see cref="ExtractableItem"/>.
        /// </summary>
        /// <param name="models">The models to be converted.</param>
        /// <param name="node">The currently active node that holds equivalents of the models.</param>
        internal void ConfirmationButtonAction(ICollection<BrowseArchivePageModel> models, Node node)
        {
            var items = new List<FileEntry>(models.Count);
            foreach (var model in models)
            {
                foreach (var child in node.Children)
                {
                    if (child.Name.Equals(model.DisplayName))
                    {
                        if (child is FileEntry entry)
                        {
                            items.Add(entry);
                        }
                    }
                }
            }
            if (items.Count > 0)
            {
                var item = new ExtractableItem(_archiveFile.DisplayName, _archiveFile, items);
                ParentPage.Frame.Navigate(typeof(DecompressionSummaryPage), item);
                ParentPage.Frame.Navigate(typeof(DecompressionSummaryPage), item);
            }
        }
    }
}
