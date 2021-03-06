﻿// ==++==
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
using System.Globalization;
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
            ArchiveHistorySizeTextBlock.Text += " (0-" + ArchiveHistory.MaxHistoryItems + "):";
            ArchiveHistorySizeTextBox.Text = GetCurrentSizeLimit().ToString(CultureInfo.InvariantCulture);
        }

        private static int GetCurrentSizeLimit()
        {
            if (!Settings.TryGet(Settings.Keys.ArchiveHistorySize, out int curValue))
            {
                curValue = (int)ArchiveHistory.MaxHistoryItems;
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
            if (Settings.TryGetTheme(out var theme))
            {
                if (theme.Equals(ApplicationTheme.Dark))
                {
                    LightThemeRadioButton.IsChecked = false;
                    DarkThemeRadioButton.IsChecked = true;
                    SystemThemeRadioButton.IsChecked = false;
                }
                else
                {
                    LightThemeRadioButton.IsChecked = true;
                    DarkThemeRadioButton.IsChecked = false;
                    SystemThemeRadioButton.IsChecked = false;

                }
            }
            else // no theme found
            {
                LightThemeRadioButton.IsChecked = false;
                DarkThemeRadioButton.IsChecked = false;
                SystemThemeRadioButton.IsChecked = true;
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
            if (sender.Equals(SystemThemeRadioButton))
            {
                Settings.Remove(Settings.Keys.ApplicationThemeKey);
            }
            else
            {
                var theme = sender.Equals(DarkThemeRadioButton)
                    ? ApplicationTheme.Dark : ApplicationTheme.Light;
                Settings.SaveTheme(theme);
            }
        }

        private void ArchiveHistorySizeTextBox_OnTextChanged(object sender, TextChangedEventArgs args)
        {
            if (sender is TextBox textBox)
            {
                if (int.TryParse(textBox.Text, out int value) &&
                    value >= 0 && value <= ArchiveHistory.MaxHistoryItems)
                {
                    Settings.PushOrUpdate(Settings.Keys.ArchiveHistorySize, value);
                }
                else // reset
                {
                    textBox.Text = GetCurrentSizeLimit().ToString(CultureInfo.InvariantCulture);
                }
            }
        }
    }
}
