namespace SimpleZIP_UI.Application.Compression.Reader
{
    /// <summary>
    /// Represents an entry within a node.
    /// </summary>
    internal class Entry
    {
        /// <summary>
        /// The key of the entry without the path.
        /// </summary>
        internal string Name { get; }

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
