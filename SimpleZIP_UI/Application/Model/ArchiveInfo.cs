using System.Collections.Generic;
using Windows.Storage;
using SimpleZIP_UI.Presentation;

namespace SimpleZIP_UI.Application.Model
{
    internal class ArchiveInfo
    {
        public ArchiveInfo(IReadOnlyList<StorageFile> files, CompressionMode mode)
        {
            SelectedFiles = files;
            Mode = mode;
        }

        internal IReadOnlyList<StorageFile> SelectedFiles { get; }

        internal string ArchiveName { get; set; }

        internal BaseControl.Algorithm Key { get; set; }

        internal CompressionMode Mode { get; }

        public enum CompressionMode
        {
            Compress, Decompress
        }
    }
}
