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

using SharpCompress.Compressors.Deflate;
using SimpleZIP_UI.Application.Compression.Algorithm.Options;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Streams;
using SimpleZIP_UI.Application.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Algorithm
{
    /// <inheritdoc />
    /// <summary>
    /// Offers archiving operations using compressor streams only.
    /// </summary>
    public abstract class CompressorAlgorithm : AbstractAlgorithm
    {
        private async Task<Stream> DecompressArchive(IStorageFile archive, IStorageFolder location,
            IReadOnlyCollection<IArchiveEntry> entries, bool collectFileNames, IDecompressionOptions options = null)
        {
            if (archive == null || location == null) return Stream.Null;

            var archiveStream = Stream.Null;
            var progressStream = Stream.Null;
            var compressorStream = Stream.Null;

            var compressorOptions = new CompressorOptions { IsCompression = false };
            options = options ?? new DecompressionOptions(false, GetDefaultEncoding());

            try
            {
                archiveStream = await archive.OpenStreamForReadAsync().ConfigureAwait(false);
                progressStream = new ProgressObservableStream(this, archiveStream);
                compressorStream = GetCompressorStream(progressStream, compressorOptions);

                string outputFileName = archive.Name.Substring(0, archive.Name.Length - archive.FileType.Length);

                var file = await location.CreateFileAsync(outputFileName, CreationCollisionOption.GenerateUniqueName);
                if (file == null) return Stream.Null; // file was not created

                using (var outputStream = await file.OpenStreamForWriteAsync().ConfigureAwait(false))
                {
                    var bytes = new byte[DefaultBufferSize];
                    int readBytes;

                    while ((readBytes = compressorStream.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        await outputStream.WriteAsync(bytes, 0, readBytes, Token).ConfigureAwait(false);
                    }

                    await outputStream.FlushAsync().ConfigureAwait(false);
                }

                await GZipOutputFileNameWorkaround(file, compressorStream).ConfigureAwait(false);

                // update file name of corresponding entry
                if (collectFileNames && !entries.IsNullOrEmpty())
                {
                    var entry = entries.ElementAt(0);
                    entry.FileName = file.Name;
                }
            }
            finally
            {
                if (!options.LeaveStreamOpen)
                {
                    compressorStream.Dispose();
                    progressStream.Dispose();
                    archiveStream.Dispose();
                }
            }

            return compressorStream;
        }

        /// <inheritdoc />
        public override async Task<Stream> Decompress(StorageFile archive,
            StorageFolder location, IDecompressionOptions options = null)
        {
            return await DecompressArchive(archive, location,
                null, false, options).ConfigureAwait(false);
        }

        /// <inheritdoc />
        /// <summary>Entries are not supported in non-archive formats,
        /// but will be used if file names are to be collected.</summary>
        public override async Task<Stream> Decompress(StorageFile archive, StorageFolder location,
            IReadOnlyList<IArchiveEntry> entries, bool collectFileNames, IDecompressionOptions options = null)
        {
            return await DecompressArchive(archive, location,
                entries, collectFileNames, options).ConfigureAwait(false);
        }

        /// <inheritdoc />
        /// <summary>Entries are not supported in non-archive formats,
        /// and hence will be ignored.</summary>
        public sealed override async Task<Stream> Decompress(StorageFile archive, StorageFolder location,
            IReadOnlyList<IArchiveEntry> entries, IDecompressionOptions options = null)
        {
            return await DecompressArchive(archive, location,
                entries, false, options).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override async Task<Stream> Compress(IReadOnlyList<StorageFile> files,
            StorageFile archive, StorageFolder location, ICompressionOptions options = null)
        {
            if (files.IsNullOrEmpty() || archive == null || location == null) return Stream.Null;

            var file = files[0]; // since multiple files are not supported

            var archiveStream = Stream.Null;
            var progressStream = Stream.Null;
            var compressorStream = Stream.Null;

            var compressorOptions = new CompressorOptions { FileName = file.Name, IsCompression = true };
            options = options ?? new CompressionOptions(false, GetDefaultEncoding());

            try
            {
                archiveStream = await archive.OpenStreamForWriteAsync().ConfigureAwait(false);
                progressStream = new ProgressObservableStream(this, archiveStream);
                compressorStream = GetCompressorStream(progressStream, compressorOptions);

                using (var inputStream = await file.OpenStreamForReadAsync().ConfigureAwait(false))
                {
                    var bytes = new byte[DefaultBufferSize];
                    int readBytes;

                    while ((readBytes = inputStream.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        await compressorStream.WriteAsync(bytes, 0, readBytes, Token).ConfigureAwait(false);
                    }

                    await compressorStream.FlushAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                if (!options.LeaveStreamOpen)
                {
                    compressorStream.Dispose();
                    progressStream.Dispose();
                    archiveStream.Dispose();
                }
            }

            return compressorStream;
        }

        /// <summary>
        /// This method only exists to provide a workaround for renaming a file to its
        /// filename as it should be defined in the GZip header.
        /// </summary>
        /// <remarks>
        /// The SharpCompress library sets the filename after the first call of
        /// <see cref="Stream.Read"/> and not when the stream instance is constructed.
        /// </remarks>
        /// <param name="file">The file to be renamed.</param>
        /// <param name="stream">The possible <see cref="GZipStream"/> which holds the filename.</param>
        /// <returns>A task that can be awaited.</returns>
        private static async Task GZipOutputFileNameWorkaround(IStorageItem file, Stream stream)
        {
            if (stream is GZipStream gzipStream
                && !string.IsNullOrEmpty(gzipStream.FileName))
            {
                await file.RenameAsync(gzipStream.FileName,
                    NameCollisionOption.GenerateUniqueName);
            }
        }

        /// <summary>
        /// Gets the concrete compressor stream from the derived class.
        /// </summary>
        /// <param name="stream">The stream to be decorated.</param>
        /// <param name="options">Options to be applied.</param>
        /// <returns>Compressor stream which can be used for compression.</returns>
        protected abstract Stream GetCompressorStream(Stream stream, CompressorOptions options);

        /// <summary>
        /// Options for compressor streams.
        /// </summary>
        protected class CompressorOptions
        {
            /// <summary>
            /// File name to be set for compression stream.
            /// </summary>
            internal string FileName;

            ///// <summary>
            ///// Comment to be set for compression stream.
            ///// </summary>
            //internal string Comment;

            /// <summary>
            /// True indicates a stream for compression, false for decompression.
            /// </summary>
            internal bool IsCompression;
        }
    }
}
