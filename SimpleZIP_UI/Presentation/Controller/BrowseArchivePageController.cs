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
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Factory;
using SimpleZIP_UI.Presentation.View;
using SimpleZIP_UI.Presentation.View.Model;

namespace SimpleZIP_UI.Presentation.Controller
{
    internal class BrowseArchivePageController : BaseController
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

        internal BrowseArchivePageController(Page parent) : base(parent)
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
                catch (Exception ex)
                {
                    var resource = ex is InvalidOperationException
                        ? "FileFormatNotSupported/Text" : "ErrorReadingArchive/Text";
                    var dialog = DialogFactory.CreateErrorDialog(resource);
                    dialog.ShowAsync().AsTask().Forget();
                    ParentPage.Frame.Navigate(typeof(MainPage));
                }
                finally
                {
                    if (rootNode == null)
                    {
                        _archiveFile = null;
                    }
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
