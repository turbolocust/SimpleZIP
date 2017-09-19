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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="ContentDialog" />
    public sealed partial class SettingsDialog
    {
        /// <inheritdoc />
        public SettingsDialog()
        {
            InitializeComponent();
            SetToggleButtonsToggledState();
        }

        private void SetToggleButtonsToggledState()
        {
            Settings.TryGet(Settings.Keys.PreferOpenArchiveKey, out bool isOpenArchive);
            BrowseArchiveToggleSwitch.IsOn = isOpenArchive;
            Settings.TryGet(Settings.Keys.HideSomeArchiveTypesKey, out bool isHideSome);
            HideArchiveTypesToggleSwitch.IsOn = isHideSome;
        }


        private void SettingsDialog_OnOpened(ContentDialog sender,
            ContentDialogOpenedEventArgs args)
        {
            SetToggleButtonsToggledState();
        }

        /// <summary>
        /// Invoked when the primary button of this dialog has been pressed. 
        /// This will simply hide the dialog.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender,
            ContentDialogButtonClickEventArgs args)
        {
            sender.Hide();
        }

        /// <summary>
        /// Invoked when a toggle button has been toggled. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs args)
        {
            if (sender.Equals(BrowseArchiveToggleSwitch))
            {
                Settings.PushOrUpdate(Settings.Keys.PreferOpenArchiveKey,
                    BrowseArchiveToggleSwitch.IsOn);

            }
            else if (sender.Equals(HideArchiveTypesToggleSwitch))
            {
                Settings.PushOrUpdate(Settings.Keys.HideSomeArchiveTypesKey,
                    HideArchiveTypesToggleSwitch.IsOn);
            }
        }
    }
}
