// ==++==
// 
// Copyright (C) 2019 Matthias Fussenegger
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
using SimpleZIP_UI.Application.Compression.Operation;
using SimpleZIP_UI.Application.Compression.Operation.Job;
using SimpleZIP_UI.Application.Compression.TreeBuilder;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.I18N;
using SimpleZIP_UI.Presentation.Factory;
using SimpleZIP_UI.Presentation.Handler;
using SimpleZIP_UI.Presentation.View;
using SimpleZIP_UI.Presentation.View.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Presentation.Controller
{
    internal sealed class BrowseArchiveController : BaseController, ICancelRequest, IDisposable
    {
        /// <inheritdoc />
        public bool IsCancelRequest { get; private set; }

        /// <summary>
        /// True if navigation is about to be executed, false otherwise.
        /// </summary>
        internal bool IsNavigating { get; private set; }

        /// <summary>
        /// Source for cancellation token.
        /// </summary>
        private readonly CancellationTokenSource _tokenSource;

        internal BrowseArchiveController(INavigation navHandler,
            IPasswordRequest pwRequest) : base(navHandler, pwRequest)
        {
            _tokenSource = new CancellationTokenSource();
        }

        /// <inheritdoc />
        public void Reset()
        {
            IsCancelRequest = false;
        }

        /// <summary>
        /// Reads the specified archive and returns its root node.
        /// </summary>
        /// <param name="archive">The archive to be read.</param>
        /// <returns>The root node of the archive.</returns>
        internal async Task<ArchiveTreeRoot> ReadArchive(StorageFile archive)
        {
            string key = !string.IsNullOrEmpty(archive.Path)
                ? archive.Path : archive.FolderRelativeId;
            var node = RootNodeCacheHandler.Instance.ReadFromCache(key);
            if (node != null) // return immediately if cached
            {
                return node;
            }

            string password = string.Empty;

            try
            {
                try
                {
                    using (var treeBuilder = new ArchiveTreeBuilder(_tokenSource.Token))
                    {
                        node = await treeBuilder.Build(archive);
                    }
                }
                catch (ArchiveEncryptedException)
                {
                    // archive is encrypted, ask for password and try again
                    password = await PasswordRequest.RequestPassword(archive.DisplayName);
                    using (var treeBuilder = new ArchiveTreeBuilder(_tokenSource.Token))
                    {
                        node = await treeBuilder.Build(archive, password);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // ignore, because we're navigating back
                // to previous archive or calling page
                // after cancelling the operation
            }
            catch (Exception ex)
            {
                var opType = password != null
                    ? ExceptionMessages.OperationType.ReadingPasswordSet
                    : ExceptionMessages.OperationType.Reading;

                string message = await ExceptionMessages.GetStringFor(ex, opType, archive);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                DialogFactory.CreateErrorDialog(message).ShowAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Navigation.Navigate(typeof(HomePage));
            }
            finally
            {
                if (node != null)
                {
                    RootNodeCacheHandler.Instance.WriteToCache(key, node);
                }
            }

            return node;
        }

        /// <summary>
        /// Extracts a sub-entry within the current archive.
        /// </summary>
        /// <param name="root">The root node of the sub-entry.</param>
        /// <param name="node">The currently active node that
        /// holds the equivalent of the model.</param>
        /// <param name="model">The model to be converted.</param>
        /// <returns>A task which returns the extracted sub-entry.</returns>
        internal async Task<StorageFile> ExtractSubEntry(ArchiveTreeRoot root,
            ArchiveTreeNode node, ArchiveEntryModel model)
        {
            StorageFile subFile = null;
            // find entry in children of current node
            var entry = (from child in node.Children
                         where child.Name.Equals(model.DisplayName)
                         select child as ArchiveTreeFile).FirstOrDefault();

            if (entry != null)
            {
                var folder = await FileUtils
                    .GetTempFolderAsync(TempFolder.Archives);
                if (!string.IsNullOrEmpty(entry.FileName))
                {
                    try
                    {
                        return await FileUtils
                            .GetFileAsync(folder, entry.FileName);
                    }
                    catch (FileNotFoundException)
                    {
                        // continue, as file most likely got deleted
                    }
                }
                // doesn't exist, hence extract and read again
                var item = new ExtractableItem(
                    root.Archive.Name,
                    root.Archive,
                    new[] { entry.ToArchiveEntry() });
                var size = await FileUtils.GetFileSizeAsync(item.Archive);
                // create operation and job for execution
                var operationInfo = new DecompressionInfo(item, size)
                {
                    OutputFolder = folder,
                    IsCollectFileNames = true,
                    Encoding = Encoding.UTF8
                };
                var operation = Operations.ForDecompression();
                var job = new DecompressionJob(operation, operationInfo)
                {
                    PasswordRequest = PasswordRequest
                };
                var result = await job.Run(this); // extraction happens here
                if (result.StatusCode == Result.Status.Success)
                {
                    entry.FileName = item.Entries[0].FileName;
                    subFile = await FileUtils.GetFileAsync(folder, entry.FileName);
                }
            }

            return subFile;
        }

        /// <summary>
        /// Cancels any ongoing <see cref="ReadArchive"/> operation.
        /// </summary>
        internal void CancelReadArchive()
        {
            if (!_tokenSource.IsCancellationRequested)
            {
                IsCancelRequest = true;
                _tokenSource.Cancel();
            }
        }

        /// <summary>
        /// Checks whether the currently opened archive only consists
        /// of one file entry. This evaluates to true if the opened 
        /// archive is e.g. a GZIP compressed file.
        /// </summary>
        /// <param name="root">The root node of the archive.</param>
        /// <returns>True if archive only consists of one file entry.</returns>
        internal bool IsSingleFileEntryArchive(ArchiveTreeRoot root)
        {
            return root.Children.Count == 1
                && !root.Children.First().IsBrowsable;
        }

        /// <summary>
        /// Checks whether the currently opened archive contains
        /// any elements (including files and folders) or not.
        /// </summary>
        /// <param name="root">The root node of the archive.</param>
        /// <returns>True if archive is empty, false otherwise.</returns>
        internal bool IsEmptyArchive(ArchiveTreeRoot root)
        {
            return root == null || root.Children.IsNullOrEmpty();
        }

        /// <summary>
        /// Navigates to <see cref="DecompressionSummaryPage"/> with the currently
        /// set archive as the argument (as <see cref="ExtractableItem"/>).
        /// </summary>
        /// <param name="root">The root node of the archive.</param>
        internal void ExtractWholeArchiveButtonAction(ArchiveTreeRoot root)
        {
            IsNavigating = true;

            var item = new ExtractableItem(
                root.Archive.Name, root.Archive)
            {
                Password = root.Password
            };

            Navigation.Navigate(typeof(DecompressionSummaryPage), item);
        }

        /// <summary>
        /// Converts each model from the specified collection to <see cref="ExtractableItem"/> 
        /// and navigates to <see cref="DecompressionSummaryPage"/> afterwards.
        /// </summary>
        /// <param name="root">The root node of the archive.</param>
        /// <param name="node">The currently active node that holds equivalents of the models.</param>
        /// <param name="models">The models to be converted.</param>
        internal void ExtractSelectedEntriesButtonAction(ArchiveTreeRoot root,
            ArchiveTreeNode node, params ArchiveEntryModel[] models)
        {
            var entries = new List<ArchiveTreeFile>(models.Length);
            foreach (var model in models)
            {
                foreach (var child in node.Children)
                {
                    if (child.Name.Equals(model.DisplayName))
                    {
                        if (child is ArchiveTreeFile entry)
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
                var item = new ExtractableItem(
                    root.Archive.Name,
                    root.Archive,
                    entries.ConvertAll(e => e.ToArchiveEntry()))
                {
                    Password = root.Password
                };
                Navigation.Navigate(typeof(DecompressionSummaryPage), item);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _tokenSource.Dispose();
        }
    }
}
