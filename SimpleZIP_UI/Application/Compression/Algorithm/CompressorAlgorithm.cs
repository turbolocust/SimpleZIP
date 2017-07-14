using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Application.Compression.Algorithm
{
    /// <summary>
    /// Offers archiving operations using compressor streams.
    /// </summary>
    public abstract class CompressorAlgorithm : IArchivingAlgorithm
    {
        /// <summary>
        /// Default buffer size for streams.
        /// </summary>
        protected const int DefaultBufferSize = 4096;

        /// <inheritdoc cref="IArchivingAlgorithm.Token"/>
        public CancellationToken Token { get; set; }

        public async Task<bool> Extract(StorageFile archive, StorageFolder location, ReaderOptions options = null)
        {
            if (archive == null | location == null) return false;

            using (var stream = await GetCompressorStream(archive, false))
            {
                // remove extension from output file name
                var outputFileName = archive.Name.Substring(0, archive.Name.Length - archive.FileType.Length);

                var file = await location.CreateFileAsync(outputFileName, CreationCollisionOption.GenerateUniqueName);
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
            }
            return true;
        }

        public Task<bool> Extract(StorageFile archive, StorageFolder location, IReadOnlyList<FileEntry> entries, ReaderOptions options = null)
        {
            throw new NotSupportedException();
        }

        public async Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive, StorageFolder location, WriterOptions options = null)
        {
            if (files.IsNullOrEmpty() | archive == null | location == null) return false;

            var file = files[0];

            using (var stream = await GetCompressorStream(archive, true))
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
        /// Gets the compressor stream from the derived class.
        /// </summary>
        /// <param name="archive">The archive whose stream is to be opened.</param>
        /// <param name="compress">True to return a stream for writing, false for reading.</param>
        /// <returns>The compressor stream to be used.</returns>
        protected abstract Task<Stream> GetCompressorStream(StorageFile archive, bool compress);
    }
}
