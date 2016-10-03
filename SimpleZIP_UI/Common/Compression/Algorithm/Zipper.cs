using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

        public async Task<bool> Compress(IReadOnlyList<StorageFile> files, string archiveName, StorageFolder location)
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
            else
            {
                return false;
            }
            return true;
        }

        public async Task<bool> Extract(StorageFile archive, StorageFolder location)
        {
            using (var fileInputStream = await archive.OpenReadAsync())
            {
                using (var zipReader = ZipReader.Open(fileInputStream.AsStreamForRead()))
                {
                    while (zipReader.MoveToNextEntry()) // write each entry to file
                    {
                        var file = await location.CreateFileAsync(zipReader.Entry.Key,
                                    CreationCollisionOption.GenerateUniqueName);
                        if (file != null)
                        {
                            using (var outputFileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                            {
                                zipReader.WriteEntryTo(outputFileStream.AsStreamForWrite());
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}