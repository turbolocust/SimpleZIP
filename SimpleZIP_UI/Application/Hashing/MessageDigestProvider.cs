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
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Hashing
{
    /// <inheritdoc />
    /// <summary>
    /// Implementation for computing hash values using <see cref="N:System.Security.Cryptography" />.
    /// </summary>
    internal class MessageDigestProvider : IMessageDigestProvider
    {
        /// <inheritdoc />
        public List<string> SupportedAlgorithms { get; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        public MessageDigestProvider()
        {
            SupportedAlgorithms = new List<string>
            {
                "MD5", "SHA1", "SHA256", "SHA384", "SHA512"
            };
        }

        /// <inheritdoc />
        public async Task<(byte[] HashedBytes, string HashedValue)> ComputeHashValue(StorageFile file, string algorithmName)
        {
            using (var fileStream = await file.OpenStreamForReadAsync())
            {
                var algorithm = GetHashAlgorithm(algorithmName);
                var hashedBytes = algorithm.ComputeHash(fileStream);
                var hashedValue = ConvertHashValueToString(hashedBytes);
                return (hashedBytes, hashedValue);
            }
        }

        /// <summary>
        /// Converts the specified byte array to a hexadecimal string representation (upper case).
        /// </summary>
        /// <param name="hashedBytes">The byte array to be converted.</param>
        /// <returns>The hexadecimal string representation of the specified byte array.</returns>
        public static string ConvertHashValueToString(IReadOnlyCollection<byte> hashedBytes)
        {
            var stringBuilder = new StringBuilder(hashedBytes.Count);
            foreach (var byteValue in hashedBytes)
            {
                // convert to upper case hex format
                stringBuilder.AppendFormat("{0:X2}", byteValue);
            }
            return stringBuilder.ToString();
        }

        private static HashAlgorithm GetHashAlgorithm(string algorithmName)
        {
            HashAlgorithm algorithm;
            switch (algorithmName)
            {
                case "MD5":
                    algorithm = MD5.Create();
                    break;
                case "SHA1":
                    algorithm = SHA1.Create();
                    break;
                case "SHA256":
                    algorithm = SHA256.Create();
                    break;
                case "SHA384":
                    algorithm = SHA384.Create();
                    break;
                case "SHA512":
                    algorithm = SHA512.Create();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithmName));
            }
            return algorithm;
        }
    }
}
