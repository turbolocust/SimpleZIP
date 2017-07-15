﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type
{
    public class SevenZip : ArchivingAlgorithm
    {
        public SevenZip() : base(ArchiveType.SevenZip)
        {
        }

        public new Task<bool> Decompress(StorageFile archive, StorageFolder location, ReaderOptions options = null)
        {
            throw new NotImplementedException();
        }

        public new Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive, StorageFolder location, WriterOptions options = null)
        {
            throw new NotImplementedException();
        }

        protected override WriterOptions GetWriterOptions()
        {
            return new WriterOptions(CompressionType.LZMA);
        }
    }
}
