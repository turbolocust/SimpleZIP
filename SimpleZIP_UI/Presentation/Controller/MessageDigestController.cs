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

using System;
using SimpleZIP_UI.Application.Hashing;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Serilog;

namespace SimpleZIP_UI.Presentation.Controller
{
    internal class MessageDigestController : BaseController
    {
        private readonly ILogger _logger = Log.ForContext<MessageDigestController>();

        /// <summary>
        /// Instance used to compute hashes (message digest).
        /// </summary>
        internal IMessageDigestProvider MessageDigestProvider { get; }

        internal MessageDigestController(INavigable navHandler,
            IMessageDigestProvider provider) : base(navHandler, null)
        {
            MessageDigestProvider = provider;
        }

        /// <summary>
        /// Tries to compute a hash value by using <see cref="MessageDigestProvider"/>. If the specified
        /// file was not found, then a internationalized text is returned instead, indicating
        /// that the file could not be found.
        /// </summary>
        /// <param name="file">The file of which to compute a hash value.</param>
        /// <param name="algorithmName">The name of the message digest algorithm.</param>
        /// <returns>The hashed value as string or an internationalized and user-friendly string
        /// if the file could not be found.</returns>
        internal async Task<string> TryComputeHashValue(StorageFile file, string algorithmName)
        {
            string hash;

            try
            {
                // suppress hashed bytes (string is sufficient)
                (_, string hashedValue) = await MessageDigestProvider.ComputeAsync(file, algorithmName);
                hash = hashedValue;
            }
            catch (FileNotFoundException)
            {
                _logger.Error("File {FileName} not found.", file.Path);
                hash = $"<<{I18N.Resources.GetString("FileNotFound/Text")}>>";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error computing hash value of {FileName}", file.Name);
                hash = $"<<{I18N.Resources.GetString("Error/Text")}>>";
            }

            return hash;
        }
    }
}
