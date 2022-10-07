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

using SimpleZIP_UI.Business.Compression.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Business.Compression.TreeBuilder
{
    /// <inheritdoc />
    /// <summary>
    /// Traverses the archive hierarchy and generates nodes, which represent directories,
    /// and file entries. A node can have other nodes and file entries as children.
    /// </summary>
    internal sealed class ArchiveTreeBuilder : IDisposable
    {
        /// <summary>
        /// Pre-defined name of the root node.
        /// </summary>
        private const string RootNodeName = "root";

        /// <summary>
        /// Dictionary that consists of existing nodes. Each node is unique and this 
        /// dictionary is used to access existing nodes as fast as possible => O(1).
        /// </summary>
        private readonly IDictionary<string, ArchiveTreeNode> _nodes;

        /// <summary>
        /// The reader used to read the archive entries.
        /// </summary>
        private IArchiveReader _reader;

        /// <summary>
        /// The token which can be used to interrupt the operation.
        /// </summary>
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// True if builder has been disposed and thus cannot be used anymore. 
        /// </summary>
        private bool _disposed;

        public ArchiveTreeBuilder(CancellationToken token)
        {
            _nodes = new Dictionary<string, ArchiveTreeNode>();
            _cancellationToken = token;
            _cancellationToken.Register(() => _disposed = true);
        }

        // ReSharper disable once SwitchStatementMissingSomeCases
        private static async Task<IArchiveReader> GetReaderInstance(StorageFile file, CancellationToken token)
        {
            IArchiveReader reader; // to be returned
            var type = await Archives.DetermineArchiveType(file).ConfigureAwait(false);

            switch (type)
            {
                case Archives.ArchiveType.Zip:
                    reader = new Reader.SZL.ArchiveReaderZip(file, token);
                    break;
                default: // using SharpCompress here
                    reader = new ArchiveReader(file, token);
                    break;
            }

            return reader;
        }

        /// <summary>
        /// Reads the entire archive and builds up a tree representing its hierarchy.
        /// </summary>
        /// <param name="archive">The archive to be read.</param>
        /// <param name="password">The password for the archive (if encrypted).</param>
        /// <returns>The root node of the tree, which represents the root directory.</returns>
        /// <exception cref="IOException">Thrown when an error while reading the archive occurred.</exception>
        /// <exception cref="OperationCanceledException">Thrown if operation has been cancelled.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if reader is closed.</exception>
        public async Task<ArchiveTreeRoot> Build(StorageFile archive, string password = null)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);

            _reader = await GetReaderInstance(archive, _cancellationToken).ConfigureAwait(false);
            await _reader.OpenArchiveAsync(password).ConfigureAwait(false);

            var task = Task.Run(() =>
            {
                var keyBuilder = new StringBuilder();
                var nameBuilder = new StringBuilder();
                var rootNode = new ArchiveTreeRoot(RootNodeName, archive, password);
                var archiveEntryTuple = new ArchiveEntryTuple();

                _nodes.Add(rootNode.Id, rootNode);

                foreach (var entry in _reader.ReadArchive())
                {
                    // directories are considered anyway
                    if (entry.IsDirectory || entry.Key == null) continue;

                    string key = Archives.NormalizeName(entry.Key);
                    UpdateEntryKeyPair(archiveEntryTuple, key);
                    ArchiveTreeNode parentNode = rootNode;

                    for (int i = 0; i <= archiveEntryTuple.SeparatorPos; ++i)
                    {
                        char c = key[i];
                        keyBuilder.Append(c);

                        if (c != Archives.NameSeparatorChar)
                        {
                            nameBuilder.Append(c);
                        }
                        else // next parent found
                        {
                            var node = GetNode(keyBuilder.ToString());
                            if (parentNode.Children.Add(node))
                            {
                                node.Name = nameBuilder.ToString();
                            }

                            parentNode = node;
                            nameBuilder.Clear();
                        }
                    }

                    if (!parentNode.Id.Equals(archiveEntryTuple.ParentKey, StringComparison.Ordinal))
                    {
                        throw new ReadingArchiveException($"Key {parentNode.Id} does not equal {archiveEntryTuple.ParentKey}");
                    }

                    var fileEntry = ArchiveTreeFile.CreateFileEntry(key,
                        archiveEntryTuple.EntryName, entry.Size, entry.Modified);

                    parentNode.Children.Add(fileEntry);
                    keyBuilder.Clear();
                }

                return rootNode; // return first element in tree, which is the root node

            }, _cancellationToken);

            return await task.ConfigureAwait(false);
        }

        private ArchiveTreeNode GetNode(string key)
        {
            if (!_nodes.TryGetValue(key, out var node))
            {
                node = new ArchiveTreeNode(key);
                _nodes.Add(key, node);
            }

            return node;
        }

        private static void UpdateEntryKeyPair(ArchiveEntryTuple tuple, string key)
        {
            string trimmedKey = key.TrimEnd(Archives.NameSeparatorChar);
            int lastSeparatorPos = trimmedKey.LastIndexOf(Archives.NameSeparatorChar);
            string entryName = trimmedKey.Substring(lastSeparatorPos + 1);
            string parentKey = lastSeparatorPos == -1
                ? RootNodeName // is root node, if no separator is present
                : trimmedKey.Substring(0, trimmedKey.Length - entryName.Length);
            tuple.SeparatorPos = lastSeparatorPos;
            tuple.EntryName = entryName;
            tuple.ParentKey = parentKey;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _reader?.Dispose();
                _disposed = true;
            }
        }

        private class ArchiveEntryTuple
        {
            internal int SeparatorPos;
            internal string EntryName;
            internal string ParentKey;
        }
    }
}