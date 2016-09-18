using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using SharpCompress.Common;
using SharpCompress.Compressor.Deflate;
using SharpCompress.Reader.Zip;
using SharpCompress.Writer.Zip;

namespace SimpleZIP_UI.Common.Compression.Algorithm
{
    public class Zipper : ICompressionAlgorithm
    {
        private static Zipper _instance;

        public static Zipper Instance => _instance ?? (_instance = new Zipper());

        private Zipper()
        {
            // singleton
        }

        public async void Compress(IReadOnlyList<StorageFile> files, string archiveName, StorageFolder location)
        {

            var compressionInfo = new CompressionInfo()
            {
                DeflateCompressionLevel = CompressionLevel.Default,
                Type = CompressionType.Deflate
            };

            var archive = await location.CreateFileAsync(archiveName);
            if (archive != null) // archive created
            {
                using (var fileOutputStream = await archive.OpenAsync(FileAccessMode.ReadWrite))
                using (var archiveStream = new ZipWriter(fileOutputStream.AsStreamForWrite(), compressionInfo, ""))
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
                    using (var zipReader = ZipReader.Open(fileInputStream.AsStreamForRead()))
                    {
                        var outputFolder = await archive.GetParentAsync();
                        if (outputFolder != null) // system has now access to parent
                        {
                            while (zipReader.MoveToNextEntry()) // write each entry to file
                            {
                                var outputFile = await outputFolder.CreateFileAsync(zipReader.Entry.Key);
                                // write archive entry to output file
                                using (var outputFileStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
                                {
                                    zipReader.WriteEntryTo(outputFileStream.AsStreamForWrite());
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}