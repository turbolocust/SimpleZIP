using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Presentation.View.Model;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MessageDigestPage
    {
        /// <summary>
        /// Models bound to the list box in view.
        /// </summary>
        public ObservableCollection<MessageDigestModel> MessageDigestModels { get; }

        /// <summary>
        /// Temporarily stores selected files until hash calculation has begun.
        /// </summary>
        private IReadOnlyList<StorageFile> _selectedFiles;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MessageDigestPage()
        {
            InitializeComponent();
            MessageDigestModels = new ObservableCollection<MessageDigestModel>();
        }

        private void CopyHashButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            throw new System.NotImplementedException();
        }

        private void CopyAllButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            var stringBuilder = new StringBuilder();
            foreach (var model in MessageDigestModels)
            {
                stringBuilder.AppendLine(model.FileName);
                stringBuilder.AppendLine(model.HashValue);
                stringBuilder.AppendLine("\r\n");
            }
            // copy values to clipboard
            var package = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
            package.SetText(stringBuilder.ToString());
            Clipboard.SetContent(package);
        }

        /// <inheritdoc />
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            if (!(args.Parameter is IReadOnlyList<StorageFile> files)) return;
            _selectedFiles = files;
        }
    }
}
