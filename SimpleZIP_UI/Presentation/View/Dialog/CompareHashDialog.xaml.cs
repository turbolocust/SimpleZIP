// ==++==
// 
// Copyright (C) 2019 Matthias Fussenegger
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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SimpleZIP_UI.Presentation.View.Dialog
{
    public sealed partial class CompareHashDialog
    {
        public string Hash { get; }

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new instance of this class.
        /// </summary>
        /// <param name="hash">The hash value to be compared.</param>
        public CompareHashDialog(string hash)
        {
            InitializeComponent();
            Hash = hash;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender,
            ContentDialogButtonClickEventArgs args)
        {
            sender.Hide();
        }

        private void CompareHashTextBox_TextChanged(object sender, TextChangedEventArgs args)
        {
            if (sender.Equals(CompareHashTextBox))
            {
                string text = CompareHashTextBox.Text;
                System.Drawing.Color color;

                if (text.Equals(Hash, StringComparison.OrdinalIgnoreCase))
                {
                    ResultIcon.Symbol = Symbol.Accept;
                    color = System.Drawing.Color.Green;
                }
                else if (string.IsNullOrEmpty(text))
                {
                    ResultIcon.Symbol = Symbol.Forward;
                    color = System.Drawing.Color.Empty;
                }
                else
                {
                    ResultIcon.Symbol = Symbol.Cancel;
                    color = System.Drawing.Color.DarkRed;
                }

                ResultIcon.Foreground = new SolidColorBrush(
                    Color.FromArgb(color.A, color.R, color.G, color.B));
            }
        }
    }
}
