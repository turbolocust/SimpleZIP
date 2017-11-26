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
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression;

namespace SimpleZIP_UI.I18N
{
    internal static class Resources
    {
        private static readonly ResourceLoader Loader;

        static Resources()
        {
            Loader = new ResourceLoader();
        }

        /// <summary>
        /// Gets the string with the specified name.
        /// </summary>
        /// <param name="name">The name of the string to be get.</param>
        /// <returns>String value of a resource.</returns>
        internal static string GetString(string name)
        {
            return Loader.GetString(name);
        }

        /// <summary>
        /// Gets the string with the specified name and replaces the format items 
        /// with the string representation of the corresponding object in the specified array.
        /// </summary>
        /// <param name="name">The name of the string to be get.</param>
        /// <param name="objects">Objects to be replaced with the format items.</param>
        /// <returns>String value of a resource.</returns>
        internal static string GetString(string name, params object[] objects)
        {
            var value = Loader.GetString(name);
            return string.Format(value, objects);
        }
    }

    internal static class ExceptionMessageHandler
    {
        /// <summary>
        /// Gets an internationalized string from the specified exception. The string
        /// is a friendly error message which can be shown to users.
        /// </summary>
        /// <param name="ex">Exception from which to receive an internationalized message.</param>
        /// <param name="acceptDefault">Returns a generic error message instead of
        /// an empty string when there is no friendly message defined for the concrete type
        /// of the specified exception.</param>
        /// <param name="file">File that was processed when exception occurred.</param>
        /// <returns>String value of a resource.</returns>
        internal static async Task<string> GetStringFrom(Exception ex, bool acceptDefault, StorageFile file = null)
        {
            var message = string.Empty;
            switch (ex)
            {
                case SharpCompress.Common.CryptographicException _:
                    // encrypted files are currently not supported
                    message = "FileEncryptedMessage/Text";
                    break;
                case InvalidOperationException _:
                    if (file != null)
                    {
                        // to inform that file format is not supported,
                        // e.g. when user tries to extract RAR5 file
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
