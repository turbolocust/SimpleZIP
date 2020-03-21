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

using System;
using Windows.Foundation.Metadata;
using Windows.System.Profile;
using Windows.UI.Xaml;

namespace SimpleZIP_UI
{
    internal static class EnvironmentInfo
    {
        /// <summary>
        /// Holds the device family of this device as string.
        /// </summary>
        internal static readonly string DeviceFamily = AnalyticsInfo.VersionInfo.DeviceFamily;

        /// <summary>
        /// Returns true, if the current device is a mobile device, false otherwise.
        /// </summary>
        internal static bool IsMobileDevice => DeviceFamily.Equals("Windows.Mobile", StringComparison.Ordinal);

        /// <summary>
        /// Returns true, if the minimum API contract is that of the Creators Update.
        /// </summary>
        internal static bool IsMinCreatorsUpdate => CheckApiContract(4);

        /// <summary>
        /// Returns true, if the requested theme equals the dark theme.
        /// </summary>
        internal static bool IsDarkThemeEnabled => Windows.UI.Xaml.Application.Current.RequestedTheme == ApplicationTheme.Dark;

        private static bool CheckApiContract(ushort majorVersion)
        {
            const string contractName = "Windows.Foundation.UniversalApiContract";
            return ApiInformation.IsApiContractPresent(contractName, majorVersion);
        }
    }
}
