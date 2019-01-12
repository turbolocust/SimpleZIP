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

using SimpleZIP_UI.Presentation.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    public sealed partial class NavigationViewRootPage
    {
        private const string TagHome = "Home";
        private const string TagOpenArchive = "OpenArchive";
        private const string TagHashCalculation = "HashCalculation";
        private const string TagProjectHome = "ProjectHome";
        private const string TagAbout = "About";

        /// <summary>
        /// Consists of value tuples for pages in <see cref="NavView"/>.
        /// </summary>
        private readonly IList<(string Tag, Type Page)> _pages;

        /// <summary>
        /// True if navigating back (via back stack). Used to detect
        /// cancellation of navigation and to avoid updating the selected
        /// menu item in case navigation got cancelled.
        /// </summary>
        private bool _isNavigateBack;

        /// <inheritdoc />
        public NavigationViewRootPage()
        {
            InitializeComponent();
            _pages = new List<(string Tag, Type Page)>
            {
                (TagHome, typeof(HomePage)),
                (TagOpenArchive, typeof(BrowseArchivePage)),
                (TagHashCalculation, typeof(MessageDigestPage))
            };
        }

        private static async void NavigateToProjectHome()
        {
            const string uriString = @"https://github.com/turbolocust/SimpleZIP";
            var dialog = DialogFactory.CreateConfirmationDialog(
                I18N.Resources.GetString("OpenWebBrowserMessage/Text"),
                "\n" + I18N.Resources.GetString("Proceed/Text"));

            var result = await dialog.ShowAsync();
            if (result.Id.Equals(0)) // launch browser
            {
                await Launcher.LaunchUriAsync(new Uri(uriString));
            }
        }

        private void UpdateSelectedMenuItem(Type destPageType)
        {
            var (tag, _) = _pages.FirstOrDefault(t => t.Page == destPageType);
            if (!string.IsNullOrEmpty(tag)) // might not be in list
            {
                var menuItem = NavView.MenuItems.OfType<NavigationViewItem>()
                    .FirstOrDefault(item => item.Tag.Equals(tag));
                if (menuItem != null) NavView.SelectedItem = menuItem;
            }
        }

        private void ContentFrameNavigate(Type destPageType, object param = null)
        {
            var curPage = ContentFrame.CurrentSourcePageType;
            if (destPageType != null && curPage != destPageType)
            {
                ContentFrame.Navigate(destPageType, param);
            }
        }

        private static async void ShowAboutViewDialog()
        {
            await new Dialog.AboutDialog().ShowAsync();
        }

        private async Task OpenArchiveAction()
        {
            var picker = PickerFactory.FileOpenPickerForArchives;
            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                ContentFrameNavigate(typeof(BrowseArchivePage), file);
            }
        }

        private async Task CalculateHashAction()
        {
            var picker = PickerFactory.FileOpenPickerForAnyFile;
            var files = await picker.PickMultipleFilesAsync();

            if (files?.Count > 0)
            {
                var args = new FilesNavigationArgs(files);
                ContentFrameNavigate(typeof(MessageDigestPage), args);
            }
        }

        private async void NavView_OnItemInvoked(NavigationView sender,
            NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrameNavigate(typeof(SettingsPage));
            }
            else if (args.InvokedItem is FrameworkElement elem &&
                     elem.Parent is NavigationViewItem item)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (item.Tag)
                {
                    case TagHome:
                        ContentFrameNavigate(typeof(HomePage));
                        break;
                    case TagOpenArchive:
                        await OpenArchiveAction();
                        break;
                    case TagHashCalculation:
                        await CalculateHashAction();
                        break;
                    case TagProjectHome:
                        NavigateToProjectHome();
                        break;
                    case TagAbout:
                        ShowAboutViewDialog();
                        break;
                }
            }
        }

        private void ContentFrame_OnNavigated(object sender, NavigationEventArgs args)
        {
            if (_isNavigateBack)
            {
                _isNavigateBack = false;
                UpdateSelectedMenuItem(args.SourcePageType);
            }
        }

        private void ContentFrame_OnNavigationStopped(object sender, NavigationEventArgs args)
        {
            _isNavigateBack = false;
        }

        private void NavigationView_OnBackRequested(NavigationView sender,
            NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                _isNavigateBack = true;
                ContentFrame.GoBack();
            }
        }

        /// <inheritdoc />
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            if (args.Parameter is RootPageNavigationArgs rootArgs)
            {
                var type = rootArgs.Content ?? typeof(HomePage);
                ContentFrameNavigate(type);
            }
            else if (args.Parameter is FileActivatedEventArgs)
            {
                Type destination;
                // check settings if archive should be opened for browsing
                if (Settings.TryGet(Settings.Keys.PreferOpenArchiveKey,
                        out bool isOpenArchive) && isOpenArchive)
                {
                    destination = typeof(BrowseArchivePage);
                }
                else
                {
                    destination = typeof(DecompressionSummaryPage);
                }

                ContentFrameNavigate(destination, args.Parameter);
            }
            else
            {
                ContentFrameNavigate(typeof(HomePage));
            }
        }
    }
}
