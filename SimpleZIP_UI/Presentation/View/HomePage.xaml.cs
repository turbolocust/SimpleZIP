﻿// ==++==
// 
// Copyright (C) 2020 Matthias Fussenegger
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

using SimpleZIP_UI.Presentation.Factory;
using SimpleZIP_UI.Presentation.Handler;
using SimpleZIP_UI.Presentation.View.Model;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
using Windows.System;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Serilog;
using SimpleZIP_UI.Presentation.Util;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    public sealed partial class HomePage : INavigable
    {
        private readonly ILogger _logger = Log.ForContext<HomePage>();

        /// <summary>
        /// Models bound to the list view.
        /// </summary>
        public ObservableCollection<RecentArchiveModel> RecentArchiveModels { get; }

        /// <inheritdoc />
        public HomePage()
        {
            RecentArchiveModels = new ObservableCollection<RecentArchiveModel>();
            InitializeComponent();

            if (EnvironmentInfo.IsMobileDevice)
            {
                PivotMain.Margin = new Thickness(0, 28, 0, 0);
                CompressButton.Margin = new Thickness(0, 32, 0, 0);
            }

            if (EnvironmentInfo.IsDarkThemeEnabled)
            {
                TitleText.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private async void PopulateOrUpdateRecentArchivesList()
        {
            var collection = await ArchiveHistory.Instance.GetHistoryAsync();

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
                catch (IndexOutOfRangeException ex)
                {
                    _logger.Error(ex, "No language detected");
                    // fall-back, although assertion error
                    cultureInfo = new CultureInfo("en-US");
                }

                foreach (var model in collection.Models)
                {
                    var colorBrush = this.DetermineSystemAccentColorBrush();
                    model.FileNameColorBrush = new SolidColorBrushModel(colorBrush);

                    try
                    {
                        // format to culture specific date
                        var dateTime = DateTime.ParseExact(model.WhenUsed,
                            ArchiveHistory.DefaultDateFormat,
                            CultureInfo.InvariantCulture);
                        model.WhenUsed = dateTime.ToString(cultureInfo);
                    }
                    catch (FormatException ex)
                    {
                        _logger.Error(ex, "Date formatting failed for {ModelName} ({FileName})", model.MruToken, model.FileName);
                        // use date string unformatted
                    }

                    RecentArchiveModels.Add(model);
                }
                // only enable button to clear list if at least one item is present
                ClearListButton.IsEnabled = RecentArchiveModels.Count > 0;
            }
        }

        private async Task RevealInExplorer(RecentArchiveModel model)
        {
            var historyManager = ArchiveHistory.Instance;
            if (!historyManager.ContainsItem(model.MruToken)) return;

            try
            {
                var folder = await historyManager.GetFolderAsync(model.MruToken);
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
            catch (FileNotFoundException ex)
            {
                _logger.Error(ex, "File {FileName} could not be revealed", model.FileName);
                string errMsg = I18N.Resources.GetString("FolderMissing/Text");
                await DialogFactory.CreateErrorDialog(errMsg).ShowAsync();
            }
        }

        private async void CompressButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            var picker = PickerFactory.FileOpenPickerForAnyFile;
            var files = await picker.PickMultipleFilesAsync();

            if (files?.Count > 0)
            {
                var navArgs = new FilesNavigationArgs(files);
                Frame.Navigate(typeof(CompressionSummaryPage), navArgs);
            }
        }

        private async void ExtractButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            var picker = PickerFactory.FileOpenPickerForArchives;
            var files = await picker.PickMultipleFilesAsync();

            if (files?.Count > 0)
            {
                var navArgs = new FilesNavigationArgs(files);
                Frame.Navigate(typeof(DecompressionSummaryPage), navArgs);
            }
        }

        private void Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (sender is Pivot pivot)
            {
                if (pivot.SelectedIndex == 0) // Start pivot
                {
                    CommandBar.ClosedDisplayMode = AppBarClosedDisplayMode.Hidden;
                }
                else if (pivot.SelectedIndex == 1) // Recent pivot
                {
                    PopulateOrUpdateRecentArchivesList();
                    CommandBar.ClosedDisplayMode = EnvironmentInfo.IsMobileDevice
                        ? AppBarClosedDisplayMode.Minimal
                        : AppBarClosedDisplayMode.Compact;
                }
            }
        }

        private void ClearListButton_OnTapped(object sender, RoutedEventArgs args)
        {
            if (RecentArchiveModels.Count > 0)
            {
                RecentArchiveModels.Clear();
                ArchiveHistory.Instance.ClearHistory();
            }
        }

        private void CopyPathButton_OnTapped(object sender, RoutedEventArgs args)
        {
            if (RecentArchivesListView.SelectedItem is RecentArchiveModel model)
            {
                var package = new DataPackage
                {
                    RequestedOperation = DataPackageOperation.Copy
                };

                string path = model.Location.EndsWith("\\", StringComparison.Ordinal)
                    ? model.Location
                    : model.Location + "\\";

                package.SetText(path + model.FileName);
                Clipboard.SetContent(package);
            }
        }

        private void RecentArchivesListView_OnSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            CopyPathButton.IsEnabled = ((ListView)sender)?.SelectedItem != null;
        }

        private void RecentArchivesGrid_OnRightTapped(object sender, RightTappedRoutedEventArgs args)
        {
            if (args.PointerDeviceType == PointerDeviceType.Mouse
                && sender is FrameworkElement elem)
            {
                FlyoutBase.GetAttachedFlyout(elem).ShowAt(elem);
                args.Handled = true;
            }
        }

        private void RecentArchivesGrid_OnHolding(object sender, HoldingRoutedEventArgs args)
        {
            if (args.HoldingState == HoldingState.Started
                && sender is FrameworkElement elem)
            {
                FlyoutBase.GetAttachedFlyout(elem).ShowAt(elem);
                args.Handled = true;
            }
        }

        private async void LaunchFolderFlyoutItem_OnTapped(object sender, RoutedEventArgs args)
        {
            if (sender is MenuFlyoutItem flyoutItem)
            {
                var model = (RecentArchiveModel)flyoutItem.DataContext;
                await RevealInExplorer(model).ConfigureAwait(false);
            }
        }

        private async void LaunchFolderFlyoutItem_OnClick(object sender, RoutedEventArgs args)
        {
            if (sender is MenuFlyoutItem flyoutItem)
            {
                var model = (RecentArchiveModel)flyoutItem.DataContext;
                await RevealInExplorer(model).ConfigureAwait(false);
            }
        }

        private void RemoveFromHistoryFlyOutItem_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            if (sender is MenuFlyoutItem flyoutItem)
            {
                var model = (RecentArchiveModel)flyoutItem.DataContext;
                ArchiveHistory.Instance.RemoveFromHistory(model);
                RecentArchiveModels.Remove(model);
            }
        }

        private void RemoveFromHistoryFlyOutItem_OnClick(object sender, RoutedEventArgs args)
        {
            if (sender is MenuFlyoutItem flyoutItem)
            {
                var model = (RecentArchiveModel)flyoutItem.DataContext;
                ArchiveHistory.Instance.RemoveFromHistory(model);
                RecentArchiveModels.Remove(model);
            }
        }

        /// <inheritdoc />
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            if (args != null)
            {
                if (args.Parameter is PageNavigationArgs navArgs)
                {
                    if (navArgs.PageType != typeof(NavigationViewRootPage))
                    {
                        Frame.BackStack.Clear(); // going back is prohibited e.g. after aborting operation
                    }
                }

                NavigationLock.Instance.IsLocked = false;
            }
        }

        /// <inheritdoc />
        public void Navigate(Type destinationPageType, object parameter = null)
        {
            if (parameter == null)
            {
                var args = new PageNavigationArgs(typeof(HomePage));
                Frame.Navigate(destinationPageType, args);
            }
            else
            {
                Frame.Navigate(destinationPageType, parameter);
            }
        }
    }
}
