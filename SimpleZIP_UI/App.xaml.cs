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
#if DEBUG
using System.Diagnostics;
#endif
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Application;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Handler;
using SimpleZIP_UI.Presentation.View;

namespace SimpleZIP_UI
{
    /// <inheritdoc cref="Application" />
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            RequestApplicationTheme();
            InitializeComponent();
            InitializeTempDir();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user. Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            // do not repeat app initialization when the Window already has content
            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame(); // create frame to act as navigation context and navigate to the first page

                rootFrame.NavigationFailed += OnNavigationFailed;
                rootFrame.Navigated += OnNavigated;

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // nothing to load
                }

                // handler for back request
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    rootFrame.CanGoBack ?
                    AppViewBackButtonVisibility.Visible :
                    AppViewBackButtonVisibility.Collapsed;

                Window.Current.Content = rootFrame; // place the frame in the current Window
            }

            if (args.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // navigate to the first page, if the navigation stack isn't restored
                    rootFrame.Navigate(typeof(MainPage), args.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <inheritdoc />
        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            base.OnFileActivated(args);
            var rootFrame = new Frame();
            var destination = Settings.TryGet(Settings.Keys.PreferOpenArchiveKey,
                out bool isOpenArchive) && isOpenArchive
                    ? typeof(BrowseArchivePage)
                    : typeof(DecompressionSummaryPage);
            rootFrame.Navigate(destination, args);
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
        }

        /// <inheritdoc />
        protected override async void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            base.OnShareTargetActivated(args);
            var shareOperation = args.ShareOperation;
            // only StorageItems are supported, hence return if none are present
            if (!shareOperation.Data.Contains(StandardDataFormats.StorageItems)) return;

            try
            {
                var handler = new ShareTargetHandler();
                await handler.Handle(shareOperation);
            }
            catch (Exception)
            {
                if (DeviceInfo.IsMinCreatorsUpdate)
                {
                    // does not seem to work properly, even though
                    // it is no longer deprecated since 1703
                    //shareOperation.ReportError(ex.Message);
                    shareOperation.ReportCompleted();
                }
                else
                {
                    shareOperation.ReportCompleted();
                }
            }
        }

        /// <summary>
        /// Requests the application theme based on the user setting. If the
        /// system's default theme is set, then no theme will be requested.
        /// </summary>
        private void RequestApplicationTheme()
        {
            Settings.TryGet(Settings.Keys.ApplicationThemeKey, out string theme);
            if (theme != null)
            {
                if (theme.Equals(ApplicationTheme.Light.ToString()))
                {
                    RequestedTheme = ApplicationTheme.Light;
                }
                else if (theme.Equals(ApplicationTheme.Dark.ToString()))
                {
                    RequestedTheme = ApplicationTheme.Dark;
                }
            }
        }

        private static async void InitializeTempDir()
        {
            var folder = await FileUtils
                .GetTempFolderAsync(TempFolder.Archives);
            await FileUtils.CleanFolderAsync(folder);
        }

        /// <summary>
        /// Each time a navigation event occurs, update the Back button's visibility.
        /// </summary>
        /// <param name="sender">The sender of the request.</param>
        /// <param name="args">Consists of event parameters.</param>
        private static void OnNavigated(object sender, NavigationEventArgs args)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ?
                AppViewBackButtonVisibility.Visible :
                AppViewBackButtonVisibility.Collapsed;
        }

        /// <summary>
        /// Invoked when navigation to a certain page fails.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private static void OnNavigationFailed(object sender, NavigationFailedEventArgs args)
        {
            throw new Exception("Failed to load Page " + args.SourcePageType.FullName);
        }

        /// <summary>
        /// Navigates back to the previous frame.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private static void OnBackRequested(object sender, BackRequestedEventArgs args)
        {
            if (Window.Current.Content is Frame frame && frame.CanGoBack)
            {
                args.Handled = true;
                frame.GoBack();
            }
        }

        /// <summary>
        /// Invoked when application execution is being suspended. Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private static void OnSuspending(object sender, SuspendingEventArgs args)
        {
            var deferral = args.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
