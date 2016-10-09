using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Common.Compression.Algorithm
{
    public abstract class AbstractAlgorithm : ICompressionAlgorithm
    {
        public abstract Task<bool> Extract(StorageFile archive, StorageFolder location);
        public abstract Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive, StorageFolder location);
    }
}
