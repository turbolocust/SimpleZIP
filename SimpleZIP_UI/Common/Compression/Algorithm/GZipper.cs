using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        public void Compress(IReadOnlyList<StorageFile> files, string archiveName, string location)
        {
            using (var fileStream = new FileStream(Path.Combine(location, archiveName), FileMode.CreateNew))
            using (var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal))
            {
                var file = files[0]; // as gzip only allows compression of one file
                using (var inputStream = new FileStream(file.Path, FileMode.Open))
                {
                    var bytes = new byte[4096];
                    var readBytes = 0;

                    while ((readBytes = inputStream.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        gzipStream.Write(bytes, 0, readBytes);
                    }
                }
            }
        }

        public void Extract(string archiveName, string location)
        {
            var archive = new FileInfo(Path.Combine(location, archiveName));

            using (var fileStream = new FileStream(archive.FullName, FileMode.Open))
            using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
            {
                // remove extension from output file name
                var outputFileName = archive.Name.Substring(0, (archive.Name.Length - archive.Extension.Length));

                using (var outputStream = new FileStream(Path.Combine(location, outputFileName), FileMode.Create))
                {
                    var bytes = new byte[4096];
                    var readBytes = 0;

                    while ((readBytes = gzipStream.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        outputStream.Write(bytes, 0, readBytes);
                    }
                }
            }
        }
    }
}
