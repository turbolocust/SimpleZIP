// ==++==
// 
// Copyright (C) 2019 Matthias Fussenegger
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// ==--==

using SimpleZIP_UI.Application.Compression.Algorithm.Event;
using SimpleZIP_UI.Application.Compression.Algorithm.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Compression.TreeBuilder;

namespace SimpleZIP_UI.Application.Compression.Algorithm
{
    /// <summary>
    /// Offers methods for compression and decompression of files/archives.
    /// </summary>
    public interface ICompressionAlgorithm
    {
        /// <summary>
        /// Event handler for total bytes processed.
        /// </summary>
        event EventHandler<TotalBytesProcessedEventArgs> TotalBytesProcessed;

        /// <summary>
        /// The token which can be used to interrupt the operation.
        /// </summary>
        CancellationToken Token { get; set; }

        /// <summary>
        /// Decompresses (extracts) all entries of the specified archive to the specified location.
        /// </summary>
        /// <param name="archive">The archive to be extracted.</param>
        /// <param name="location">The location where to extract the archive to.</param>
        /// <param name="options">Options for the reader. May be omitted.></param>
        /// <returns>Task which returns a stream that has been used to read the archive. The stream will not be 
        /// disposed if <see cref="IOptions.LeaveStreamOpen"/> in options is set to true.</returns>
        /// <exception cref="IOException">Thrown on any error when reading from or writing to streams.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to file is not permitted.</exception>
        /// <exception cref="ArchiveEncryptedException">Thrown if encrypted archive could not be handled.</exception>
        Task<Stream> Decompress(StorageFile archive, StorageFolder location, IDecompressionOptions options = null);

        /// <summary>
        /// Decompresses (extracts) entries from the specified archive to the specified location.
        /// </summary>
        /// <param name="archive">The archive which contains the entries.</param>
        /// <param name="location">The location where to extract the entries to.</param>
        /// <param name="entries">Entries of the archive to be extracted.</param>
        /// <param name="options">Options for the reader. May be omitted.</param>
        /// <returns>Task which returns a stream that has been used to read the archive. The stream will not be 
        /// disposed if <see cref="IOptions.LeaveStreamOpen"/> in options is set to true.</returns>
        /// <exception cref="IOException">Thrown on any error when reading from or writing to streams.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to file is not permitted.</exception>
        /// <exception cref="ArchiveEncryptedException">Thrown if encrypted archive could not be handled.</exception>
        Task<Stream> Decompress(StorageFile archive, StorageFolder location,
            IReadOnlyList<IArchiveEntry> entries, IDecompressionOptions options = null);

        /// <summary>
        /// Decompresses (extracts) entries from the specified archive to the specified location.
        /// </summary>
        /// <param name="archive">The archive which contains the entries.</param>
        /// <param name="location">The location where to extract the entries to.</param>
        /// <param name="entries">Entries of the archive to be extracted.</param>
        /// <param name="collectFileNames">True to collect file names, false otherwise.
        /// Names of extracted files will be saved to <see cref="ArchiveTreeFile.FileName"/>.</param>.
        /// <param name="options">Options for the reader. May be omitted.</param>
        /// <returns>Task which returns a stream that has been used to read the archive. The stream will not be 
        /// disposed if <see cref="IOptions.LeaveStreamOpen"/> in options is set to true.</returns>
        /// <exception cref="IOException">Thrown on any error when reading from or writing to streams.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to file is not permitted.</exception>
        /// <exception cref="ArchiveEncryptedException">Thrown if encrypted archive could not be handled.</exception>
        Task<Stream> Decompress(StorageFile archive, StorageFolder location,
            IReadOnlyList<IArchiveEntry> entries, bool collectFileNames, IDecompressionOptions options = null);

        /// <summary>
        /// Compresses files to the specified location. If the writer options are omitted, the default
        /// fallback algorithm is used for the corresponding archive type of this instance.
        /// </summary>
        /// <param name="files">The files to be put into the archive.</param>
        /// <param name="archive">The file to write compressed bytes to.</param>
        /// <param name="location">Where the archive is to be created.</param>
        /// <param name="options">Options for the writer. May be omitted.</param>
        /// <returns>Task which returns a stream that has been used to write the archive. The stream will not be 
        /// disposed if <see cref="IOptions.LeaveStreamOpen"/> in options is set to true.</returns>
        /// <exception cref="IOException">Thrown on any error when reading from or writing to streams.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when access to file is not allowed.</exception>
        /// <exception cref="NotSupportedException">Thrown when archive type does not have a writer.</exception>
        Task<Stream> Compress(IReadOnlyList<StorageFile> files, StorageFile archive,
            StorageFolder location, ICompressionOptions options = null);
    }
}
