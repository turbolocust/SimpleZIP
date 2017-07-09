namespace SimpleZIP_UI.Application.Compression.Reader
{
    /// <summary>
    /// Represents a file entry within a node.
    /// </summary>
    public class FileEntry : IArchiveEntry
    {
        /// <summary>
        /// The key of the entry without the path.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Defaults to false as this entry is not a node.
        /// </summary>
        public bool IsNode { get; } = false;

        /// <summary>
        /// The uncompressed size of this file entry.
        /// </summary>
        public ulong Size { get; }

        /// <summary>
        /// The checksum of the file entry.
        /// </summary>
        internal long Crc { get; }

        internal FileEntry(string name, long crc, ulong size)
        {
            Name = name;
            Crc = crc;
            Size = size;
        }
    }
}
