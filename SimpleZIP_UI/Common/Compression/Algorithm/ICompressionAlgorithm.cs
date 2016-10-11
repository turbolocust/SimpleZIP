using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Writers;

namespace SimpleZIP_UI.Common.Compression.Algorithm
{
    public interface ICompressionAlgorithm
    {
        /// <summary>
        /// Extracts an archive to a specified location.
        /// </summary>
        /// <param name="archive">The archive to be extracted.</param>
        /// <param name="location">The location where to extract the archive to.</param>
        /// <returns>True on success, false on error.</returns>
        /// <exception cref="IOException">Thrown on any error when reading from or writing to streams.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to file is not allowed.</exception>
        Task<bool> Extract(StorageFile archive, StorageFolder location);

        /// <summary>
        /// Compresses files to a specified location. If the writer options are omitted, the default
        /// fallback algorithm is used for the corresponding archive type of this intance.
        /// </summary>
        /// <param name="files">The files to be put into the archive.</param>
        /// <param name="archive">The archive to write compressed bytes to.</param>
        /// <param name="location">Where the archive will be created.</param>
        /// <param name="options">Options that the writer will use. May be omitted.</param>
        /// <returns>True on success, false on error.</returns>
        /// <exception cref="IOException">Thrown on any error when reading from or writing to streams.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to file is not allowed.</exception>
        /// <exception cref="NotSupportedException">Thrown when archive type does not have a writer.</exception>
        Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive, StorageFolder location, WriterOptions options = null);
    }
}
