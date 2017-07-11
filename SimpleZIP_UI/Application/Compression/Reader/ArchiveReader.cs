using System;
using SharpCompress.Common;
using SharpCompress.Readers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleZIP_UI.Exceptions;

namespace SimpleZIP_UI.Application.Compression.Reader
{
    /// <summary>
    /// Traverses the archive hierarchy and generates nodes, which represent folders,
    /// and file entries. A node can have other nodes and file entries as children.
    /// </summary>
    internal class ArchiveReader : IDisposable
    {
        /// <summary>
        /// The defined name of the root node.
        /// </summary>
        private const string RootNodeName = "root";

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
        /// <param name="archive">The archive to be read.</param>
        /// <param name="leaveOpen">True to leave the stream open. Defaults to true.</param>
        /// <returns>The stream used to read the archive.</returns>
        public async Task<Stream> OpenArchiveAsync(StorageFile archive, bool leaveOpen = true)
        {
            if (Closed) throw new ObjectDisposedException(GetType().FullName);

            var stream = await archive.OpenStreamForReadAsync();
            Reader = ReaderFactory.Open(stream, new ReaderOptions
            {
                LeaveStreamOpen = leaveOpen
            });
            return stream;
        }

        /// <summary>
        /// Reads the entire archive and builds up a tree representing its hierarchy.
        /// </summary>
        /// <returns>The root node of the tree.</returns>
        public Node Read()
        {
            if (Closed) throw new ObjectDisposedException(GetType().FullName);

            var rootNode = new Node(RootNodeName);
            var keyBuilder = new StringBuilder();
            var nameBuilder = new StringBuilder();
            _nodes.Add(rootNode.Id, rootNode);

            foreach (var entry in ReadArchive())
            {
                if (entry.IsDirectory) continue; // are considered anyway

                var key = entry.Key;
                var pair = GetEntryKeyPair(key);
                var parentNode = rootNode;

                for (var i = 0; i <= pair.SeparatorPos; ++i)
                {
                    var c = key[i];
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
                var fileEntry = new FileEntry(pair.EntryName, entry.Crc, entrySize);
                parentNode.Children.Add(fileEntry);
                keyBuilder.Clear();
            }

            return rootNode; // return first element in tree, which is the root node
        }

        /// <summary>
        /// Reads each entry of the archive.
        /// </summary>
        /// <returns>An enumerator which can be used to iterate over each entry.</returns>
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
            _nodes.TryGetValue(key, out Node node);
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
            internal string EntryName { get; set; }

            internal string ParentKey { get; set; }

            internal int SeparatorPos { get; set; }
        }
    }
}
