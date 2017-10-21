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

namespace SimpleZIP_UI.Presentation.View.Dialog
{
    /// <inheritdoc cref="ContentDialog" />
    public sealed partial class SettingsDialog
    {
        /// <inheritdoc />
        public SettingsDialog()
        {
            InitializeComponent();
            SetToggleButtonsToggledState();
            SetThemeGroupToggleButton();
            PrimaryButtonText = I18N.Resources.GetString("ContentDialog/PrimaryButtonText");
        }

        private void SetToggleButtonsToggledState()
        {
            Settings.TryGet(Settings.Keys.PreferOpenArchiveKey, out bool isOpenArchive);
            BrowseArchiveToggleSwitch.IsOn = isOpenArchive;
            Settings.TryGet(Settings.Keys.HideSomeArchiveTypesKey, out bool isHideSome);
            HideArchiveTypesToggleSwitch.IsOn = isHideSome;
        }

        private void SetThemeGroupToggleButton()
        {
            Settings.TryGet(Settings.Keys.ApplicationThemeKey, out string themeKey);
            LightThemeRadioButton.IsChecked = themeKey?.Equals(ApplicationTheme.Light.ToString());
            DarkThemeRadioButton.IsChecked = themeKey?.Equals(ApplicationTheme.Dark.ToString());
            SystemThemeRadioButton.IsChecked = LightThemeRadioButton.IsChecked == null ||
                                               LightThemeRadioButton.IsChecked == false &&
                                               DarkThemeRadioButton.IsChecked == null ||
                                               DarkThemeRadioButton.IsChecked == false;
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

        /// <summary>
        /// Invoked when a toggle button belonging to the theme group has been checked. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void ThemeGroupToggleButton_OnChecked(object sender, RoutedEventArgs args)
        {
            string theme = null;
            if (sender.Equals(LightThemeRadioButton))
            {
                theme = ApplicationTheme.Light.ToString();
            }
            else if (sender.Equals(DarkThemeRadioButton))
            {
                theme = ApplicationTheme.Dark.ToString();
            }
            Settings.PushOrUpdate(Settings.Keys.ApplicationThemeKey, theme);
        }
    }
}
