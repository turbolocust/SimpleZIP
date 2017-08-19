// ==++==
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
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Presentation.Factory;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using SimpleZIP_UI.Presentation.Controller;

namespace SimpleZIP_UI.Presentation.View
{
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Constant which defines the preferred width of the view.
        /// </summary>
        private const double PreferredLaunchSizeWidth = 400d;

        /// <summary>
        /// Constant which defines the preferred height of the view.
        /// </summary>
        private const double PreferredLaunchSizeHeight = 600d;

        /// <summary>
        /// The aggregated control instance.
        /// </summary>
        private readonly MainPageController _controller;

        public MainPage()
        {
            InitializeComponent();
            _controller = new MainPageController(this);
            // set default launch size (will have no effect on phones)
            ApplicationView.PreferredLaunchViewSize = new Size(PreferredLaunchSizeWidth, PreferredLaunchSizeHeight);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void HamburgerButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            MenuSplitView.IsPaneOpen = !MenuSplitView.IsPaneOpen;
        }

        private async void CompressButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            await _controller.CompressButtonAction();
        }

        private async void ExtractButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            await _controller.DecompressButtonAction();
        }

        /// <summary>
        /// Opens an archive for exploring.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private async void OpenArchiveButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await _controller.OpenArchiveButtonAction();
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
        /// Shows a dialog with information about the application.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private async void AboutMenuButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await new AboutDialog().ShowAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            Frame.BackStack.Clear(); // going back is prohibited after aborting operation
        }
    }
}
