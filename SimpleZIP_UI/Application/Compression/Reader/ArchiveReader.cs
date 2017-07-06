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
        private readonly IDictionary<string, Node> _nodes;

        internal IReader Reader { get; private set; }

        internal bool AutoClose { get; set; } = false;

        internal bool Closed { get; private set; }

        public ArchiveReader()
        {
            _nodes = new Dictionary<string, Node>();
        }

        public async Task<Stream> OpenArchiveAsync(StorageFile archive)
        {
            if (Closed) throw new ObjectDisposedException(GetType().FullName);

            var stream = await archive.OpenStreamForReadAsync();
            Reader = ReaderFactory.Open(stream, new ReaderOptions
            {
                LeaveStreamOpen = true
            });
            return stream;
        }

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
                    var parentNode = rootNode;
                    var lastSeparatorIndex = key.LastIndexOf('/');

                    if (lastSeparatorIndex != -1) // not in root
                    {
                        var parentKey = GetParentKeyEntryNamePair(key, lastSeparatorIndex)[0];
                        parentNode = GetNode(parentKey);
                    }

                    parentNode.Children.Add(GetNode(key));
                }
                else
                {
                    var lastSeparatorIndex = key.LastIndexOf('/');
                    var pair = GetParentKeyEntryNamePair(key, lastSeparatorIndex);
                    GetNode(pair[0]).Entries.Add(new Entry(pair[1], entry.Crc));
                }
            }

            return rootNode; // return first element in tree, which is the root node
        }

        private IEnumerable<IEntry> ReadArchive()
        {
            while (!Closed && Reader.MoveToNextEntry())
            {
                yield return Reader.Entry;
            }
        }

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

        private static string[] GetParentKeyEntryNamePair(string entryKey, int separatorIndex)
        {
            var entryName = entryKey.Substring(separatorIndex + 1);
            var parentKey = entryKey.Substring(0, entryKey.Length - entryName.Length - 1);
            return new[] { parentKey, entryName };
        }

        public void Dispose()
        {
            Closed = true;
            if (AutoClose) Reader?.Dispose();
        }
    }
}
