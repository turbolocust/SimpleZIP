using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;
using SimpleZIP_UI.Exceptions;

namespace SimpleZIP_UI.Common.Compression.Algorithm
{
    public abstract class AbstractAlgorithm : IArchivingAlgorithm
    {
        protected readonly ArchiveType Type;

        protected AbstractAlgorithm(ArchiveType type)
        {
            Type = type;
        }

        public async Task<bool> Extract(StorageFile archive, StorageFolder location, ReaderOptions options = null)
        {
            if (archive == null || location == null) return false;

            options = options ?? new ReaderOptions()
            {
                LeaveStreamOpen = true
            };

            using (var inputStream = await archive.OpenSequentialReadAsync())
            using (var reader = ReaderFactory.Open(inputStream.AsStreamForRead(), options))
            {
                while (reader.MoveToNextEntry()) // write each entry to file
                {
                    var file = await location.CreateFileAsync(reader.Entry.Key,
                                CreationCollisionOption.GenerateUniqueName);

                    if (file == null) return false;

                    using (var fileStream = await file.OpenStreamForWriteAsync())
                    {
                        reader.WriteEntryTo(fileStream);
                    }
                }
            }
            return true;
        }

        public async Task<bool> Compress(StorageFile file, StorageFile archive, StorageFolder location, WriterOptions options = null)
        {
            if (file == null || archive == null || location == null) return false;

            options = options ?? SetWriterOptions(); // set options if null

            using (var outputStream = await archive.OpenAsync(FileAccessMode.ReadWrite))
            using (var archiveStream = WriterFactory.Open(outputStream.AsStreamForWrite(), Type, options))
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

            options = options ?? SetWriterOptions(); // set options if null

            using (var outputStream = await archive.OpenAsync(FileAccessMode.ReadWrite))
            using (var archiveStream = WriterFactory.Open(outputStream.AsStreamForWrite(), Type, options))
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
        /// <exception cref="InvalidArchiveTypeException">Thrown if archive type is not supported.</exception>
        private WriterOptions SetWriterOptions()
        {
            WriterOptions options;

            switch (Type)
            {
                case ArchiveType.Zip:
                    options = new ZipWriterOptions(CompressionType.Deflate);
                    break;

                case ArchiveType.GZip:
                    options = new WriterOptions(CompressionType.GZip);
                    break;

                case ArchiveType.Tar:
                    options = new WriterOptions(CompressionType.BZip2);
                    break;

                case ArchiveType.Rar:
                    options = new WriterOptions(CompressionType.Rar);
                    break;

                case ArchiveType.SevenZip:
                    options = new WriterOptions(CompressionType.LZMA);
                    break;

                default:
                    throw new InvalidArchiveTypeException("Writer options could not be set.");
            }
            return options;
        }
    }
}
