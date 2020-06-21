// ==++==
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

using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;
using Serilog;
using SimpleZIP_UI.Presentation.Cache;
using SimpleZIP_UI.Presentation.View.Model;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    public sealed partial class NavigationViewRootPage
    {
        #region Tag names for each NavigationViewItem in NavigationView

        private const string TagHome = "Home";
        private const string TagOpenArchive = "OpenArchive";
        private const string TagHashCalculation = "HashCalculation";
        private const string TagAbout = "About";

        #endregion

        #region Message templates

        private const string NavigateToPageMessageTemplate = "Navigating to {DestinationPageType}";
        private const string NavigationFailedMessageTemplate = "Navigation to {DestinationPageType} failed";

        #endregion

        private const int DefaultControlsFontSize = 16;

        private readonly ILogger _logger = Log.ForContext<NavigationViewRootPage>();

        /// <summary>
        /// Consists of value tuples for defined pages in <see cref="NavView"/>.
        /// </summary>
        private readonly IList<(string Tag, Type Page)> _pages;

        /// <summary>
        /// True if navigating back (via back stack). Used to detect
        /// cancellation of navigation and to avoid updating the selected
        /// menu item in case the navigation process got cancelled.
        /// </summary>
        private volatile bool _isNavigateBack;

        /// <summary>
        /// Specifies the default font size of texts of controls.
        /// </summary>
        public IntegerModel FontSizeViewItem { get; set; } = DefaultControlsFontSize;

        /// <inheritdoc />
        public NavigationViewRootPage()
        {
            InitializeComponent();
            NavigationLock.Instance.IsLocked = false;
            RestoreStateOfNavigationViewPane();

            // initialize list consisting of tuples for pages
            // each tuple holds the tag and type of a page
            _pages = new List<(string Tag, Type Page)>
            {
                (TagHome, typeof(HomePage)),
                (TagOpenArchive, typeof(BrowseArchivePage)),
                (TagHashCalculation, typeof(MessageDigestPage)),
                (TagAbout, typeof(AboutPage))
            };
        }

        private void RestoreStateOfNavigationViewPane()
        {
            const string settingsKey = Settings.Keys.IsNavigationViewPaneOpen;
            if (Settings.TryGet(settingsKey, out bool isNavigationViewPaneOpen))
            {
                NavView.IsPaneOpen = isNavigationViewPaneOpen;
            }
        }

        private void UpdateSelectedMenuItem(Type destPageType)
        {
            var (tag, _) = _pages.FirstOrDefault(t => t.Page == destPageType);
            if (!string.IsNullOrEmpty(tag)) // page might not be in list
            {
                // page was found, now find the item with matching tag
                var menuItem = NavView.MenuItems.OfType<NavigationViewItem>()
                    .FirstOrDefault(item => item.Tag.Equals(tag));
                if (menuItem != null) NavView.SelectedItem = menuItem;
            }
        }

        /// <summary>
        /// Loads the specified page within <see cref="ContentFrame"/>.
        /// </summary>
        /// <param name="destPageType">Type of page to be loaded.</param>
        /// <param name="param">Optional parameter to be passed to page.</param>
        private void ContentFrameNavigate(Type destPageType, object param = null)
        {
            var curPage = ContentFrame.CurrentSourcePageType;
            if (destPageType != null && curPage != destPageType)
            {
                _logger.Debug(NavigateToPageMessageTemplate, destPageType);

                if (!ContentFrame.Navigate(destPageType, param))
                {
                    _logger.Debug(NavigationFailedMessageTemplate, destPageType);
                }
            }
        }

        private async Task OpenArchiveAction()
        {
            var picker = PickerFactory.FileOpenPickerForArchives;
            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                var destPage = typeof(BrowseArchivePage);
                _logger.Debug(NavigateToPageMessageTemplate, destPage);

                if (!ContentFrame.Navigate(destPage, file))
                {
                    _logger.Debug(NavigationFailedMessageTemplate, destPage);
                }
            }
        }

        private async Task CalculateHashAction()
        {
            var picker = PickerFactory.FileOpenPickerForAnyFile;
            var files = await picker.PickMultipleFilesAsync();

            if (files?.Count > 0)
            {
                var args = new FilesNavigationArgs(files);
                var destPage = typeof(MessageDigestPage);
                _logger.Debug(NavigateToPageMessageTemplate, destPage);

                if (!ContentFrame.Navigate(destPage, args))
                {
                    _logger.Debug(NavigationFailedMessageTemplate, destPage);
                }
            }
        }

        #region Events

        private void ContentFrame_OnNavigated(object sender, NavigationEventArgs args)
        {
            if (_isNavigateBack)
            {
                _isNavigateBack = false;
                UpdateSelectedMenuItem(args.SourcePageType);
            }
            // try initialize cache if back stack is empty
            if (ContentFrame.BackStack.IsNullOrEmpty())
            {
                RootNodeCache.CheckInitialize();
            }
        }

        private void ContentFrame_OnNavigationStopped(object sender, NavigationEventArgs args)
        {
            _isNavigateBack = false;
        }

        private async void NavigationView_OnItemInvoked(NavigationView sender,
            NavigationViewItemInvokedEventArgs args)
        {
            if (!NavigationLock.Instance.IsLocked)
            {
                if (args.IsSettingsInvoked)
                {
                    // opening a content dialog here causes an exception
                    ContentFrameNavigate(typeof(SettingsPage));
                }
                // invoked item is TextBlock (content of NavigationViewItem)
                else if (args.InvokedItem is FrameworkElement elem &&
                         elem.Parent is NavigationViewItem item)
                {
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
                        case TagAbout:
                            ContentFrameNavigate(typeof(AboutPage));
                            break;
                    }
                }
            }
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

        private void NavigationView_OnLoaded(object sender, RoutedEventArgs args)
        {
            if (sender is NavigationView navView &&
                navView.SettingsItem is NavigationViewItem navViewItem)
            {
                var binding = new Binding
                {
                    Source = FontSizeViewItem,
                    Path = new PropertyPath("Value"),
                    Mode = BindingMode.OneWay
                };

                navViewItem.SetBinding(FontSizeProperty, binding);
            }
        }

        private void NavigationView_OnPaneOpened(NavigationView sender, object args)
        {
            if (!sender.Equals(NavView)) return;
            Settings.PushOrUpdate(Settings.Keys.IsNavigationViewPaneOpen, true);
        }

        private void NavigationView_OnPaneClosed(NavigationView sender, object args)
        {
            if (!sender.Equals(NavView)) return;
            Settings.PushOrUpdate(Settings.Keys.IsNavigationViewPaneOpen, false);
        }

        /// <inheritdoc />
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            if (args == null) return;

            // arguments may hold a specified page type
            if (args.Parameter is PageNavigationArgs navArgs)
            {
                var type = navArgs.PageType ?? typeof(HomePage);
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

        #endregion
    }
}
