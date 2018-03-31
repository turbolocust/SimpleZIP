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
using Windows.System;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Presentation.Factory;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                StackPanelStart.Orientation = Orientation.Vertical;
                // configure split view for smaller screens
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

        private void PopulateOrUpdateRecentArchivesList()
        {
            var collection = RecentArchivesHistoryHandler.GetHistory();
            if (collection.Models.Length > 0)
            {
                RecentArchiveModels.Clear(); // important

                var culture = Windows.System.UserProfile.GlobalizationPreferences.Languages[0];
                var cultureInfo = new CultureInfo(culture);

                foreach (var model in collection.Models)
                {
                    try
                    {
                        // format to culture specific date
                        var dateTime = DateTime.ParseExact(model.WhenUsed,
                            RecentArchivesHistoryHandler.DefaultDateFormat,
                            CultureInfo.InvariantCulture);
                        model.WhenUsed = dateTime.ToString(cultureInfo);
                    }
                    catch (FormatException)
                    {
                        // use date string unformatted
                    }
                    RecentArchiveModels.Add(model);
                }
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
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (pivot.SelectedIndex)
                {
                    case 0: // StartPivot
                        ClearListButton.IsEnabled = false;
                        ClearListButton.Visibility = Visibility.Collapsed;
                        break;
                    case 1: // RecentPivot
                        PopulateOrUpdateRecentArchivesList();
                        ClearListButton.IsEnabled = RecentArchiveModels.Count > 0;
                        ClearListButton.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        /// <summary>
        /// Clears the history of recently created archives.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void ClearListButton_Click(object sender, RoutedEventArgs args)
        {
            RecentArchiveModels.Clear();
            Settings.PushOrUpdate(Settings.Keys.RecentArchivesKey, string.Empty);
        }

        /// <summary>
        /// Reveals the selected file in the file explorer.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private async void RevealInExplorerButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            if (!(args.OriginalSource is FrameworkElement element)) return;
            if (element.DataContext is RecentArchiveModel model)
            {
                try
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(model.Location);
                    if (!await Launcher.LaunchFolderAsync(folder))
                    {
                        var title = I18N.Resources.GetString("Error/Text");
                        var message = I18N.Resources.GetString("ErrorOpeningExplorer/Text");
                        var dialog = DialogFactory.CreateConfirmationDialog(title, message);
                        var result = await dialog.ShowAsync();
                        if (result.Id.Equals(0)) // user has confirmed
                        {
                            RecentArchiveModels.Remove(model);
                            RecentArchivesHistoryHandler.RemoveFromHistory(model);
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    var errorMessage = I18N.Resources.GetString("ErrorNoAuthorization/Text");
                    var errorDialog = DialogFactory.CreateErrorDialog(errorMessage);
                    await errorDialog.ShowAsync();
                }
                args.Handled = true;
            }
        }

        /// <inheritdoc />
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            Frame.BackStack.Clear(); // going back is prohibited after aborting operation
        }
    }
}
