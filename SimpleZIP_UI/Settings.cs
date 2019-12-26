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
using Windows.Storage;
using Windows.UI.Xaml;

namespace SimpleZIP_UI
{
    internal static class Settings
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        /// <summary>
        /// Stores away the specified application theme preference.
        /// </summary>
        /// <param name="theme">The theme to be stored away.</param>
        internal static void SaveTheme(ApplicationTheme theme)
        {
            PushOrUpdate(Keys.ApplicationThemeKey, theme.ToString());
        }

        /// <summary>
        /// Returns the stored away theme. If no theme is in the storage,
        /// then <see cref="ApplicationTheme.Light"/> is returned.
        /// </summary>
        /// <returns>False if no theme is in storage, true otherwise.</returns>
        internal static bool TryGetTheme(out ApplicationTheme theme)
        {
            bool found = TryGet(Keys.ApplicationThemeKey, out string themeName);
            if (!string.IsNullOrEmpty(themeName) && themeName.Equals(
                    ApplicationTheme.Dark.ToString(), StringComparison.Ordinal))
            {
                theme = ApplicationTheme.Dark;
            }
            else
            {
                theme = ApplicationTheme.Light;
            }

            return found;
        }

        /// <summary>
        /// Adds a new value to the local settings or updates the existing one.
        /// </summary>
        /// <param name="key">The key of the mapped value.</param>
        /// <param name="value">Value which is mapped to the key.</param>
        internal static void PushOrUpdate(string key, object value)
        {
            LocalSettings.Values[key] = value;
        }

        /// <summary>
        /// Returns the value which is mapped to the specified key.
        /// </summary>
        /// <param name="key">The key of the mapped value.</param>
        /// <returns>The value which is mapped to the specified key.</returns>
        internal static object Get(string key)
        {
            return LocalSettings.Values[key];
        }

        /// <summary>
        /// Removes the record with the specified key.
        /// </summary>
        /// <param name="key">The key of the record to be removed.</param>
        /// <returns>False if no record with the specified key exists.</returns>
        internal static bool Remove(string key)
        {
            return LocalSettings.Values.Remove(key);
        }

        /// <summary>
        /// Tries to get the value which is mapped to the specified key.
        /// If the value exists and was safely cast to the specified type,
        /// true is returned and the value is guaranteed to be of the exact
        /// type as specified. If not, false is returned and the out value
        /// equals the default value of that specific type.
        /// </summary>
        /// <typeparam name="T">Exact type of the out value.</typeparam>
        /// <param name="key">The key of the mapped value.</param>
        /// <param name="value">Outgoing value with the same type as specified.</param>
        /// <returns>True if value was found and safely cast, false otherwise.</returns>
        internal static bool TryGet<T>(string key, out T value)
        {
            var val = LocalSettings.Values[key];
            if (val?.GetType() == typeof(T))
            {
                value = (T)val;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Keys used to store application settings.
        /// </summary>
        internal static class Keys
        {
            internal const string ApplicationThemeKey = "ApplicationTheme";
            internal const string HideSomeArchiveTypesKey = "HideSomeArchiveTypes";
            internal const string PreferOpenArchiveKey = "PreferOpenArchive";
            internal const string RecentArchivesKey = "RecentArchives";
            internal const string ArchiveHistorySize = "ArchiveHistorySize";
            internal const string RecentHashAlgorithmKey = "RecentHashAlgorithm";
            internal const string LowerCaseHashToggledKey = "LowerCaseHashToggled";
            internal const string DisplayLocationToggledKey = "DisplayLocationToggled";
        }
    }
}
