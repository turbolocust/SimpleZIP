﻿// ==++==
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
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Services.Store.Engagement;
using SimpleZIP_UI.Presentation;
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
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user. Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
#if DEBUG
            if (Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            // register notification channel to send push notifications to customers
            var engagementManager = StoreServicesEngagementManager.GetDefault();
            await engagementManager.RegisterNotificationChannelAsync();

            // do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

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

                // place the frame in the current Window
                Window.Current.Content = rootFrame;
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

        /// <summary>
        /// Invoked when application is in foreground.
        /// </summary>
        /// <param name="args">Consists of event parameters.</param>
        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            if (args is ToastNotificationActivatedEventArgs toastActivationArgs)
            {
                var engagementManager = StoreServicesEngagementManager.GetDefault();
                engagementManager.ParseArgumentsAndTrackAppLaunch(toastActivationArgs.Argument);
            }
        }

        /// <summary>
        /// Invoked when file has been opened via file explorer.
        /// </summary>
        /// <param name="args">Consists of event parameters.</param>
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
