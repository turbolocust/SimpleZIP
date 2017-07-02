using SharpCompress.Common;
using SharpCompress.Writers;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type
{
    public class Tarball : ArchivingAlgorithm
    {
        private static Tarball _instance;

        public static Tarball Instance => _instance ?? (_instance = new Tarball());

        private Tarball() : base(ArchiveType.Tar)
        {
            // singleton
        }

        protected override WriterOptions GetWriterOptions()
        {
            return new WriterOptions(CompressionType.BZip2); // default
        }
    }
}
