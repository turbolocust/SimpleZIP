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
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Operation;
using SimpleZIP_UI.Application.Compression.Operation.Job;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Factory;
using SimpleZIP_UI.Presentation.View;
using SimpleZIP_UI.Presentation.View.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Presentation.Controller
{
    internal sealed class BrowseArchiveController : BaseController, ICancelRequest, IDisposable
    {
        /// <summary>
        /// Cache used for faster access to already read archives.
        /// </summary>
        private static readonly Dictionary<string, RootNode> NodesCache;

        /// <summary>
        /// The currently active root node.
        /// </summary>
        private static RootNode _curNode;

        /// <summary>
        /// Source for cancellation token.
        /// </summary>
        private readonly CancellationTokenSource _tokenSource;

        /// <summary>
        /// True if navigation is about to be executed, false otherwise.
        /// </summary>
        internal bool IsNavigating { get; private set; }

        /// <inheritdoc />
        public bool IsCancelRequest { get; private set; }

        static BrowseArchiveController()
        {
            NodesCache = new Dictionary<string, RootNode>();
        }

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
        internal async Task<Node> ReadArchive(StorageFile archive)
        {
            string id = archive.FolderRelativeId;
            // try to read root node from cache first
            if (NodesCache.TryGetValue(id, out var root))
            {
                return root.Node;
            }

            bool passwordSet = false;
            Node rootNode = null;
            try
            {
                try
                {
                    using (var reader = new ArchiveReader(_tokenSource.Token))
                    {
                        rootNode = await reader.Read(archive);
                    }
                }
                catch (SharpCompress.Common.CryptographicException)
                {
                    // archive is encrypted, ask for password and try again
                    string password = await PasswordRequest
                        .RequestPassword(archive.DisplayName);
                    passwordSet = password != null;
                    using (var reader = new ArchiveReader(_tokenSource.Token))
                    {
                        rootNode = await reader.Read(archive, password);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // ignore, because we're navigating back to
                // to previous archive or calling page
                // after cancelling the operation
            }
            catch (Exception ex)
            {
                string message = await I18N.ExceptionMessageHandler
                    .GetStringFor(ex, true, passwordSet, archive);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                DialogFactory.CreateErrorDialog(message).ShowAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Navigation.Navigate(typeof(MainPage));
            }
            finally
            {
                if (rootNode != null)
                {
                    root = new RootNode(rootNode, archive);
                    NodesCache.Add(id, root);
                    _curNode = root;
                }
                else
                {
                    _curNode.Password.Initialize();
                    _curNode = null;
                }
            }

            return rootNode;
        }

        /// <summary>
        /// Extracts a sub-archive within the current archive.
        /// </summary>
        /// <param name="node">The currently active node that
        /// holds the equivalent of the model.</param>
        /// <param name="model">The model to be converted.</param>
        /// <returns></returns>
        internal async Task<StorageFile> ExtractSubArchive(Node node, ArchiveEntryModel model)
        {
            StorageFile archive = null;
            // find entry in children of current node
            var entry = (from child in node.Children
                         where child.Name.Equals(model.DisplayName)
                         select child as FileEntry).FirstOrDefault();

            if (entry != null)
            {
                var folder = await FileUtils.GetTempFolderAsync();
                if (!string.IsNullOrEmpty(entry.FileName))
                {
                    // file has already been extracted
                    archive = await folder.GetFileAsync(entry.FileName);
                    if (archive != null) // exists
                    {
                        return archive;
                    }
                }
                // doesn't exist, hence extract and read again
                var item = new ExtractableItem(
                    _curNode.ArchiveFile.Name,
                    _curNode.ArchiveFile, new[] { entry });
                var size = await FileUtils.GetFileSizeAsync(item.Archive);
                // create operation and job for execution
                var operationInfo = new DecompressionInfo(item, size)
                {
                    OutputFolder = folder,
                    IsCollectFileNames = true
                };
                var operation = Operations.ForDecompression();
                var job = new DecompressionJob(operation, operationInfo)
                {
                    PasswordRequest = PasswordRequest
                };
                var result = await job.Run(this); // extraction happens here
                if (result.StatusCode == Result.Status.Success)
                {
                    archive = await folder.GetFileAsync(entry.FileName);
                }
            }

            return archive;
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
        /// <returns>True if archive only consists of one file entry.</returns>
        internal bool IsSingleFileEntryArchive()
        {
            return _curNode.Node.Children.Count == 1
                && !_curNode.Node.Children.First().IsBrowsable;
        }

        /// <summary>
        /// Checks whether the currently opened archive contains
        /// any elements (including files and folders) or not.
        /// </summary>
        /// <returns>True if archive is empty, false otherwise.</returns>
        internal bool IsEmptyArchive()
        {
            return _curNode.Node == null ||
                   _curNode.Node.Children.IsNullOrEmpty();
        }

        /// <summary>
        /// Navigates to <see cref="DecompressionSummaryPage"/> with the currently
        /// set archive file as the argument.
        /// </summary>
        internal void ExtractWholeArchiveButtonAction()
        {
            IsNavigating = true;
            var args = new NavigationArgs(new[] { _curNode.ArchiveFile });
            Navigation.Navigate(typeof(DecompressionSummaryPage), args);
        }

        /// <summary>
        /// Converts each model from the specified collection to <see cref="ExtractableItem"/> 
        /// and navigates to <see cref="DecompressionSummaryPage"/> afterwards.
        /// </summary>
        /// <param name="node">The currently active node that holds equivalents of the models.</param>
        /// <param name="models">The models to be converted.</param>
        internal void ExtractSelectedEntriesButtonAction(Node node, params ArchiveEntryModel[] models)
        {
            var entries = new List<FileEntry>(models.Length);
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
                var item = new ExtractableItem(
                    _curNode.ArchiveFile.Name,
                    _curNode.ArchiveFile, entries);
                Navigation.Navigate(typeof(DecompressionSummaryPage), item);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _tokenSource.Dispose();
        }

        private sealed class RootNode
        {
            internal Node Node { get; }

            internal StorageFile ArchiveFile { get; }

            internal char[] Password { get; }

            internal RootNode(Node node, StorageFile archiveFile, char[] password = null)
            {
                Node = node;
                ArchiveFile = archiveFile;
                Password = password ?? new char[0];
            }
        }
    }
}
