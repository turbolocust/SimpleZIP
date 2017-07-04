using SharpCompress.Common;
using SharpCompress.Writers;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type
{
    public class TarGzip : Tar
    {
        protected override WriterOptions GetWriterOptions()
        {
            return new WriterOptions(CompressionType.GZip);
        }
    }
}
