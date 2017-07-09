using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Model
{
    internal abstract class OperationInfo
    {
        internal StorageFolder OutputFolder { get; set; }
    }
}
