using SharpCompress.Common;

namespace SimpleZIP_UI.Common.Compression.Algorithm.Type
{
    public class Zip : AbstractAlgorithm, IArchiveType
    {
        private static Zip _instance;

        public static Zip Instance => _instance ?? (_instance = new Zip());

        private Zip() : base(ArchiveType.Zip)
        {
            // singleton
        }
    }
}