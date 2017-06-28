using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;

namespace SimpleZIP_UI.Common.Compression.Algorithm
{
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

        protected ArchivingAlgorithm(ArchiveType type)
        {
            _type = type;
        }

        public async Task<bool> Extract(StorageFile archive, StorageFolder location, ReaderOptions options = null)
        {
            if (archive == null || location == null) return false;

            options = options ?? new ReaderOptions()
            {
                LeaveStreamOpen = false
            };

            using (var inputStream = await archive.OpenReadAsync())
            using (var reader = ReaderFactory.Open(inputStream.AsStreamForRead(), options))
            {
                var size = inputStream.Size;
                while (reader.MoveToNextEntry()) // write each entry to file
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        var file = await location.CreateFileAsync(reader.Entry.Key,
                            CreationCollisionOption.GenerateUniqueName);

                        if (file == null) return false;

                        using (var fileStream = await file.OpenStreamForWriteAsync())
                        {
                            reader.WriteEntryTo(fileStream);
                        }
                    }

                    if (inputStream.Position == size) // to be extra sure that the end of stream has been reached
                    {
                        break;
                    }
                }
            }
            return true;
        }

        public async Task<bool> Compress(StorageFile file, StorageFile archive, StorageFolder location, WriterOptions options = null)
        {
            if (file == null || archive == null || location == null) return false;

            options = options ?? GetWriterOptions(); // set options if null

            using (var outputStream = await archive.OpenAsync(FileAccessMode.ReadWrite))
            using (var archiveStream = WriterFactory.Open(outputStream.AsStreamForWrite(), _type, options))
            {
                using (var fileStream = await file.OpenStreamForReadAsync())
                {
                    archiveStream.Write(file.Name, fileStream, DateTime.Now);
                }
            }
            return true;
        }

        public async Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive,
            StorageFolder location, WriterOptions options = null)
        {
            if (files == null || archive == null || location == null) return false;

            options = options ?? GetWriterOptions(); // set options if null

            using (var outputStream = await archive.OpenAsync(FileAccessMode.ReadWrite))
            using (var archiveStream = WriterFactory.Open(outputStream.AsStreamForWrite(), _type, options))
            {
                foreach (var file in files)
                {
                    using (var fileStream = await file.OpenStreamForReadAsync())
                    {
                        archiveStream.Write(file.Name, fileStream, DateTime.Now);
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
