﻿using SharpCompress.Common;

namespace SimpleZIP_UI.Common.Compression.Algorithm.Type
{
    public class Tar : AbstractAlgorithm, IArchiveType
    {
        private static Tar _instance;

        public static Tar Instance => _instance ?? (_instance = new Tar());

        private Tar() : base(ArchiveType.Tar)
        {
            // singleton
        }
    }
}
