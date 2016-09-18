using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;

namespace SimpleZIP_UI.Common.Compression.Algorithm
{
    public interface ICompressionAlgorithm
    {
        /// <summary>
        /// Extracts an archive to a specified location.
        /// </summary>
        /// <param name="archive">The archive to be extracted.</param>
        /// <exception cref="IOException">Thrown on any error when reading from or writing to streams.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to file is not allowed.</exception>
        void Extract(StorageFile archive);

        /// <summary>
        /// Compresses an archive to a specified location.
        /// </summary>
        /// <param name="files">The files to be put into the archive.</param>
        /// <param name="archiveName">The name of the archive to be compressed.</param>
        /// <param name="location">Where the archive will be created.</param>
        /// <exception cref="IOException">Thrown on any error when reading from or writing to streams.</exception>
        ///<exception cref="UnauthorizedAccessException">Thrown when access to file is not allowed.</exception>
        void Compress(IReadOnlyList<StorageFile> files, string archiveName, StorageFolder location);
    }
}
