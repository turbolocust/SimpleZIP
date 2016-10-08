using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Common.Compression.Algorithm
{
    public class GZipper : ICompressionAlgorithm
    {
        private static GZipper _instance;

        public static GZipper Instance => _instance ?? (_instance = new GZipper());

        private GZipper()
        {
            // singleton
        }

        public async Task<bool> Compress(IReadOnlyList<StorageFile> files, string archiveName, StorageFolder location)
        {
            var outputFile = await location.CreateFileAsync(archiveName);
            if (outputFile != null) // file created
            {
                using (var outputFileStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
                using (var gzipStream = new GZipStream(outputFileStream.AsStreamForWrite(), CompressionLevel.Optimal))
                {
                    var file = files[0]; // as gzip only allows compression of one file

                    using (var inputStream = (await file.OpenReadAsync()).AsStreamForRead())
                    {
                        var bytes = new byte[4096];
                        int readBytes;

                        while ((readBytes = await inputStream.ReadAsync(bytes, 0, bytes.Length)) > 0)
                        {
                            await gzipStream.WriteAsync(bytes, 0, readBytes);
                        }
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public async Task<bool> Extract(StorageFile archive, StorageFolder location)
        {
            using (var fileStream = await archive.OpenReadAsync())
            using (var gzipStream = new GZipStream(fileStream.AsStreamForRead(), CompressionMode.Decompress))
            {
                // remove extension from output file name
                var outputFileName = archive.Name.Substring(0, (archive.Name.Length - archive.FileType.Length));

                var file = await location.CreateFileAsync(outputFileName, CreationCollisionOption.GenerateUniqueName);
                if (file != null) // file created
                {
                    using (var outputStream = (await file.OpenAsync(FileAccessMode.ReadWrite)).AsStreamForWrite())
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
