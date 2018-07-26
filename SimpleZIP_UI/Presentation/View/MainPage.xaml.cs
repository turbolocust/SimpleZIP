// ==++==
// 
// Copyright (C) 2018 Matthias Fussenegger
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
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
using Windows.System;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Presentation.Factory;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using SimpleZIP_UI.Presentation.Controller;
using SimpleZIP_UI.Presentation.Handler;
using SimpleZIP_UI.Presentation.View.Model;
using static SimpleZIP_UI.Presentation.Controller.MainPageController;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    public sealed partial class MainPage
    {
        /// <summary>
        /// Constant which defines the preferred width of the view.
        /// </summary>
        private const double PreferredLaunchSizeWidth = 1024d;

        /// <summary>
        /// Constant which defines the preferred height of the view.
        /// </summary>
        private const double PreferredLaunchSizeHeight = 780d;

        /// <summary>
        /// The aggregated controller instance.
        /// </summary>
        private readonly MainPageController _controller;

        /// <summary>
        /// Models bound to the list view.
        /// </summary>
        public ObservableCollection<RecentArchiveModel> RecentArchiveModels { get; }

        /// <inheritdoc />
        public MainPage()
        {
            _controller = new MainPageController(this);
            RecentArchiveModels = new ObservableCollection<RecentArchiveModel>();
            InitializeComponent();

            if (Settings.IsMobileDevice)
            {
                // configure page elements for smaller screens
                Pivot.Margin = new Thickness(0);
                CompressButton.Margin = new Thickness(0, 32, 0, 0);
                StackPanelStart.Orientation = Orientation.Vertical;
                MenuSplitView.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                MenuSplitView.IsPaneOpen = false;
            }
            else
            {
                // set default launch size (will have no effect on phones)
                ApplicationView.PreferredLaunchViewSize = new Size(PreferredLaunchSizeWidth, PreferredLaunchSizeHeight);
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            }
        }

        private async void PopulateOrUpdateRecentArchivesList()
        {
            var collection = await ArchiveHistoryHandler.GetHistoryAsync();
            if (collection.Models.Length > 0)
            {
                RecentArchiveModels.Clear(); // important
                CultureInfo cultureInfo;

                try
                {
                    var culture = Windows.System.UserProfile
                        .GlobalizationPreferences.Languages[0];
                    cultureInfo = new CultureInfo(culture);
                }
                catch (IndexOutOfRangeException)
                {
                    // fall-back, although assertion error
                    cultureInfo = new CultureInfo("en-US");
                }

                foreach (var model in collection.Models)
                {
                    try
                    {
                        // format to culture specific date
                        var dateTime = DateTime.ParseExact(model.WhenUsed,
                            ArchiveHistoryHandler.DefaultDateFormat,
                            CultureInfo.InvariantCulture);
                        model.WhenUsed = dateTime.ToString(cultureInfo);
                    }
                    catch (FormatException)
                    {
                        // use date string unformatted
                    }
                    RecentArchiveModels.Add(model);
                }
                // only enable button to clear list if at least one item is present
                ClearListButton.IsEnabled = RecentArchiveModels.Count > 0;
            }
        }

        private void HamburgerButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            MenuSplitView.IsPaneOpen = !MenuSplitView.IsPaneOpen;
        }

        private async void CompressButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await _controller.PerformAction(MainPageActionType.Compress);
        }

        private async void ExtractButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await _controller.PerformAction(MainPageActionType.Decompress);
        }

        private async void OpenArchiveButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await _controller.PerformAction(MainPageActionType.OpenArchive);
        }

        private async void CalculateHashesButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await _controller.PerformAction(MainPageActionType.HashCalculation);
        }

        private async void GetSourceButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            var dialog = DialogFactory.CreateConfirmationDialog(
                I18N.Resources.GetString("OpenWebBrowserMessage/Text"),
                "\n" + I18N.Resources.GetString("Proceed/Text"));

            var result = await dialog.ShowAsync();
            if (result.Id.Equals(0)) // launch browser
            {
                await Launcher.LaunchUriAsync(new Uri("https://github.com/turbolocust/SimpleZIP"));
            }
        }

        private async void SettingsMenuButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await new Dialog.SettingsDialog().ShowAsync();
        }

        private async void AboutMenuButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await new Dialog.AboutDialog().ShowAsync();
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (sender is Pivot pivot)
            {
                // ReSharper disable once RedundantEmptySwitchSection
                switch (pivot.SelectedIndex)
                {
                    case 0: // StartPivot
                        {
                            CommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Hidden;
                        }
                        break;
                    case 1: // RecentPivot
                        {
                            PopulateOrUpdateRecentArchivesList();
                            CommandBar.ClosedDisplayMode = Settings.IsMobileDevice
                                ? AppBarClosedDisplayMode.Minimal
                                : AppBarClosedDisplayMode.Compact;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void ClearListButton_Tap(object sender, RoutedEventArgs args)
        {
            if (RecentArchiveModels.Count > 0)
            {
                RecentArchiveModels.Clear();
                ArchiveHistoryHandler.MruList.Clear();
                Settings.PushOrUpdate(Settings.Keys.RecentArchivesKey, string.Empty);
            }
        }

        private void CopyPathButton_Tap(object sender, RoutedEventArgs args)
        {
            if (RecentArchivesListView.SelectedItem is RecentArchiveModel model)
            {
                var package = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };

                string path = model.Location.EndsWith("\\")
                    ? model.Location
                    : model.Location + "\\";

                package.SetText(path + model.FileName);
                Clipboard.SetContent(package);
            }
        }

        private void RecentArchivesListView_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            CopyPathButton.IsEnabled = ((ListView)sender)?.SelectedItem != null;
        }

        private void RecentArchivesGrid_RightTapped(object sender, RightTappedRoutedEventArgs args)
        {
            if (args.PointerDeviceType == PointerDeviceType.Mouse
                && sender is FrameworkElement elem)
            {
                FlyoutBase.GetAttachedFlyout(elem).ShowAt(elem);
                args.Handled = true;
            }
        }

        private void RecentArchivesGrid_Holding(object sender, HoldingRoutedEventArgs args)
        {
            if (args.HoldingState == HoldingState.Started
                && sender is FrameworkElement elem)
            {
                FlyoutBase.GetAttachedFlyout(elem).ShowAt(elem);
                args.Handled = true;
            }
        }

        private async void LaunchFolderFlyoutItem_Tapped(object sender, RoutedEventArgs args)
        {
            if (sender is MenuFlyoutItem flyoutItem)
            {
                var model = (RecentArchiveModel)flyoutItem.DataContext;
                var mru = ArchiveHistoryHandler.MruList;
                if (!mru.ContainsItem(model.MruToken)) return;
                var folder = await mru.GetFolderAsync(model.MruToken);
                if (folder == null) return; // shouldn't be null
                var options = new FolderLauncherOptions();
                try
                {
                    var file = await folder.GetFileAsync(model.FileName);
                    options.ItemsToSelect.Add(file);
                }
                catch (FileNotFoundException)
                {
                    // file probably got deleted by someone;
                    // thus, ignore and just launch directory
                }
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Launcher.LaunchFolderAsync(folder, options);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private void RemoveFromHistoryFlyOutItem_Tapped(object sender, TappedRoutedEventArgs args)
        {
            if (sender is MenuFlyoutItem flyoutItem)
            {
                var model = (RecentArchiveModel)flyoutItem.DataContext;
                ArchiveHistoryHandler.RemoveFromHistory(model);
                RecentArchiveModels.Remove(model);
            }
        }

        /// <inheritdoc />
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            Frame.BackStack.Clear(); // going back is prohibited after aborting operation
        }
    }
}
