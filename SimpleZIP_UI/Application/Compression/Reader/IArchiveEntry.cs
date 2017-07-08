namespace SimpleZIP_UI.Application.Compression.Reader
{
    internal interface IArchiveEntry
    {
        /// <summary>
        /// The key of the entry.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// True if this entry represents a directory, false otherwise.
        /// </summary>
        bool IsNode { get; }
    }
}
