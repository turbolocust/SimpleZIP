using System.Collections.Generic;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression.Reader;

namespace SimpleZIP_UI.Application.Compression.Model
{
    public class ExtractableItem
    {
        /// <summary>
        /// A friendly name of this item.
        /// </summary>
        internal string DisplayName { get; }

        /// <summary>
        /// The archive which can be extracted.
        /// </summary>
        internal StorageFile Archive { get; }

        /// <summary>
        /// Optional list of entries to be extracted. If this is
        /// not <code>null</code>, then only these entries will be extracted.
        /// </summary>
        internal IReadOnlyList<FileEntry> Entries { get; }

        internal ExtractableItem(string displayName, StorageFile archive,
            IReadOnlyList<FileEntry> entries = null)
        {
            DisplayName = displayName;
            Archive = archive;
            Entries = entries;
        }
    }
}
