using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Common;
using SharpCompress.Readers.Zip;
using SharpCompress.Writers.Zip;

namespace SimpleZIP_UI.Common.Compression.Algorithm
{
    public class Zip : AbstractAlgorithm
    {
        private static Zip _instance;

        public static Zip Instance => _instance ?? (_instance = new Zip());

        private Zip()
        {
            // singleton
        }

        public override async Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive, StorageFolder location)
        {
            if (files == null || archive == null || location == null) return false;

            var options = new ZipWriterOptions(CompressionType.Deflate);

            using (var fileOutputStream = await archive.OpenAsync(FileAccessMode.ReadWrite))
            using (var archiveStream = new ZipWriter(fileOutputStream.AsStreamForWrite(), options))
            {
                foreach (var file in files)
                {
                    using (var fileStream = await file.OpenStreamForReadAsync())
                    {
                        archiveStream.Write(file.Name, fileStream, DateTime.Now);
                    }
                }
            }
            return true;
        }

        public override async Task<bool> Extract(StorageFile archive, StorageFolder location)
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
                            using (var fileStream = await file.OpenStreamForWriteAsync())
                            {
                                zipReader.WriteEntryTo(fileStream);
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