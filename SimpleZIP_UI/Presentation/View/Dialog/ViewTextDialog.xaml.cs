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
    public sealed partial class ViewTextDialog
    {
        /// <summary>
        /// The title to be displayed.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Text which is bound to the text box.
        /// </summary>
        public string Text { get; set; }

        /// <inheritdoc />
        public ViewTextDialog(string header, string text)
        {
            InitializeComponent();
            Header = header;
            Text = text;
        }

        private void ContentDialog_PrimaryButtonClick(
            ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            sender.Hide();
        }
    }
}
