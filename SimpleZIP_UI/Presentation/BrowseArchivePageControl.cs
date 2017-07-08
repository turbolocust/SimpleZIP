using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Compression.Reader;

namespace SimpleZIP_UI.Presentation
{
    internal class BrowseArchivePageControl : BaseControl
    {
        internal BrowseArchivePageControl(Page parent) : base(parent)
        {
        }

        internal async Task<Node> ReadArchive(StorageFile archive)
        {
            using (var reader = new ArchiveReader())
            {
                await reader.OpenArchiveAsync(archive, false);
                return reader.Read();
            }
        }
    }
}
