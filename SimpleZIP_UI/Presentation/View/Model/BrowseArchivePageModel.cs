using Windows.UI.Xaml.Controls;

namespace SimpleZIP_UI.Presentation.View.Model
{
    /// <summary>
    /// Represents a list box item for the <see cref="BrowseArchivePage"/>.
    /// </summary>
    public class BrowseArchivePageModel
    {
        /// <summary>
        /// True if this model represents a node.
        /// </summary>
        public bool IsNode { get; }

        /// <summary>
        /// A friendly name which will be displayed in the list box.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The symbol that will be displayed together with <see cref="DisplayName"/>
        /// in the list box.
        /// </summary>
        public Symbol Symbol { get; set; }

        public BrowseArchivePageModel(bool isNode)
        {
            IsNode = isNode;
        }
    }
}
