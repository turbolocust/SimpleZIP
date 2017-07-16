using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Application.Compression.Algorithm
{
    /// <summary>
    /// Offers archiving operations using compressor streams.
    /// </summary>
    public abstract class CompressorAlgorithm : ICompressionAlgorithm
    {
        /// <summary>
        /// Default buffer size for streams.
        /// </summary>
        protected const int DefaultBufferSize = 8192;

        /// <inheritdoc cref="ICompressionAlgorithm.Token"/>
        public CancellationToken Token { get; set; }

        public async Task<bool> Decompress(StorageFile archive, StorageFolder location,
            ReaderOptions options = null)
        {
            if (archive == null | location == null) return false;

            var compressorOptions = new CompressorOptions(false);

            using (var stream = await GetCompressorStream(archive, compressorOptions))
            {
                var outputFileName = archive.Name.Substring(0,
                    archive.Name.Length - archive.FileType.Length);

                var file = await location.CreateFileAsync(outputFileName,
                    CreationCollisionOption.GenerateUniqueName);
                if (file == null) return false; // file was not created

                using (var outputStream = await file.OpenStreamForWriteAsync())
                {
                    var bytes = new byte[DefaultBufferSize];
                    int readBytes;

                    while ((readBytes = stream.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        await outputStream.WriteAsync(bytes, 0, readBytes, Token);
                    }
                }
                await GZipOutputFileNameWorkaround(file, stream);
            }
            return true;
        }

        public async Task<bool> Decompress(StorageFile archive, StorageFolder location,
            IReadOnlyList<FileEntry> entries, ReaderOptions options = null)
        {
            // ignore entries as they are not supported
            return await Decompress(archive, location, options);
        }

        public async Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive,
            StorageFolder location, WriterOptions options = null)
        {
            if (files.IsNullOrEmpty() | archive == null | location == null) return false;

            var file = files[0];
            var compressorOptions = new CompressorOptions(true)
            {
                FileName = file.Name
            };

            using (var stream = await GetCompressorStream(archive, compressorOptions))
            {
                using (var inputStream = await file.OpenStreamForReadAsync())
                {
                    var bytes = new byte[DefaultBufferSize];
                    int readBytes;

                    while ((readBytes = await inputStream.ReadAsync(bytes, 0, bytes.Length, Token)) > 0)
                    {
                        await stream.WriteAsync(bytes, 0, readBytes, Token);
                    }
                }
            }
            return true;
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
        /// Gets the compressor stream from the derived class.
        /// </summary>
        /// <param name="archive">The archive whose stream is to be opened.</param>
        /// <param name="options">Options to be applied.</param>
        /// <returns>The compressor stream to be used.</returns>
        protected abstract Task<Stream> GetCompressorStream(StorageFile archive, CompressorOptions options);
    }
}
