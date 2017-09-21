// ==++==
// 
// Copyright (C) 2017 Matthias Fussenegger
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// ==--==
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Application.Hashing;
using SimpleZIP_UI.Presentation.View.Dialog;
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
        /// Instance used to compute hashes.
        /// </summary>
        private readonly IMessageDigestProvider _messageDigestAlgorithm;

        /// <summary>
        /// List of selected files needed for re-computation of hash values.
        /// </summary>
        private IReadOnlyList<StorageFile> _selectedFiles = new StorageFile[] { };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MessageDigestPage()
        {
            _messageDigestAlgorithm = new MessageDigestProvider();
            MessageDigestModels = new ObservableCollection<MessageDigestModel>();
            InitializeComponent();
        }

        private async Task PopulateListBox()
        {
            try
            {
                MessageDigestModels.Clear();
                HashAlgorithmComboBox.IsEnabled = false;
                // start computation of hash value for each file
                foreach (var file in _selectedFiles)
                {
                    var selectedItem = (ComboBoxItem)HashAlgorithmComboBox.SelectedItem;
                    var algorithmName = selectedItem?.Content as string;
                    // perform this check to avoid exceptions in case something in the UI changes
                    if (_messageDigestAlgorithm.SupportedAlgorithms.Contains(algorithmName))
                    {
                        var hashedData = await _messageDigestAlgorithm.ComputeHashValue(file, algorithmName);
                        var model = new MessageDigestModel(file.Name, file.Path, hashedData.HashedValue);
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { MessageDigestModels.Add(model); });
                    }
                }
            }
            finally
            {
                HashAlgorithmComboBox.IsEnabled = true;
            }
        }

        private static void CopyToClipboard(string text)
        {
            var package = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            package.SetText(text);
            Clipboard.SetContent(package);
        }

        private async void HashAlgorithmComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            await PopulateListBox();
        }

        private async void ViewFullHashButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            if (!(args.OriginalSource is FrameworkElement element)) return;
            if (element.DataContext is MessageDigestModel model)
            {
                await new ViewTextDialog(model.HashValue).ShowAsync();
                args.Handled = true;
            }
        }

        private void CopyHashButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            if (!(args.OriginalSource is FrameworkElement element)) return;
            if (element.DataContext is MessageDigestModel model)
            {
                CopyToClipboard(model.HashValue);
                args.Handled = true;
            }
        }

        private void CopyAllButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            var stringBuilder = new StringBuilder();
            foreach (var model in MessageDigestModels)
            {
                stringBuilder.AppendLine(model.FileName);
                stringBuilder.AppendLine(model.Location);
                stringBuilder.AppendLine(model.HashValue);
                stringBuilder.AppendLine("\r\n");
            }
            CopyToClipboard(stringBuilder.ToString());
        }

        /// <inheritdoc />
        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            if (args.Parameter is IReadOnlyList<StorageFile> files)
            {
                _selectedFiles = files;
                await PopulateListBox();
            }
        }
    }
}
