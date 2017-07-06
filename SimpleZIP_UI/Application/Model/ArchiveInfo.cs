using System.Collections.Generic;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression;

namespace SimpleZIP_UI.Application.Model
{
    internal class ArchiveInfo
    {
        public ArchiveInfo(OperationMode mode)
        {
            Mode = mode;
        }

        internal OperationMode Mode { get; }

        internal IReadOnlyList<StorageFile> SelectedFiles { get; set; }

        internal string ArchiveName { get; set; }

        internal StorageFolder OutputFolder { get; set; }

        internal Archives.ArchiveType ArchiveType { get; set; }
    }
}
