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
    internal class Tarball : ICompressionAlgorithm
    {
        private static Tarball _instance;

        public static Tarball Instance => _instance ?? (_instance = new Tarball());

        private Tarball()
        {
            // singleton
        }

        public async Task<bool> Compress(IReadOnlyList<StorageFile> files, string archiveName, StorageFolder location)
        {
            var options = new WriterOptions(archiveName.EndsWith("bz2") ? CompressionType.BZip2 : CompressionType.GZip);

            var archive = await location.CreateFileAsync(archiveName);
            if (archive != null) // archive created
            {
                using (var fileOutputStream = await archive.OpenAsync(FileAccessMode.ReadWrite))
                using (var archiveStream = new TarWriter(fileOutputStream.AsStreamForWrite(), options))
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
                using (var tarReader = TarReader.Open(fileInputStream.AsStreamForRead()))
                {
                    while (tarReader.MoveToNextEntry()) // write each entry to file
                    {
                        var file = await location.CreateFileAsync(tarReader.Entry.Key,
                                    CreationCollisionOption.GenerateUniqueName);
                        if (file != null)
                        {
                            using (var outputFileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                            {
                                tarReader.WriteEntryTo(outputFileStream.AsStreamForWrite());
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
