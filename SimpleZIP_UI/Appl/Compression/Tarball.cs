using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCompress.Archive;
using SharpCompress.Archive.Tar;
using SharpCompress.Common;
using SharpCompress.Writer;
using SimpleZIP_UI.Exceptions;

namespace SimpleZIP_UI.Appl.Compression
{
    internal class Tarball : ICompressionAlgorithm
    {

        private static Tarball _instance;

        public static Tarball Instance => _instance ?? (_instance = new Tarball());

        private Tarball()
        {
            // singleton
        }

        public void Compress(FileInfo[] files, string archiveName, string location)
        {
            using (var archive = TarArchive.Create())
            {
                foreach (var f in files)
                {
                    //TODO
                    archive.AddEntry(f.Name, f.FullName);
                }

                archive.SaveTo(location, CompressionType.GZip);
            }

        }

        public void Extract(string archiveName, string location)
        {
            if (TarArchive.IsTarFile(archiveName))
            {
                using (var memoryStream = new MemoryStream())
                using (var archive = TarArchive.Open(archiveName))
                {
                    var reader = archive.ExtractAllEntries();
                    while (reader.MoveToNextEntry())
                    {
                        reader.WriteEntryTo(memoryStream);
                    }

                    using (var fileStream = new FileStream(@location + archiveName, FileMode.Create))
                    {
                        memoryStream.Seek(0, SeekOrigin.Begin);
                        memoryStream.CopyTo(fileStream);
                    }
                }
            }
            else
            {
                throw new InvalidFileTypeException("File is not a TAR archive");
            }
        }
    }
}
