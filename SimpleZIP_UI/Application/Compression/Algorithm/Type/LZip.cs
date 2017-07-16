using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Compressors;
using SharpCompress.Compressors.LZMA;
using SimpleZIP_UI.Application.Compression.Model;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type
{
    internal class LZip : CompressorAlgorithm
    {
        protected override async Task<Stream> GetCompressorStream(StorageFile archive, CompressorOptions options)
        {
            return options.IsCompression
                ? new LZipStream(await archive.OpenStreamForWriteAsync(), CompressionMode.Compress)
                : new LZipStream(await archive.OpenStreamForReadAsync(), CompressionMode.Decompress);
        }
    }
}
