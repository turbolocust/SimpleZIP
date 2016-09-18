using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;
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

        public async void Compress(IReadOnlyList<StorageFile> files, string archiveName, StorageFolder location)
        {
            var compressionInfo = new CompressionInfo()
            {
                DeflateCompressionLevel = CompressionLevel.Default,
                Type = archiveName.EndsWith("bz2") ? CompressionType.BZip2 : CompressionType.GZip
            };

            var archive = await location.CreateFileAsync(archiveName);
            if (archive != null) // archive created
            {
                using (var fileOutputStream = await archive.OpenAsync(FileAccessMode.ReadWrite))
                using (var archiveStream = new TarWriter(fileOutputStream.AsStreamForWrite(), compressionInfo))
                {
                    foreach (var f in files)
                    {
                        using (var fileInputStream = await f.OpenReadAsync())
                        {
                            archiveStream.Write(f.Name, fileInputStream.AsStreamForRead(), DateTime.Now);
                        }
                    }
                }
            }
        }

        public async void Extract(StorageFile archive)
        {
            using (var fileInputStream = await archive.OpenReadAsync())
            {
                if (fileInputStream != null) // system has now access to file
                {
                    using (var tarReader = TarReader.Open(fileInputStream.AsStreamForRead()))
                    {
                        var outputFolder = await archive.GetParentAsync();
                        if (outputFolder != null) // system has now access to folder
                        {
                            while (tarReader.MoveToNextEntry()) // write each entry to file
                            {
                                var outputFile =
                                    await outputFolder.CreateFileAsync(tarReader.Entry.Key,
                                            CreationCollisionOption.GenerateUniqueName);
                                // write archive entry to output file
                                using (var outputFileStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
                                {
                                    tarReader.WriteEntryTo(outputFileStream.AsStreamForWrite());
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
