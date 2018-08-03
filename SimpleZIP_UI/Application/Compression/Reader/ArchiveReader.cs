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
using System;
using SharpCompress.Common;
using SharpCompress.Readers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Reader
{
    /// <inheritdoc />
    /// <summary>
    /// Traverses the archive hierarchy and generates nodes, which represent folders,
    /// and file entries. A node can have other nodes and file entries as children.
    /// </summary>
    internal class ArchiveReader : IDisposable
    {
        /// <summary>
        /// Pre-defined name of the root node.
        /// </summary>
        public const string RootNodeName = "root";

        /// <summary>
        /// To avoid too many calls on <see cref="Task.Delay(int)"/>.
        /// </summary>
        private const uint TaskDelayThreshold = 100;

        /// <summary>
        /// Dictionary that consists of existing nodes. Each node is unique and this 
        /// dictionary is used to access existing nodes as fast as possible => O(1).
        /// </summary>
        private readonly IDictionary<string, Node> _nodes;

        /// <summary>
        /// The reader used to read the archive entries. Will be instantiated when 
        /// opening the archive, see <see cref="OpenArchiveAsync"/>.
        /// </summary>
        private IReader Reader { get; set; }

        /// <summary>
        /// The token which can be used to interrupt the operation.
        /// </summary>
        private CancellationToken _cancellationToken;

        /// <summary>
        /// True if this reader is closed and thus cannot be used anymore. 
        /// Once this instance is disposed, this value will always be true.
        /// </summary>
        internal bool Closed { get; private set; }

        public ArchiveReader(CancellationToken token)
        {
            _nodes = new Dictionary<string, Node>();
            _cancellationToken = token;
            _cancellationToken.Register(() => Closed = true);
        }

        /// <summary>
        /// Opens the specified archive file asynchronously.
        /// </summary>
        /// <param name="archive">The archive to be opened.</param>
        /// <param name="password">The password for the archive (if encrypted).</param>
        /// <returns>The stream used to read the archive.</returns>
        private async Task<Stream> OpenArchiveAsync(IStorageFile archive, string password = null)
        {
            var options = new ReaderOptions
            {
                LeaveStreamOpen = false
            };
            if (password != null)
            {
                options.Password = password;
            }

            var stream = await archive.OpenStreamForReadAsync();
            Reader = ReaderFactory.Open(stream, options);
            return stream;
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
        public async Task<Node> Read(StorageFile archive, string password = null)
        {
            if (Closed) throw new ObjectDisposedException(GetType().FullName);

            await OpenArchiveAsync(archive, password);

            char separator = DetermineFileSeparator();
            var keyBuilder = new StringBuilder();
            var nameBuilder = new StringBuilder();
            var rootNode = new Node(RootNodeName);
            var pair = new EntryKeyPair();
            _nodes.Add(rootNode.Id, rootNode);

            uint threshold = 0; // to avoid too many calls on Task.Delay

            foreach (var entry in ReadArchive())
            {
                // directories are considered anyway
                if (entry.IsDirectory || entry.Key == null) continue;

                UpdateEntryKeyPair(ref pair, entry.Key, separator);
                var parentNode = rootNode;

                for (int i = 0; i <= pair.SeparatorPos; ++i)
                {
                    var c = entry.Key[i];
                    keyBuilder.Append(c);

                    if (c == separator) // next parent found
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

                var fileEntry = new FileEntry(entry.Key, pair.EntryName, (ulong)entry.Size);
                parentNode.Children.Add(fileEntry);
                keyBuilder.Clear();

                if (++threshold == TaskDelayThreshold)
                {
                    // to keep any waiting thread responsive
                    await Task.Delay(1, _cancellationToken);
                    threshold = 0;
                }
            }

            return rootNode; // return first element in tree, which is the root node
        }

        /// <summary>
        /// Reads each entry of the archive.
        /// </summary>
        /// <returns>An enumerable element of type <see cref="IEntry"/>.</returns>
        private IEnumerable<IEntry> ReadArchive()
        {
            while (!Closed && Reader.MoveToNextEntry())
            {
                _cancellationToken.ThrowIfCancellationRequested();
                yield return Reader.Entry;
            }
        }

        /// <summary>
        /// Returns the node with the specified key. If it does not exist, it 
        /// will first be created and added to <see cref="_nodes"/>.
        /// </summary>
        /// <param name="key">The key of the node as string.</param>
        /// <returns>The node with the specified key.</returns>
        private Node GetNode(string key)
        {
            _nodes.TryGetValue(key, out var node);
            if (node == null)
            {
                node = new Node(key);
                _nodes.Add(key, node);
            }
            return node;
        }

        /// <summary>
        /// Determines the file separator character used by the specific archive type.
        /// </summary>
        /// <returns>The correct file separator character for the archive type.</returns>
        private char DetermineFileSeparator()
        {
            return Reader.ArchiveType == ArchiveType.Rar ? '\\' : '/';
        }

        /// <summary>
        /// Updates an <see cref="EntryKeyPair"/> which holds the name of the entry 
        /// and the key of its parent node, also considering the root node.
        /// </summary>
        /// <param name="pair">Reference to the pair to be updated.</param>
        /// <param name="key">The full key of the entry.</param>
        /// <param name="separator">File separator character.</param>
        /// <returns>An <see cref="EntryKeyPair"/>.</returns>
        private static void UpdateEntryKeyPair(ref EntryKeyPair pair, string key, char separator)
        {
            var trimmedKey = key.TrimEnd(separator);
            var lastSeparatorPos = trimmedKey.LastIndexOf(separator);
            var entryName = trimmedKey.Substring(lastSeparatorPos + 1);
            var parentKey = lastSeparatorPos == -1 ? RootNodeName
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
                Closed = true;
                Reader?.Dispose();
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
