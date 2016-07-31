using System.IO;
using System.IO.Compression;

namespace SimpleZIP_UI.Appl.Compression
{
    public class GZipper : ICompressionAlgorithm
    {

        private static GZipper _instance;

        public static GZipper Instance => _instance ?? (_instance = new GZipper());

        private GZipper()
        {
            // singleton
        }

        public void Compress(FileInfo[] files, string archiveName, string location)
        {
            using (var inputStream = new FileStream(files[0].FullName, FileMode.Open))
            {
                var file = files[0]; // as gzip only allows compression of one file
                var bytes = new byte[file.Length];
                inputStream.Read(bytes, 0, files.Length); //read file to bytes array

                var archive = new FileInfo(location + archiveName);

                using (var outputStream = new FileStream(archive.FullName, FileMode.Create))
                using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    gzipStream.Write(bytes, 0, bytes.Length); // write bytes to archive
                }
            }
        }

        public void Extract(string archiveName, string location)
        {
            var archive = new FileInfo(archiveName);

            using (var inputStream = new FileStream(archive.FullName, FileMode.Open))
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                const int size = 4096;
                var buffer = new byte[size];

                // make new file without file extension of gzip
                var outputFile = new FileInfo(location + archiveName.Substring(0, archiveName.LastIndexOf('.') - 1));
                using (var outputStream = new FileStream(outputFile.FullName, FileMode.Create))
                {
                    var readBytes = 0;
                    do
                    {
                        readBytes = gzipStream.Read(buffer, 0, size);
                        if (readBytes > 0)
                        {
                            outputStream.Write(buffer, 0, readBytes);
                        }
                    } while (readBytes > 0);
                }
            }
        }
    }
}
