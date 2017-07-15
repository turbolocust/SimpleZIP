using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Compressors;
using SharpCompress.Compressors.BZip2;
using SimpleZIP_UI.Application.Compression.Model;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type
{
    internal class BZip2 : CompressorAlgorithm
    {
        protected override async Task<Stream> GetCompressorStream(StorageFile archive, CompressorOptions options)
        {
            return options.IsCompression
                ? new BZip2Stream(await archive.OpenStreamForWriteAsync(), CompressionMode.Compress)
                : new BZip2Stream(await archive.OpenStreamForReadAsync(), CompressionMode.Decompress);
        }
    }
}
