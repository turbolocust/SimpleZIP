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

using SimpleZIP_UI.Application.Compression.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Tree
{
    /// <inheritdoc />
    /// <summary>
    /// Traverses the archive hierarchy and generates nodes, which represent folders,
    /// and file entries. A node can have other nodes and file entries as children.
    /// </summary>
    internal class ArchiveTreeBuilder : IDisposable
    {
        /// <summary>
        /// Pre-defined name of the root node.
        /// </summary>
        public const string RootNodeName = "root";

        /// <summary>
        /// To avoid too many calls of <see cref="Task.Delay(int)"/>.
        /// </summary>
        private const uint DefaultTaskDelayRate = 100;

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
        private CancellationToken _cancellationToken;

        /// <summary>
        /// True if builder has been interrupted and thus cannot be used anymore. 
        /// Once this instance is disposed, this value will always be true.
        /// </summary>
        internal bool Interrupt { get; private set; }

        public ArchiveTreeBuilder(CancellationToken token)
        {
            _nodes = new Dictionary<string, ArchiveTreeNode>();
            _cancellationToken = token;
            _cancellationToken.Register(() => Interrupt = true);
        }

        // ReSharper disable once SwitchStatementMissingSomeCases
        private static async Task<IArchiveReader> GetReaderInstance(
            StorageFile file, CancellationToken token)
        {
            IArchiveReader reader; // to be returned
            var type = await Archives.DetermineArchiveType(file);

            switch (type)
            {
                case Archives.ArchiveType.Zip:
                    // make use of SharpZipLib for ZIP files,
                    // because it's more reliable than SharpCompress
                    reader = new Reader.SZL.ArchiveReader(file, token);
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
            if (Interrupt) throw new ObjectDisposedException(GetType().FullName);

            _reader = await GetReaderInstance(archive, _cancellationToken);
            await _reader.OpenArchiveAsync(password);

            var keyBuilder = new StringBuilder();
            var nameBuilder = new StringBuilder();
            var rootNode = new ArchiveTreeRoot(RootNodeName, archive, password);
            var pair = new EntryKeyPair();
            _nodes.Add(rootNode.Id, rootNode);

            uint delayRate = 0; // to avoid too many calls of Task.Delay

            foreach (var entry in _reader.ReadArchive())
            {
                // directories are considered anyway
                if (entry.IsDirectory || entry.Key == null) continue;

                string key = Archives.NormalizeName(entry.Key);
                UpdateEntryKeyPair(ref pair, key);
                ArchiveTreeNode parentNode = rootNode;

                for (int i = 0; i <= pair.SeparatorPos; ++i)
                {
                    var c = key[i];
                    keyBuilder.Append(c);

                    if (c == Archives.NameSeparatorChar) // next parent found
                    {
                        var node = GetNode(keyBuilder.ToString());
                        if (parentNode.Children.Add(node))
                        {
                            node.Name = nameBuilder.ToString();
                        }
                        parentNode = node;
                        nameBuilder.Clear();
                    }
                    else
                    {
                        nameBuilder.Append(c);
                    }
                }

                if (!parentNode.Id.Equals(pair.ParentKey))
                    throw new ReadingArchiveException("Error reading archive.");

                var fileEntry = ArchiveTreeFile.CreateFileEntry(
                    key, pair.EntryName, entry.Size);
                parentNode.Children.Add(fileEntry);
                keyBuilder.Clear();

                if (++delayRate == DefaultTaskDelayRate)
                {
                    // to keep any waiting thread responsive
                    await Task.Delay(1, _cancellationToken);
                    delayRate = 0;
                }
            }

            return rootNode; // return first element in tree, which is the root node
        }

        /// <summary>
        /// Returns the node with the specified key. If it does not exist, it 
        /// will first be created and added to <see cref="_nodes"/>.
        /// </summary>
        /// <param name="key">The key of the node as string.</param>
        /// <returns>The node with the specified key.</returns>
        private ArchiveTreeNode GetNode(string key)
        {
            _nodes.TryGetValue(key, out var node);
            if (node == null)
            {
                node = new ArchiveTreeNode(key);
                _nodes.Add(key, node);
            }
            return node;
        }

        /// <summary>
        /// Updates an <see cref="EntryKeyPair"/> which holds the name of the entry 
        /// and the key of its parent node, also considering the root node.
        /// </summary>
        /// <param name="pair">Reference to the pair to be updated.</param>
        /// <param name="key">The full key of the entry.</param>
        /// <returns>An <see cref="EntryKeyPair"/>.</returns>
        private static void UpdateEntryKeyPair(ref EntryKeyPair pair, string key)
        {
            string trimmedKey = key.TrimEnd(Archives.NameSeparatorChar);
            int lastSeparatorPos = trimmedKey.LastIndexOf(Archives.NameSeparatorChar);
            string entryName = trimmedKey.Substring(lastSeparatorPos + 1);
            string parentKey = lastSeparatorPos == -1 ? RootNodeName
                : trimmedKey.Substring(0, trimmedKey.Length - entryName.Length);
            pair.SeparatorPos = lastSeparatorPos;
            pair.EntryName = entryName;
            pair.ParentKey = parentKey;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Interrupt = true;
                _reader.Dispose();
            }
        }

        private struct EntryKeyPair
        {
            internal int SeparatorPos;
            internal string EntryName;
            internal string ParentKey;
        }
    }
}
