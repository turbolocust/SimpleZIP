using System.IO;
using System.IO.Compression;

namespace SimpleZIP_UI.Compression
{
    public class Zipper : ICompressionAlgorithm
    {

        private static Zipper _instance;

        public static Zipper Instance => _instance ?? (_instance = new Zipper());

        private Zipper()
        {
            // singleton
        }

        public void Compress(FileInfo[] files, string archiveName, string location)
        {
            using (var memoryStream = new MemoryStream()) // work in memory
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var t in files)
                    {
                        archive.CreateEntryFromFile(t.FullName, t.Name);
                    }
                }

                // actually write the memory stream to a zip archive
                using (var fileStream = new FileStream(@location + archiveName, FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }
        }

        public void Extract(string archiveName, string location)
        {
            ZipFile.ExtractToDirectory(archiveName, location);
        }

    }
}
