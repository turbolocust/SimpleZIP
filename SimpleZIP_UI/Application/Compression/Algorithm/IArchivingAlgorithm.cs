using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SimpleZIP_UI.Application.Compression.Reader;

namespace SimpleZIP_UI.Application.Compression.Algorithm
{
    /// <summary>
    /// Any implementing class offers methods for archiving operations, like compressing or decompressing files.
    /// </summary>
    public interface IArchivingAlgorithm
    {
        /// <summary>
        /// The token which can be used to interrupt the operation.
        /// </summary>
        CancellationToken Token { get; set; }

        /// <summary>
        /// Extracts an archive to the specified location.
        /// </summary>
        /// <param name="archive">The archive to be extracted.</param>
        /// <param name="location">The location where to extract the archive to.</param>
        /// <param name="options">Options for the reader. May be omitted.></param>
        /// <returns>True on success, false on fail.</returns>
        /// <exception cref="IOException">Thrown on any error when reading from or writing to streams.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to file is not permitted.</exception>
        Task<bool> Extract(StorageFile archive, StorageFolder location, ReaderOptions options = null);

        /// <summary>
        /// Extracts entries from the specified archive to the specified location.
        /// </summary>
        /// <param name="archive">The archive which contains the entry.</param>
        /// <param name="location">The location where to extract the entries to.</param>
        /// <param name="entries">Entries of the archive to be extracted.</param>
        /// <param name="options">Options for the reader. May be omitted.</param>
        /// <exception cref="IOException">Thrown on any error when reading from or writing to streams.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to file is not permitted.</exception>
        /// <returns>True on success, false on fail.</returns>
        Task<bool> Extract(StorageFile archive, StorageFolder location, IReadOnlyList<FileEntry> entries, ReaderOptions options = null);

        /// <summary>
        /// Compresses files to the specified location. If the writer options are omitted, the default
        /// fallback algorithm is used for the corresponding archive type of this instance.
        /// </summary>
        /// <param name="files">The files to be put into the archive.</param>
        /// <param name="archive">The file to write compressed bytes to.</param>
        /// <param name="location">Where the archive will be created.</param>
        /// <param name="options">Options for the writer. May be omitted.</param>
        /// <returns>True on success, false on fail.</returns>
        /// <exception cref="IOException">Thrown on any error when reading from or writing to streams.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to file is not allowed.</exception>
        /// <exception cref="NotSupportedException">Thrown when archive type does not have a writer.</exception>
        Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive, StorageFolder location, WriterOptions options = null);
    }
}
