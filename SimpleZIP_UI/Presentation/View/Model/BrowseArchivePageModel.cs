using Windows.UI.Xaml.Controls;

namespace SimpleZIP_UI.Presentation.View.Model
{
    public class BrowseArchivePageModel
    {
        public bool IsNode { get; }

        public string DisplayName { get; set; }

        public Symbol Symbol { get; set; }

        public BrowseArchivePageModel(bool isNode)
        {
            IsNode = isNode;
        }
    }
}
