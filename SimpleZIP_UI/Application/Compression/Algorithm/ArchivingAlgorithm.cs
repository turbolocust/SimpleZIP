using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Application.Compression.Algorithm
{
    public abstract class ArchivingAlgorithm : IArchivingAlgorithm
    {
        /// <summary>
        /// Default buffer size for streams.
        /// </summary>
        protected const int DefaultBufferSize = 8192;

        /// <summary>
        /// The token which can be used to interrupt the operation.
        /// </summary>
        protected CancellationToken Token;

        /// <summary>
        /// The concrete algorithm to be used.
        /// </summary>
        private readonly ArchiveType _type;

        protected ArchivingAlgorithm(ArchiveType type)
        {
            _type = type;
            Token = CancellationToken.None;
        }

        public async Task<bool> Extract(StorageFile archive, StorageFolder location, ReaderOptions options = null)
        {
            if (archive == null || location == null) return false;

            options = options ?? new ReaderOptions
            {
                LeaveStreamOpen = false
            };

            using (var reader = ReaderFactory.Open(await archive.OpenStreamForReadAsync(), options))
            {
                var bytes = new byte[DefaultBufferSize];

                while (!IsInterrupted() && reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        var file = await FileUtils.CreateFileAsync(location, reader.Entry.Key);
                        if (file == null) return false;

                        using (var entryStream = reader.OpenEntryStream())
                        {
                            using (var outputStream = await file.OpenStreamForWriteAsync())
                            {
                                int readBytes;
                                while (!IsInterrupted() && (readBytes = entryStream.Read(bytes, 0, bytes.Length)) > 0)
                                {
                                    await outputStream.WriteAsync(bytes, 0, readBytes, Token);
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        public async Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive,
            StorageFolder location, WriterOptions options = null)
        {
            if (files == null || archive == null || location == null) return false;

            options = options ?? GetWriterOptions(); // get options if null
            options.LeaveStreamOpen = false;

            using (var writer = WriterFactory.Open(await archive.OpenStreamForWriteAsync(), _type, options))
            {
                foreach (var file in files)
                {
                    if (IsInterrupted()) break;

                    using (var inputStream = await file.OpenStreamForReadAsync())
                    {
                        writer.Write(file.Name, inputStream);
                    }
                }
            }
            return true;
        }

        public void SetCancellationToken(CancellationToken token)
        {
            Token = token;
        }

        /// <summary>
        /// Checks if this operation should be canceled.
        /// </summary>
        /// <returns>True if cancellation request has been made, false otherwise.</returns>
        protected bool IsInterrupted()
        {
            return Token.IsCancellationRequested;
        }

        /// <summary>
        /// Returns the writer instance with the default fallback compression type.
        /// </summary>
        /// <returns>The writer options instance for the corresponding algorithm.</returns>
        protected abstract WriterOptions GetWriterOptions();
    }
}
