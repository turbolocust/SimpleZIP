using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Application.Compression.Algorithm
{
    /// <summary>
    /// Offers archiving operations using SharpCompress' Reader and Writer API.
    /// </summary>
    public abstract class ArchivingAlgorithm : IArchivingAlgorithm
    {
        /// <summary>
        /// Default buffer size for streams.
        /// </summary>
        protected const int DefaultBufferSize = 8192;

        /// <summary>
        /// The concrete algorithm to be used.
        /// </summary>
        private readonly ArchiveType _type;

        /// <inheritdoc cref="IArchivingAlgorithm.Token"/>
        public CancellationToken Token { get; set; }

        protected ArchivingAlgorithm(ArchiveType type)
        {
            _type = type;
            Token = CancellationToken.None;
        }

        public async Task<bool> Extract(StorageFile archive, StorageFolder location, ReaderOptions options = null)
        {
            if (archive == null | location == null) return false;

            options = options ?? new ReaderOptions
            {
                LeaveStreamOpen = false
            };

            using (var reader = ReaderFactory.Open(await archive.OpenStreamForReadAsync(), options))
            {
                while (!Token.IsCancellationRequested && reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        await WriteEntry(reader, location);
                    }
                }
            }
            return true;
        }

        public async Task<bool> Extract(StorageFile archive, StorageFolder location, IReadOnlyList<FileEntry> entries, ReaderOptions options = null)
        {
            if (archive == null | entries.IsNullOrEmpty() | location == null) return false;

            options = options ?? new ReaderOptions
            {
                LeaveStreamOpen = false
            };

            using (var reader = ReaderFactory.Open(await archive.OpenStreamForReadAsync(), options))
            {
                while (!Token.IsCancellationRequested && reader.MoveToNextEntry())
                {
                    foreach (var entry in entries)
                    {
                        if (reader.Entry.Crc.Equals(entry.Crc))
                        {
                            await WriteEntry(reader, location);
                        }
                    }
                }
            }
            return true;
        }

        private async Task<IEntry> WriteEntry(IReader reader, StorageFolder location)
        {
            var entry = reader.Entry;
            var file = await FileUtils.CreateFileAsync(location, entry.Key);
            if (file == null) return null;

            using (var entryStream = reader.OpenEntryStream())
            {
                using (var outputStream = await file.OpenStreamForWriteAsync())
                {
                    var bytes = new byte[DefaultBufferSize];
                    int readBytes;
                    while ((readBytes = entryStream.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        await outputStream.WriteAsync(bytes, 0, readBytes, Token);
                    }
                }
            }
            return entry;
        }

        public async Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive,
            StorageFolder location, WriterOptions options = null)
        {
            if (files.IsNullOrEmpty() | archive == null | location == null) return false;

            options = options ?? GetWriterOptions(); // get options if null
            options.LeaveStreamOpen = false;

            using (var writer = WriterFactory.Open(await archive.OpenStreamForWriteAsync(), _type, options))
            {
                foreach (var file in files)
                {
                    if (Token.IsCancellationRequested) break;

                    using (var inputStream = await file.OpenStreamForReadAsync())
                    {
                        writer.Write(file.Name, inputStream);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the writer instance with the default fallback compression type.
        /// </summary>
        /// <returns>The writer options instance for the corresponding algorithm.</returns>
        protected abstract WriterOptions GetWriterOptions();
    }
}
