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

using SimpleZIP_UI.Presentation.Handler;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SimpleZIP_UI.Presentation.View
{
    /// <inheritdoc cref="Page" />
    public sealed partial class SettingsPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            SetToggleButtonsToggledState();
            SetThemeGroupToggleButton();
            ArchiveHistorySizeTextBlock.Text += " (0-" + ArchiveHistoryHandler.MaxHistoryItems + "):";
            ArchiveHistorySizeTextBox.Text = GetCurrentSizeLimit().ToString();
            SetToggleButtonsToggledState();
        }

        private static int GetCurrentSizeLimit()
        {
            if (!Settings.TryGet(Settings.Keys.ArchiveHistorySize, out int curValue))
            {
                curValue = (int)ArchiveHistoryHandler.MaxHistoryItems;
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
