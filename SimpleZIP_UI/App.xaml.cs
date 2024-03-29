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

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security;
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
using Serilog;
using SimpleZIP_UI.Business;
using SimpleZIP_UI.Business.Util;
using SimpleZIP_UI.Presentation.Handler;
using SimpleZIP_UI.Presentation.View;

[assembly: InternalsVisibleTo("SimpleZIP_UI_TEST")]

namespace SimpleZIP_UI
{
    /// <inheritdoc cref="Business" />
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        /// The log message template to be used by Serilog, which adds <c>Properties</c>.
        /// </summary>
        private const string LogMessageTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}";

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
            SetUpApplicationLogging();
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

            //Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "de";
#endif

            // do not repeat app initialization when the Window already has content
            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame(); // create frame to act as navigation context and navigate to the first page

                rootFrame.NavigationFailed += OnNavigationFailed;
                rootFrame.Navigated += OnNavigated;

                // handler for back request
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    rootFrame.CanGoBack ?
                    AppViewBackButtonVisibility.Visible :
                    AppViewBackButtonVisibility.Collapsed;

                Window.Current.Content = rootFrame; // place the frame in the current Window
            }

            if (args != null && args.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // navigate to the first page, if the navigation stack isn't restored
                    rootFrame.Navigate(typeof(NavigationViewRootPage), args.Arguments);
                }

                Window.Current.Activate(); // ensure the current window is active
            }
        }

        /// <inheritdoc />
        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            base.OnFileActivated(args);
            var rootFrame = new Frame();
            var destPage = typeof(NavigationViewRootPage);
            rootFrame.Navigate(destPage, args);
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
        }

        /// <inheritdoc />
        protected override async void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            if (args == null) return;

            base.OnShareTargetActivated(args);
            var shareOperation = args.ShareOperation;
            // only StorageItems are supported, hence return if none are present
            if (!shareOperation.Data.Contains(StandardDataFormats.StorageItems)) return;

            try
            {
                await ShareTargetHandler.Handle(shareOperation);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error handling shared file(s)");
                shareOperation.ReportCompleted();
            }
        }

        #region Private methods

        private void RequestApplicationTheme()
        {
            if (Settings.TryGetTheme(out var theme))
            {
                RequestedTheme = theme;
            }
        }

        private static void SetUpApplicationLogging()
        {
            if (EnvironmentInfo.IsMobileDevice) return;

            var loggerConfiguration = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
                .WriteTo.Debug()
#else
                .MinimumLevel.Information()
#endif
                .Enrich.FromLogContext();

            try
            {
                string tempFolderPath = Path.GetTempPath();
                string logFilePath = Path.Combine(tempFolderPath, "app.log");

                loggerConfiguration.WriteTo.Async(asyncConfig => asyncConfig
                    .File(logFilePath, rollingInterval: RollingInterval.Day, outputTemplate: LogMessageTemplate));
            }
            catch (SecurityException)
            {
                // ignore, logging to file is disabled as a result
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        private static async void InitializeTempDir()
        {
            var folder = await FileUtils.GetTempFolderAsync(TempFolder.Archives).ConfigureAwait(false);
            await FileUtils.CleanFolderAsync(folder).ConfigureAwait(false);
        }

        /// <summary>
        /// Each time a navigation event occurs, update the Back button's visibility.
        /// </summary>
        /// <param name="sender">The sender of the request.</param>
        /// <param name="args">Consists of event parameters.</param>
        private static void OnNavigated(object sender, NavigationEventArgs args)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                ((Frame)sender).CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        /// <summary>
        /// Invoked when navigation to a certain page fails.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private static void OnNavigationFailed(object sender, NavigationFailedEventArgs args)
        {
            Log.Logger.Error(args.Exception, "Failed to load Page {PageName}", args.SourcePageType.FullName);
            throw new Exception($"Failed to load Page {args.SourcePageType.FullName}");
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
            Log.CloseAndFlush();
            var deferral = args.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        #endregion
    }
}
