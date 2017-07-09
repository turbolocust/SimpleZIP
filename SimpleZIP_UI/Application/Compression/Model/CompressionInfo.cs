using System.Collections.Generic;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Model
{
    internal class CompressionInfo : OperationInfo
    {
        /// <summary>
        /// The type of the archive. See <see cref="Archives.ArchiveType"/>.
        /// </summary>
        internal Archives.ArchiveType ArchiveType { get; }

        /// <summary>
        /// List of selected files to be compressed.
        /// </summary>
        internal IReadOnlyList<StorageFile> SelectedFiles { get; set; }

        /// <summary>
        /// The name of the archive to be created.
        /// </summary>
        internal string ArchiveName { get; set; }

        public CompressionInfo(Archives.ArchiveType archiveType)
        {
            ArchiveType = archiveType;
        }
    }
}
