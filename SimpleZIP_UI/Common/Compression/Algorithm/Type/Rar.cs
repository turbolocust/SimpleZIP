using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;

namespace SimpleZIP_UI.Common.Compression.Algorithm.Type
{
    internal class Rar : ArchivingAlgorithm
    {
        private static Rar _instance;

        public static Rar Instance => _instance ?? (_instance = new Rar());

        private Rar() : base(ArchiveType.Rar)
        {
            // singleton
        }

        public new Task<bool> Extract(StorageFile archive, StorageFolder location, ReaderOptions options = null)
        {
            throw new NotImplementedException();
        }

        public new Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive, StorageFolder location, WriterOptions options = null)
        {
            throw new NotImplementedException();
        }

        protected override WriterOptions GetWriterOptions()
        {
            return new WriterOptions(CompressionType.Rar);
        }
    }
}
