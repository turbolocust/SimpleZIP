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
using SimpleZIP_UI.Presentation.Handler;

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
            ArchiveHistorySizeTextBlock.Text += " (0-" + ArchiveHistoryHandler.MaxHistoryItems + "):";
            ArchiveHistorySizeTextBox.Text = GetCurrentSizeLimit().ToString();
        }

        private static int GetCurrentSizeLimit()
        {
            if (!Settings.TryGet(Settings.Keys.ArchiveHistorySize, out int curValue))
            {
                curValue = (int) ArchiveHistoryHandler.MaxHistoryItems;
            }
            return curValue;
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
            Settings.TryGet(Settings.Keys.ApplicationThemeKey, out string theme);
            if (theme == null) // system theme
            {
                LightThemeRadioButton.IsChecked = false;
                DarkThemeRadioButton.IsChecked = false;
                SystemThemeRadioButton.IsChecked = true;
            }
            else if (theme.Equals(ApplicationTheme.Light.ToString()))
            {
                LightThemeRadioButton.IsChecked = true;
                DarkThemeRadioButton.IsChecked = false;
                SystemThemeRadioButton.IsChecked = false;

            }
            else if (theme.Equals(ApplicationTheme.Dark.ToString()))
            {
                LightThemeRadioButton.IsChecked = false;
                DarkThemeRadioButton.IsChecked = true;
                SystemThemeRadioButton.IsChecked = false;
            }
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
        /// Invoked when a toggle button belonging to the theme group has been toggled. 
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void ThemeGroupToggleButton_OnChecked(object sender, RoutedEventArgs args)
        {
            string theme;
            if (sender.Equals(LightThemeRadioButton))
            {
                theme = ApplicationTheme.Light.ToString();
            }
            else if (sender.Equals(DarkThemeRadioButton))
            {
                theme = ApplicationTheme.Dark.ToString();
            }
            else // system theme
            {
                theme = null;
            }
            Settings.PushOrUpdate(Settings.Keys.ApplicationThemeKey, theme);
        }

        /// <summary>
        /// Invoked when text has been changed in the textbox for archive history size.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">Consists of event parameters.</param>
        private void ArchiveHistorySizeTextBox_OnTextChanged(object sender, TextChangedEventArgs args)
        {
            if (sender is TextBox textBox)
            {
                if (int.TryParse(textBox.Text, out int value) &&
                    value >= 0 && value <= ArchiveHistoryHandler.MaxHistoryItems)
                {
                    Settings.PushOrUpdate(Settings.Keys.ArchiveHistorySize, value);
                }
                else
                {
                    textBox.Text = GetCurrentSizeLimit().ToString(); // reset
                }
            }
        }
    }
}
