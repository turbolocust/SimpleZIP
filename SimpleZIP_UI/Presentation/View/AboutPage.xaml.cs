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

using SimpleZIP_UI.Presentation.Factory;
using System;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    public sealed partial class AboutPage
    {
        private const string Author = "Matthias Fussenegger";
        private const string License = "GNU General Public License 3";

        private const string LicenseUri = @"https://www.gnu.org/licenses/gpl-3.0";
        private const string LibraryScUri = @"https://github.com/adamhathcock/sharpcompress";
        private const string LibrarySzlUri = @"https://github.com/icsharpcode/SharpZipLib";

        private const string LogoUriString = @"ms-appx:///Assets/Logo/simplezip_logo.png";
        private const string LogoInvertedUriString = @"ms-appx:///Assets/Logo/simplezip_logo_inverted.png";

        public AboutPage()
        {
            InitializeComponent();

            DevelopedByRun.Text = I18N.Resources.GetString("DevelopedBy/Text", Author);
            LicenseRun.Text = I18N.Resources.GetString("License/Text", License);

            LibraryScHyperlinkButton.NavigateUri = new Uri(LibraryScUri);
            LibrarySzlHyperlinkButton.NavigateUri = new Uri(LibrarySzlUri);
            LicenseHyperlinkButton.NavigateUri = new Uri(LicenseUri);

            LogoImage.Source = EnvironmentInfo.IsDarkThemeEnabled
                ? new BitmapImage(new Uri(LogoInvertedUriString)) { DecodePixelHeight = 200 }
                : new BitmapImage(new Uri(LogoUriString)) { DecodePixelHeight = 200 };
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

        private void GetSourceButton_OnTapped(object sender, TappedRoutedEventArgs args)
        {
            NavigateToProjectHome();
        }
    }
}
