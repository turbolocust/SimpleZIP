// ==++==
// 
// Copyright (C) 2017 Matthias Fussenegger
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Compression.Streams;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Application.Compression.Algorithm
{
    /// <summary>
    /// Offers archiving operations using compressor streams only.
    /// </summary>
    public abstract class CompressorAlgorithm : AbstractAlgorithm
    {
        public override async Task<Stream> Decompress(StorageFile archive, StorageFolder location,
            ReaderOptions options = null)
        {
            if (archive == null || location == null) return Stream.Null;

            var compressorOptions = new CompressorOptions { IsCompression = false };
            Stream archiveStream = null, progressStream = null, compressorStream = Stream.Null;
            try
            {
                archiveStream = await archive.OpenStreamForReadAsync();
                progressStream = new ProgressObservableStream(this, archiveStream);
                compressorStream = GetCompressorStream(progressStream, compressorOptions);

                var outputFileName = archive.Name.Substring(0, archive.Name.Length - archive.FileType.Length);

                var file = await location.CreateFileAsync(outputFileName, CreationCollisionOption.GenerateUniqueName);
                if (file == null) return Stream.Null; // file was not created

                using (var outputStream = await file.OpenStreamForWriteAsync())
                {
                    var bytes = new byte[DefaultBufferSize];
                    int readBytes;

                    while ((readBytes = compressorStream.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        await outputStream.WriteAsync(bytes, 0, readBytes, Token);
                    }
                }
                await GZipOutputFileNameWorkaround(file, compressorStream);
            }
            finally
            {
                if (options != null && !options.LeaveStreamOpen)
                {
                    archiveStream?.Dispose();
                    progressStream?.Dispose();
                    compressorStream.Dispose();
                }
            }
            return compressorStream;
        }

        public sealed override async Task<Stream> Decompress(StorageFile archive, StorageFolder location,
            IReadOnlyList<FileEntry> entries, ReaderOptions options = null)
        {
            // ignore entries as they are not supported
            return await Decompress(archive, location, options);
        }

        public override async Task<Stream> Compress(IReadOnlyList<StorageFile> files, StorageFile archive,
            StorageFolder location, WriterOptions options = null)
        {
            if (files.IsNullOrEmpty() || archive == null || location == null) return Stream.Null;

            var file = files[0]; // since multiple files are not supported
            var compressorOptions = new CompressorOptions { FileName = file.Name, IsCompression = true };
            Stream archiveStream = null, progressStream = null, compressorStream = Stream.Null;
            try
            {
                archiveStream = await archive.OpenStreamForWriteAsync();
                progressStream = new ProgressObservableStream(this, archiveStream);
                compressorStream = GetCompressorStream(progressStream, compressorOptions);

                using (var inputStream = await file.OpenStreamForReadAsync())
                {
                    var bytes = new byte[DefaultBufferSize];
                    int readBytes;

                    while ((readBytes = inputStream.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        await compressorStream.WriteAsync(bytes, 0, readBytes, Token);
                    }
                }
            }
            finally
            {
                if (options != null && !options.LeaveStreamOpen)
                {
                    archiveStream?.Dispose();
                    progressStream?.Dispose();
                    compressorStream.Dispose();
                }
            }

            return compressorStream;
        }

        /// <summary>
        /// This method only exists to provide a workaround for renaming a file to its
        /// filename as it should be defined in the GZip header. The SharpCompress library 
        /// only sets the filename after the first call of <see cref="Stream.Read"/> and 
        /// not when the stream instance is constructed.
        /// </summary>
        /// <param name="file">The file to be renamed.</param>
        /// <param name="stream">The possible <see cref="GZipStream"/> which holds the filename.</param>
        /// <returns>True if stream is <see cref="GZipStream"/> an file was successfully renamed.</returns>
        private static async Task<bool> GZipOutputFileNameWorkaround(IStorageItem file, Stream stream)
        {
            if (stream is GZipStream gzipStream && !string.IsNullOrEmpty(gzipStream.FileName))
            {
                await file.RenameAsync(gzipStream.FileName);
                return true;
            }
            return false;
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
        protected struct CompressorOptions
        {
            /// <summary>
            /// File name to be set for compression stream.
            /// </summary>
            internal string FileName;

            /// <summary>
            /// Comment to be set for compression stream.
            /// </summary>
            //internal string Comment;

            /// <summary>
            /// True indicates a stream for compression, false for decompression.
            /// </summary>
            internal bool IsCompression;
        }
    }
}
