// ==++==
// 
// Copyright (C) 2020 Matthias Fussenegger
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

using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SimpleZIP_UI.Presentation.Util
{
    internal static class PageUtils
    {
        /// <summary>
        /// Tries to determine the resource with the key <c>SystemAccentColor</c>. If the
        /// resources cannot be determined (for whatever reason) or if the dark theme is
        /// enabled, then a color brush with the color <see cref="Colors.White"/> is returned.
        /// </summary>
        /// <param name="page">An instance of <see cref="Page"/>.</param>
        /// <returns>A new instance of <see cref="SolidColorBrush"/>.</returns>
        internal static SolidColorBrush DetermineSystemAccentColorBrush(this Page page)
        {
            const string resourceKey = "SystemAccentColor";
            SolidColorBrush solidColorBrush = null;

            if (page.Resources.TryGetValue(resourceKey, out var resource))
            {
                if (resource is SolidColorBrush brush)
                {
                    solidColorBrush = brush;
                }
            }

            if (EnvironmentInfo.IsDarkThemeEnabled || solidColorBrush == null)
            {
                solidColorBrush = new SolidColorBrush(Colors.White);
            }

            return solidColorBrush;
        }
    }
}
