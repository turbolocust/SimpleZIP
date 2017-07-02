using System.Collections.Generic;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression;
using SimpleZIP_UI.Presentation;

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

        internal BaseControl.Algorithm Algorithm { get; set; }
    }
}
