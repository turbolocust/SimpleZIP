// ==++==
// 
// Copyright (C) 2019 Matthias Fussenegger
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

using SimpleZIP_UI.Application.Hashing;
using SimpleZIP_UI.Presentation.Controller;
using SimpleZIP_UI.Presentation.View.Dialog;
using SimpleZIP_UI.Presentation.View.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MessageDigestPage : INavigation
    {
        /// <summary>
        /// Models bound to the combo box in view.
        /// </summary>
        public ObservableCollection<HashAlgorithmModel> HashAlgorithmModels { get; }

        /// <summary>
        /// Models bound to the list box in view.
        /// </summary>
        public ObservableCollection<MessageDigestModel> MessageDigestModels { get; }

        /// <summary>
        /// Enables or disables certain UI elements.
        /// </summary>
        public BooleanModel IsPopulateListBox { get; set; } = false;

        /// <summary>
        /// The currently selected algorithm (ComboBox item).
        /// </summary>
        public HashAlgorithmModel SelectedAlgorithm { get; set; }

        /// <summary>
        /// The aggregated controller instance.
        /// </summary>
        private readonly MessageDigestController _controller;

        /// <summary>
        /// Instance used to compute hashes.
        /// </summary>
        private readonly IMessageDigestProvider _messageDigestProvider;

        /// <summary>
        /// List of selected files needed for re-computation of hash values.
        /// </summary>
        private IReadOnlyList<StorageFile> _selectedFiles = new StorageFile[] { };

        /// <inheritdoc />
        public MessageDigestPage()
        {
            _controller = new MessageDigestController(this);
            _messageDigestProvider = new MessageDigestProvider();
            HashAlgorithmModels = new ObservableCollection<HashAlgorithmModel>();
            MessageDigestModels = new ObservableCollection<MessageDigestModel>();
            InitializeComponent(); // has to be called after creating lists
            InitHashAlgorithmComboBox();
            LoadToggleButtonsState();
        }

        private void InitHashAlgorithmComboBox()
        {
            var algorithms = _messageDigestProvider.SupportedAlgorithms;
            foreach (string algorithm in algorithms)
            {
                HashAlgorithmModels.Add(new HashAlgorithmModel(algorithm));
            }

            int i = LoadHashAlgorithmIndex();
            SelectedAlgorithm = HashAlgorithmModels[i];
        }

        /// <summary>
        /// Populates the list box with file names and computed hash values.
        /// This may only be called once UI components are initialized.
        /// </summary>
        private async void PopulateListBox()
        {
            try
            {
                NavigationLock.Instance.IsLocked = true;
                IsPopulateListBox.IsTrue = true;
                MessageDigestModels.Clear();

                string algorithmName = SelectedAlgorithm.HashAlgorithm;
                bool isLowercase = LowercaseHashToggleSwitch.IsOn;
                bool isDisplayLocation = DisplayLocationToggleSwitch.IsOn;

                var models = await BuildModelsAsync(algorithmName, isLowercase, isDisplayLocation);
                models.ForEach(model => MessageDigestModels.Add(model));
            }
            finally
            {
                IsPopulateListBox.IsTrue = false;
                NavigationLock.Instance.IsLocked = false;
            }
        }

        /// <summary>
        /// Asynchronously builds all <see cref="MessageDigestModel"/>
        /// which are to be displayed in the ListBox of the UI.
        /// </summary>
        /// <param name="algorithmName">The name of the algorithm.</param>
        /// <param name="isLowercase">True for lower case hash value.</param>
        /// <param name="isDisplayLocation">True if location is to be displayed.</param>
        /// <returns>A list consisting of <see cref="MessageDigestModel"/>.</returns>
        private async Task<List<MessageDigestModel>> BuildModelsAsync(
            string algorithmName, bool isLowercase, bool isDisplayLocation)
        {
            var models = new List<MessageDigestModel>();

            if (_messageDigestProvider.SupportedAlgorithms.Contains(algorithmName))
            {
                // start computation of hash value for each file
                foreach (var file in _selectedFiles)
                {
                    // suppress hashed bytes (string is sufficient)
                    var (_, hash) = await _messageDigestProvider
                        .ComputeHashValue(file, algorithmName);

                    if (isLowercase)
                    {
                        hash = hash.ToLowerInvariant();
                    }

                    models.Add(new MessageDigestModel(file.Name, file.Path, hash)
                    {
                        IsDisplayLocation = isDisplayLocation
                    });
                }
            }

            return models;
        }

        private void SaveHashAlgorithmIndex()
        {
            int index = HashAlgorithmModels.IndexOf(SelectedAlgorithm);
            Settings.PushOrUpdate(Settings.Keys.RecentHashAlgorithmKey, index);
        }

        private static int LoadHashAlgorithmIndex()
        {
            bool exists = Settings.TryGet(Settings
                .Keys.RecentHashAlgorithmKey, out int index);

            if (!exists || index < 0)
            {
                index = 0; // set to first element
            }

            return index;
        }

        private void LoadToggleButtonsState()
        {
            // LowercaseHashToggleSwitch
            bool exists = Settings.TryGet(Settings
                .Keys.LowerCaseHashToggledKey, out bool toggle);

            if (exists)
            {
                LowercaseHashToggleSwitch.IsOn = toggle;
            }

            // DisplayLocationToggleSwitch
            exists = Settings.TryGet(Settings
                .Keys.DisplayLocationToggledKey, out toggle);

            if (exists)
            {
                DisplayLocationToggleSwitch.IsOn = toggle;
            }
        }

        private void RefreshListBox()
        {
            MessageDigestModelsListBox.ItemsSource = null;
            MessageDigestModelsListBox.ItemsSource = MessageDigestModels;
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

        private void HashAlgorithmComboBox_OnSelectionChanged(
            object sender, SelectionChangedEventArgs args)
        {
            SaveHashAlgorithmIndex();
            PopulateListBox();
        }

        private async void ViewFullHashButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            if (!(args.OriginalSource is FrameworkElement element)) return;
            if (element.DataContext is MessageDigestModel model)
            {
                await new ViewTextDialog(model.FileName, model.HashValue).ShowAsync();
                args.Handled = true;
            }
        }

        private void CopyHashButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            if (!(args.OriginalSource is FrameworkElement element)) return;
            if (element.DataContext is MessageDigestModel model)
            {
                CopyToClipboard(model.HashValue);
                args.Handled = true;
            }
        }

        private void CopyAllButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            var stringBuilder = new StringBuilder();
            foreach (var model in MessageDigestModels)
            {
                stringBuilder.AppendLine(model.FileName);
                stringBuilder.AppendLine(model.Location);
                stringBuilder.AppendLine(model.HashValue);
                stringBuilder.AppendLine();
            }
            CopyToClipboard(stringBuilder.ToString());
            // show toast (without audio) and hide it after 4 seconds
            _controller.ShowToastNotification("SimpleZIP",
                I18N.Resources.GetString("CopiedToClipboard/Text"), 4, true);
        }

        private void LowercaseHashToggleSwitch_OnToggled(object sender, RoutedEventArgs args)
        {
            // save state of toggle button
            Settings.PushOrUpdate(
                Settings.Keys.LowerCaseHashToggledKey,
                LowercaseHashToggleSwitch.IsOn);

            foreach (var model in MessageDigestModels)
            {
                model.HashValue = LowercaseHashToggleSwitch.IsOn
                    ? model.HashValue.ToLowerInvariant()
                    : model.HashValue.ToUpperInvariant();
            }

            RefreshListBox();
        }

        private void DisplayLocationToggleSwitch_OnToggled(object sender, RoutedEventArgs args)
        {
            // save state of toggle button
            Settings.PushOrUpdate(
                Settings.Keys.DisplayLocationToggledKey,
                DisplayLocationToggleSwitch.IsOn);

            foreach (var model in MessageDigestModels)
            {
                model.IsDisplayLocation.IsTrue = DisplayLocationToggleSwitch.IsOn;
            }

            RefreshListBox();
        }

        private void MessageDigestModelsGrid_OnRightTapped(object sender, RightTappedRoutedEventArgs args)
        {
            if (args.PointerDeviceType == PointerDeviceType.Mouse
                && sender is FrameworkElement elem)
            {
                FlyoutBase.GetAttachedFlyout(elem).ShowAt(elem);
                args.Handled = true;
            }
        }

        private async void CompareHashFlyoutItem_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            if (sender is MenuFlyoutItem flyoutItem)
            {
                var model = (MessageDigestModel)flyoutItem.DataContext;
                var dialog = new CompareHashDialog(model.HashValue);
                await dialog.ShowAsync();
            }
        }

        private async void CompareHashFlyoutItem_OnClick(object sender, RoutedEventArgs args)
        {
            if (sender is MenuFlyoutItem flyoutItem)
            {
                var model = (MessageDigestModel)flyoutItem.DataContext;
                var dialog = new CompareHashDialog(model.HashValue);
                await dialog.ShowAsync();
            }
        }

        /// <inheritdoc />
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            if (args.Parameter is FilesNavigationArgs navigationArgs)
            {
                _selectedFiles = navigationArgs.StorageFiles;
                _controller.ShareOperation = navigationArgs.ShareOperation;
            }
        }

        /// <inheritdoc />
        public void Navigate(Type destinationPageType, object parameter = null)
        {
            if (parameter == null)
            {
                var args = new PageNavigationArgs(typeof(MessageDigestPage));
                Frame.Navigate(destinationPageType, args);
            }
            else
            {
                Frame.Navigate(destinationPageType, parameter);
            }
        }
    }
}
