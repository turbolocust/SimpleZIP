﻿using SharpCompress.Common;
using SharpCompress.Writers;

namespace SimpleZIP_UI.Common.Compression.Algorithm.Type
{
    public class Tar : ArchivingAlgorithm, IArchiveType
    {
        private static Tar _instance;

        public static Tar Instance => _instance ?? (_instance = new Tar());

        private Tar() : base(ArchiveType.Tar)
        {
            // singleton
        }

        protected override WriterOptions GetWriterOptions()
        {
            return new WriterOptions(CompressionType.BZip2);
        }
    }
}
