using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Common;
using SharpCompress.Readers.Tar;
using SharpCompress.Writers;
using SharpCompress.Writers.Tar;

namespace SimpleZIP_UI.Common.Compression.Algorithm
{
    public class Tar : AbstractAlgorithm
    {
        private static Tar _instance;

        public static Tar Instance => _instance ?? (_instance = new Tar());

        private Tar()
        {
            // singleton
        }

        public override async Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive, StorageFolder location)
        {
            if (files == null || archive == null || location == null) return false;

            var options = new WriterOptions(archive.FileType.EndsWith("bz2") ? CompressionType.BZip2 : CompressionType.GZip);

            using (var fileOutputStream = await archive.OpenAsync(FileAccessMode.ReadWrite))
            using (var archiveStream = new TarWriter(fileOutputStream.AsStreamForWrite(), options))
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
                using (var tarReader = TarReader.Open(fileInputStream.AsStreamForRead()))
                {
                    while (tarReader.MoveToNextEntry()) // write each entry to file
                    {
                        var file = await location.CreateFileAsync(tarReader.Entry.Key,
                                    CreationCollisionOption.GenerateUniqueName);
                        if (file != null)
                        {
                            using (var fileStream = await file.OpenStreamForWriteAsync())
                            {
                                tarReader.WriteEntryTo(fileStream);
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
