using SharpCompress.Common;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;

namespace SimpleZIP_UI.Common.Compression.Algorithm.Type
{
    public class Zip : ArchivingAlgorithm
    {
        private static Zip _instance;

        public static Zip Instance => _instance ?? (_instance = new Zip());

        private Zip() : base(ArchiveType.Zip)
        {
            // singleton
        }

        protected override WriterOptions GetWriterOptions()
        {
            return new ZipWriterOptions(CompressionType.Deflate);
        }
    }
}