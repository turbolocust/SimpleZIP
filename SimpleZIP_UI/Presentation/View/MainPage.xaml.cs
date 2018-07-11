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

        /// <summary>
        /// Handles the event for the hamburger button (of split view).
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void HamburgerButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            MenuSplitView.IsPaneOpen = !MenuSplitView.IsPaneOpen;
        }

        /// <summary>
        /// Allows the user to select files for compression.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private async void CompressButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await _controller.PerformAction(MainPageActionType.Compress);
        }

        /// <summary>
        /// Allows the user to select archives for decompression.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private async void ExtractButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await _controller.PerformAction(MainPageActionType.Decompress);
        }

        /// <summary>
        /// Allows the user to open an archive for exploring.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private async void OpenArchiveButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await _controller.PerformAction(MainPageActionType.OpenArchive);
        }

        /// <summary>
        /// Allows the user to select files whose hashes are to be calculated.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private async void CalculateHashesButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await _controller.PerformAction(MainPageActionType.HashCalculation);
        }

        /// <summary>
        /// Opens the project's homepage using the <see cref="Launcher"/>.
        /// Brings up a confirmation dialog first to avoid accidental redirection. 
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
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

        /// <summary>
        /// Shows a settings dialog.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private async void SettingsMenuButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await new Dialog.SettingsDialog().ShowAsync();
        }

        /// <summary>
        /// Shows a dialog with information about the application.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private async void AboutMenuButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await new Dialog.AboutDialog().ShowAsync();
        }

        /// <summary>
        /// Reacts to the selection change of pivot items.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
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

        /// <summary>
        /// Clears the history of recently created archives.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void ClearListButton_Tap(object sender, RoutedEventArgs args)
        {
            if (RecentArchiveModels.Count > 0)
            {
                RecentArchiveModels.Clear();
                ArchiveHistoryHandler.MruList.Clear();
                Settings.PushOrUpdate(Settings.Keys.RecentArchivesKey, string.Empty);
            }
        }

        /// <summary>
        /// Copies the path of the selected file to the clipboard.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
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

        /// <summary>
        /// Handles the selection change of items in the list view consisting of recently created archives.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void RecentArchivesListView_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            CopyPathButton.IsEnabled = ((ListView)sender)?.SelectedItem != null;
        }

        /// <summary>
        /// Handles a right tap on an item in the list consisting of most recently created archives.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void RecentArchivesGrid_RightTapped(object sender, RightTappedRoutedEventArgs args)
        {
            if (args.PointerDeviceType == PointerDeviceType.Mouse
                && sender is FrameworkElement elem)
            {
                FlyoutBase.GetAttachedFlyout(elem).ShowAt(elem);
                args.Handled = true;
            }
        }

        /// <summary>
        /// Handles a holding event on an item in the list consisting of most recently created archives.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void RecentArchivesGrid_Holding(object sender, HoldingRoutedEventArgs args)
        {
            if (args.HoldingState == HoldingState.Started
                && sender is FrameworkElement elem)
            {
                FlyoutBase.GetAttachedFlyout(elem).ShowAt(elem);
                args.Handled = true;
            }
        }

        /// <summary>
        /// Handles a tap on the button in flyout which reveals the selected item in the file explorer.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private async void LaunchFolderButton_Tapped(object sender, RoutedEventArgs args)
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

        /// <inheritdoc />
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            Frame.BackStack.Clear(); // going back is prohibited after aborting operation
        }
    }
}
