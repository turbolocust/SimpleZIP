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
using SharpCompress.Common;
using SharpCompress.Readers;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        /// The defined name of the root node.
        /// </summary>
        public const string RootNodeName = "root";

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
        /// True if this reader is closed and therefore, cannot be used anymore. 
        /// Once this object gets disposed this variable will always evaluate to true.
        /// </summary>
        internal bool Closed { get; private set; }

        public ArchiveReader()
        {
            _nodes = new Dictionary<string, Node>();
        }

        /// <summary>
        /// Opens the specified archive file asynchronously.
        /// </summary>
        /// <param name="archive">The archive to be opened.</param>
        /// <param name="options">Options for the <see cref="IReader"/>.</param>
        /// <returns>The stream used to read the archive.</returns>
        private async Task<Stream> OpenArchiveAsync(IStorageFile archive, ReaderOptions options = null)
        {
            options = options ?? new ReaderOptions
            {
                LeaveStreamOpen = false
            };

            var stream = await archive.OpenStreamForReadAsync();
            Reader = ReaderFactory.Open(stream, options);
            return stream;
        }

        /// <summary>
        /// Reads the entire archive and builds up a tree representing its hierarchy.
        /// </summary>
        /// <param name="archive">The archive to be read.</param>
        /// <returns>The root node of the tree, which represents the root directory.</returns>
        /// <exception cref="IOException">Thrown when an error while reading the archive occurred.</exception>
        public async Task<Node> Read(StorageFile archive)
        {
            if (Closed) throw new ObjectDisposedException(GetType().FullName);

            await OpenArchiveAsync(archive);

            var rootNode = new Node(RootNodeName);
            var keyBuilder = new StringBuilder();
            var nameBuilder = new StringBuilder();
            _nodes.Add(rootNode.Id, rootNode);

            foreach (var entry in ReadArchive())
            {
                // directories are considered anyway
                if (entry.IsDirectory || entry.Key == null) continue;

                var pair = GetEntryKeyPair(entry.Key);
                var parentNode = rootNode;

                for (var i = 0; i <= pair.SeparatorPos; ++i)
                {
                    var c = entry.Key[i];
                    keyBuilder.Append(c);

                    if (c == '/') // next parent found
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
                {
                    throw new ReadingArchiveException("Error reading archive.");
                }

                var entrySize = (ulong)entry.Size;
                var fileEntry = new FileEntry(entry.Key, pair.EntryName, entrySize);
                parentNode.Children.Add(fileEntry);
                keyBuilder.Clear();
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
        /// Returns an <see cref="EntryKeyPair"/> which holds the name of the entry 
        /// and the key of its parent node, also considering the root node.
        /// </summary>
        /// <param name="key">The full key of the entry.</param>
        /// <returns>An <see cref="EntryKeyPair"/>.</returns>
        private static EntryKeyPair GetEntryKeyPair(string key)
        {
            var trimmedKey = key.TrimEnd('/');
            var lastSeparatorIndex = trimmedKey.LastIndexOf('/');
            var entryName = trimmedKey.Substring(lastSeparatorIndex + 1);
            var parentKey = lastSeparatorIndex == -1 // is in root directory
                ? RootNodeName
                : trimmedKey.Substring(0, trimmedKey.Length - entryName.Length);
            return new EntryKeyPair
            {
                EntryName = entryName,
                ParentKey = parentKey,
                SeparatorPos = lastSeparatorIndex
            };
        }

        public void Dispose()
        {
            Closed = true;
            Reader?.Dispose();
        }

        private struct EntryKeyPair
        {
            internal string EntryName;

            internal string ParentKey;

            internal int SeparatorPos;
        }
    }
}
