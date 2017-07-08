namespace SimpleZIP_UI.Application.Compression.Reader
{
    /// <summary>
    /// Represents an entry within a node.
    /// </summary>
    internal class Entry : IArchiveEntry
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
        /// The checksum of the archive entry.
        /// </summary>
        internal long Crc { get; }

        internal Entry(string name, long crc)
        {
            Name = name;
            Crc = crc;
        }
    }
}
