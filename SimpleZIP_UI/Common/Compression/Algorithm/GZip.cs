using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Common.Compression.Algorithm
{
    public class GZip : AbstractAlgorithm
    {
        private static GZip _instance;

        public static GZip Instance => _instance ?? (_instance = new GZip());

        private GZip()
        {
            // singleton
        }

        public override async Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive, StorageFolder location)
        {
            if (files == null || archive == null || location == null) return false;

            using (var outputFileStream = await archive.OpenAsync(FileAccessMode.ReadWrite))
            using (var gzipStream = new GZipStream(outputFileStream.AsStreamForWrite(), CompressionLevel.Optimal))
            {
                var file = files[0]; // as gzip only allows compression of one file

                using (var inputStream = await file.OpenStreamForReadAsync())
                {
                    var bytes = new byte[4096];
                    int readBytes;

                    while ((readBytes = await inputStream.ReadAsync(bytes, 0, bytes.Length)) > 0)
                    {
                        await gzipStream.WriteAsync(bytes, 0, readBytes);
                    }
                }
            }
            return true;
        }

        public override async Task<bool> Extract(StorageFile archive, StorageFolder location)
        {
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
                        var bytes = new byte[4096];
                        int readBytes;

                        while ((readBytes = gzipStream.Read(bytes, 0, bytes.Length)) > 0)
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
    }
}
