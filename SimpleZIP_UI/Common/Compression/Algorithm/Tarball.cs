using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using SharpCompress.Common;
using SharpCompress.Compressor.Deflate;
using SharpCompress.Reader;
using SharpCompress.Reader.Tar;
using SharpCompress.Writer.Tar;

namespace SimpleZIP_UI.Common.Compression.Algorithm
{
    internal class Tarball : ICompressionAlgorithm
    {
        private static Tarball _instance;

        public static Tarball Instance => _instance ?? (_instance = new Tarball());

        private Tarball()
        {
            // singleton
        }

        public void Compress(IReadOnlyList<StorageFile> files, string archiveName, string location)
        {
            var compressionInfo = new CompressionInfo()
            {
                DeflateCompressionLevel = CompressionLevel.Default,
                Type = archiveName.EndsWith("bz2") ? CompressionType.BZip2 : CompressionType.GZip
            };

            using (var fileOutputStream = new FileStream(Path.Combine(location, archiveName), FileMode.CreateNew))
            using (var archiveStream = new TarWriter(fileOutputStream, compressionInfo))
            {
                foreach (var f in files)
                {
                    using (var fileInputStream = new FileStream(f.Path, FileMode.Open))
                    {
                        archiveStream.Write(f.Name, fileInputStream, DateTime.Now);
                    }
                }
            }
        }

        public void Extract(string archiveName, string location)
        {
            using (var fileInputStream = new FileStream(archiveName, FileMode.Open))
            using (var tarReader = TarReader.Open(fileInputStream))
            {
                tarReader.WriteAllToDirectory(location, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
            }
        }
    }
}
