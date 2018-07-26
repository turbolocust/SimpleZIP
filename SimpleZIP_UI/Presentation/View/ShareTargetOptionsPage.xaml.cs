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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    /// <summary>
    /// Page which exists to let user choose the operation on a share event.
    /// </summary>
    public sealed partial class ShareTargetOptionsPage
    {
        private NavigationArgs _navigationArgs;

        public ShareTargetOptionsPage()
        {
            InitializeComponent();
        }

        private void CompressFilesButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            Frame.Navigate(typeof(CompressionSummaryPage), _navigationArgs);
            args.Handled = true;
        }

        private void ExtractButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            Frame.Navigate(typeof(DecompressionSummaryPage), _navigationArgs);
            args.Handled = true;
        }

        private void ComputeHashes_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            Frame.Navigate(typeof(MessageDigestPage), _navigationArgs);
            args.Handled = true;
        }

        /// <inheritdoc />
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            _navigationArgs = args.Parameter as NavigationArgs;
            if (_navigationArgs != null && _navigationArgs.IsArchivesOnly)
            {
                ExtractButton.Visibility = Visibility.Visible;
            }
        }
    }
}
