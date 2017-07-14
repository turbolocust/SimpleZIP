﻿using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type
{
    public class GZip : CompressorAlgorithm
    {
        protected override async Task<Stream> GetCompressorStream(StorageFile archive, bool compress)
        {
            return compress
                ? new GZipStream(await archive.OpenStreamForWriteAsync(), CompressionMode.Compress, CompressionLevel.BestSpeed)
                : new GZipStream(await archive.OpenStreamForReadAsync(), CompressionMode.Decompress);
        }
    }
}
