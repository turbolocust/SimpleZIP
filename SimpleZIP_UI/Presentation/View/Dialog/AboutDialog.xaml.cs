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

using Windows.UI.Xaml.Controls;

namespace SimpleZIP_UI.Presentation.View.Dialog
{
    /// <inheritdoc cref="ContentDialog" />
    public sealed partial class AboutDialog
    {
        private const string Author = "Matthias Fussenegger";
        private const string License = "GNU General Public License 3";

        /// <inheritdoc />
        public AboutDialog()
        {
            InitializeComponent();
            DevelopedByRun.Text = I18N.Resources.GetString("DevelopedBy/Text", Author);
            LicenseRun.Text = I18N.Resources.GetString("License/Text", License);
            PrimaryButtonText = I18N.Resources.GetString("ContentDialog/PrimaryButtonText");
        }

        private void ContentDialog_PrimaryButtonClick(
            ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            sender.Hide();
        }
    }
}
