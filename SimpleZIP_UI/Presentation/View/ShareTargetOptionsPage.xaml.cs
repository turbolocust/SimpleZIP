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
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    /// <summary>
    /// Page which exists to let user choose the operation on a share event.
    /// </summary>
    public sealed partial class ShareTargetOptionsPage : INavigation
    {
        private FilesNavigationArgs _filesNavigationArgs;

        public ShareTargetOptionsPage()
        {
            InitializeComponent();

            if (EnvironmentInfo.IsDarkThemeEnabled)
            {
                TitleText.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void CompressFilesButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            Frame.Navigate(typeof(CompressionSummaryPage), _filesNavigationArgs);
            args.Handled = true;
        }

        private void ExtractButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            Frame.Navigate(typeof(DecompressionSummaryPage), _filesNavigationArgs);
            args.Handled = true;
        }

        private void ComputeHashes_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            Frame.Navigate(typeof(MessageDigestPage), _filesNavigationArgs);
            args.Handled = true;
        }

        /// <inheritdoc />
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            _filesNavigationArgs = args.Parameter as FilesNavigationArgs;

            if (_filesNavigationArgs != null && _filesNavigationArgs.IsArchivesOnly)
            {
                ExtractButton.Visibility = Visibility.Visible;
            }
        }

        /// <inheritdoc />
        public void Navigate(Type destinationPageType, object parameter = null)
        {
            if (parameter == null)
            {
                var args = new PageNavigationArgs(typeof(ShareTargetOptionsPage));
                Frame.Navigate(destinationPageType, args);
            }
            else
            {
                Frame.Navigate(destinationPageType, parameter);
            }
        }
    }
}
