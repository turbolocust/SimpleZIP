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

using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace SimpleZIP_UI.Presentation.View.Dialog
{
    public sealed partial class EnterPasswordDialog
    {
        public string FileName { get; set; }

        public string Password { get; set; }

        public EnterPasswordDialog(string fileName)
        {
            InitializeComponent();
            FileName = fileName;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender,
            ContentDialogButtonClickEventArgs args)
        {
            sender.Hide();
        }

        private void PasswordBox_OnKeyDown(object sender, KeyRoutedEventArgs args)
        {
            if (sender.Equals(PasswordBox) && args.Key == VirtualKey.Enter)
            {
                ContentDialog.Hide();
            }
        }
    }
}
