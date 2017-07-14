using SharpCompress.Common;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type
{
    public class Zip : ArchivingAlgorithm
    {
        public Zip() : base(ArchiveType.Zip)
        {
        }

        protected override WriterOptions GetWriterOptions()
        {
            return new ZipWriterOptions(CompressionType.Deflate)
            {
                DeflateCompressionLevel = CompressionLevel.BestSpeed
            };
        }
    }
}