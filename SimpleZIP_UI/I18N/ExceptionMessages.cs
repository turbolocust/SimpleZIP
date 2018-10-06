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

using SimpleZIP_UI.Application.Compression;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.I18N
{
    internal static class ExceptionMessages
    {
        /// <summary>
        /// Gets an internationalized string for the specified exception. 
        /// The string is a friendly error message which can be shown to users.
        /// </summary>
        /// <param name="ex">Exception from which to receive an internationalized message.</param>
        /// <param name="acceptDefault">Returns a generic error message instead of
        /// an empty string when there is no friendly message defined for the concrete type
        /// of the specified exception.</param>
        /// <param name="passwordSet">True to indicate that password is set.</param>
        /// <param name="file">File that was processed when exception occurred.</param>
        /// <returns>String value of a resource.</returns>
        internal static async Task<string> GetStringFor(Exception ex,
            bool acceptDefault, bool passwordSet, StorageFile file = null)
        {
            var message = string.Empty;
            switch (ex)
            {
                case SharpCompress.Common.CryptographicException _:
                    message = passwordSet
                        ? "ErrorReadingArchiveWithPassword/Text"
                        : "FileEncryptedMessage/Text"; // not all encryption types are supported
                    break;
                case InvalidOperationException _:
                    if (file != null)
                    {
                        // to inform that file format is not supported
                        message = await Archives.IsRarArchive(file)
                            ? "RAR5FormatNotSupported/Text"
                            : "FileFormatNotSupported/Text";
                    }
                    else
                    {
                        message = "FileFormatNotSupported/Text";
                    }
                    break;
                default:
                    if (acceptDefault)
                    {
                        message = "ErrorReadingArchive/Text";
                    }
                    break;
            }
            return message.Length > 0 ? Resources.GetString(message) : message;
        }
    }
}
