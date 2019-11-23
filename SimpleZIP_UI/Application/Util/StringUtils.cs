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
using System.IO;
using System.Text;

namespace SimpleZIP_UI.Application.Util
{
    internal static class StringUtils
    {
        /// <summary>
        /// Appends a copy of the specified string followed by the default line terminator
        /// to the end of the current <see cref="T:System.Text.StringBuilder" /> object.
        /// If the specified string is <c>null</c> or empty, this is a no-op.
        /// </summary>
        /// <param name="sb">Instance of <see cref="StringBuilder"/>.</param>
        /// <param name="value">The string to be appended.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        public static StringBuilder CheckAndAppendLine(this StringBuilder sb, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                sb.AppendLine(value);
            }

            return sb;
        }

        /// <summary>
        /// Converts the specified string to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="s">The string to be converted.</param>
        /// <param name="charEncoding">Character encoding of the string.</param>
        /// <returns>A new <see cref="Stream"/> object.</returns>
        public static Stream ToStream(this string s, Encoding charEncoding = null)
        {
            var encoding = charEncoding ?? Encoding.UTF8;
            return new MemoryStream(encoding.GetBytes(s ?? string.Empty));
        }
    }
}
