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
using Windows.UI.Xaml.Data;

namespace SimpleZIP_UI.Presentation.View.Converter
{
    /// <inheritdoc />
    /// <summary>
    /// Converter which flips the value of a Boolean.
    /// </summary>
    internal class FlipBooleanConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return FlipBoolean((bool)value);
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return FlipBoolean((bool)value);
        }

        private static bool FlipBoolean(bool value)
        {
            return !value;
        }
    }
}
