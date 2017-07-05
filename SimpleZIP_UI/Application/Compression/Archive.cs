using System;
using System.Collections.Generic;

namespace SimpleZIP_UI.Application.Compression
{
    internal static class Archive
    {
        /// <summary>
        /// Enumeration to identify archive types.
        /// </summary>
        public enum ArchiveType
        {
            Zip, GZip, SevenZip, Tar, TarGz, TarBz2, TarLz
        }

        /// <summary>
        /// Maps file types for each archive. Consists only of file types with a single file name extension.
        /// </summary>
        internal static readonly Dictionary<string, ArchiveType> AlgorithmFileTypes;

        /// <summary>
        /// Maps file types for each archive. Consists only of file types with multiple file name extensions.
        /// </summary>
        internal static readonly Dictionary<string, ArchiveType> AlgorithmExtendedFileTypes;

        static Archive()
        {
            // populate dictionary that maps file types to algorithms
            AlgorithmFileTypes = new Dictionary<string, ArchiveType>(Enum.GetNames(typeof(ArchiveType)).Length * 2)
            {
                {".zip", ArchiveType.Zip},
                {".tar", ArchiveType.Tar},
                {".gzip", ArchiveType.GZip},
                {".gz", ArchiveType.GZip},
                {".tgz", ArchiveType.TarGz},
                {".tbz2", ArchiveType.TarBz2},
                {".tlz", ArchiveType.TarLz}
            };

            // populate dictionary that maps extended file types to algorithms
            AlgorithmExtendedFileTypes = new Dictionary<string, ArchiveType>
            {
                { ".tar.gz", ArchiveType.TarGz },
                { ".tar.bz2", ArchiveType.TarBz2 },
                { ".tar.lz", ArchiveType.TarLz }
            };
        }
    }
}
