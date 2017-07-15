using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using SimpleZIP_UI.Application.Compression.Model;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type
{
    public class GZip : CompressorAlgorithm
    {
        protected override async Task<Stream> GetCompressorStream(StorageFile archive, CompressorOptions options)
        {
            var stream = options.IsCompression
                ? new GZipStream(await archive.OpenStreamForWriteAsync(),
                        CompressionMode.Compress, CompressionLevel.BestSpeed)
                : new GZipStream(await archive.OpenStreamForReadAsync(), CompressionMode.Decompress);

            // set file name to stream
            var fileName = options.FileName;
            if (!string.IsNullOrEmpty(fileName))
            {
                stream.FileName = fileName;
            }

            return stream;
        }
    }
}
