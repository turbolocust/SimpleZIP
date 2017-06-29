using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;

namespace SimpleZIP_UI.Common.Compression.Algorithm.Type
{
    public class GZip : ArchivingAlgorithm
    {
        private static GZip _instance;

        public static GZip Instance => _instance ?? (_instance = new GZip());

        private GZip() : base(ArchiveType.GZip)
        {
            // singleton
        }

        public new async Task<bool> Extract(StorageFile archive, StorageFolder location, ReaderOptions options = null)
        {
            if (archive == null || location == null) return false;

            using (var fileStream = await archive.OpenReadAsync())
            using (var gzipStream = new GZipStream(fileStream.AsStreamForRead(), CompressionMode.Decompress))
            {
                // remove extension from output file name
                var outputFileName = archive.Name.Substring(0, archive.Name.Length - archive.FileType.Length);

                var file = await location.CreateFileAsync(outputFileName, CreationCollisionOption.GenerateUniqueName);
                if (file != null) // file created
                {
                    using (var outputStream = await file.OpenStreamForWriteAsync())
                    {
                        var bytes = new byte[DefaultBufferSize];
                        int readBytes;

                        while (!IsInterrupted() && (readBytes = gzipStream.Read(bytes, 0, bytes.Length)) > 0)
                        {
                            await outputStream.WriteAsync(bytes, 0, readBytes);
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public new async Task<bool> Compress(StorageFile file, StorageFile archive, StorageFolder location, WriterOptions options = null)
        {
            if (file == null || archive == null || location == null) return false;

            using (var outputFileStream = await archive.OpenAsync(FileAccessMode.ReadWrite))
            using (var gzipStream = new GZipStream(outputFileStream.AsStreamForWrite(), CompressionLevel.Optimal))
            {
                using (var inputStream = await file.OpenStreamForReadAsync())
                {
                    var bytes = new byte[DefaultBufferSize];
                    int readBytes;

                    while (!IsInterrupted() && (readBytes = await inputStream.ReadAsync(bytes, 0, bytes.Length)) > 0)
                    {
                        await gzipStream.WriteAsync(bytes, 0, readBytes);
                    }
                }
            }
            return true;
        }

        public new Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive, StorageFolder location, WriterOptions options = null)
        {
            throw new NotSupportedException("Gzip only allows the compression of a single file.");
        }

        protected override WriterOptions GetWriterOptions()
        {
            return new WriterOptions(CompressionType.GZip);
        }
    }
}
