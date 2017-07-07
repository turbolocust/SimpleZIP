using System;
using SharpCompress.Common;
using SharpCompress.Readers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Reader
{
    internal class ArchiveReader : IDisposable
    {
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

            var rootNode = new Node("root");
            _nodes.Add(rootNode.Id, rootNode);

            foreach (var entry in ReadArchive())
            {
                var key = entry.Key;
                if (entry.IsDirectory)
                {
                    var pair = GetEntryKeyPair(key);
                    var parentKey = pair.ParentKey;
                    var parentNode = string.IsNullOrEmpty(parentKey)
                        ? rootNode : GetNode(parentKey);
                    var childNode = GetNode(key);
                    childNode.Name = pair.EntryName;
                    parentNode.Children.Add(childNode);
                }
                else
                {
                    var pair = GetEntryKeyPair(key);
                    GetNode(pair.ParentKey).Entries.Add(new Entry(pair.EntryName, entry.Crc));
                }
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
        /// <param name="key"></param>
        /// <returns></returns>
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
        /// and the key of its parent node. The parent key might be <see cref="string.Empty"/> 
        /// if the entry is located in the root directory.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static EntryKeyPair GetEntryKeyPair(string key)
        {
            var trimmedKey = key.TrimEnd('/');
            var lastSeparatorIndex = trimmedKey.LastIndexOf('/');
            var entryName = trimmedKey.Substring(lastSeparatorIndex + 1);
            var parentKey = lastSeparatorIndex == -1 // is in root directory
                ? string.Empty
                : trimmedKey.Substring(0, trimmedKey.Length - entryName.Length);
            return new EntryKeyPair
            {
                EntryName = entryName,
                ParentKey = parentKey
            };
        }

        public void Dispose()
        {
            Closed = true;
            Reader?.Dispose();
        }

        private class EntryKeyPair
        {
            internal string EntryName { get; set; }

            internal string ParentKey { get; set; }
        }
    }
}
