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

using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleZIP_UI.Application.Hashing;

namespace SimpleZIP_UI.Presentation.Controller
{
    internal class MessageDigestController : BaseController
    {
        /// <summary>
        /// Instance used to compute hashes (message digest).
        /// </summary>
        internal IMessageDigestProvider MdProvider { get; }

        internal MessageDigestController(INavigation navHandler,
            IMessageDigestProvider provider) : base(navHandler, null)
        {
            MdProvider = provider;
        }

        /// <summary>
        /// Tries to compute a hash value by using <see cref="MdProvider"/>. If the specified
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
                var (_, hashedValue) = await MdProvider
                    .ComputeHashValue(file, algorithmName);
                hash = hashedValue;
            }
            catch (FileNotFoundException)
            {
                hash = $"<<{I18N.Resources.GetString("FileNotFound/Text")}>>";
            }

            return hash;
        }
    }
}
