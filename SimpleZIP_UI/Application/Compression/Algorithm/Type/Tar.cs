using SharpCompress.Common;
using SharpCompress.Writers;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type
{
    public class Tar : ArchivingAlgorithm
    {
        private static Tar _instance;

        public static Tar Instance => _instance ?? (_instance = new Tar());

        public Tar() : base(ArchiveType.Tar)
        {
            // singleton
        }

        protected override WriterOptions GetWriterOptions()
        {
            return new WriterOptions(CompressionType.None);
        }
    }
}
