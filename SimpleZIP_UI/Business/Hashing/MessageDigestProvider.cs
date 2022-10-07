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

using SimpleZIP_UI.Business.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Business.Hashing
{
    /// <inheritdoc />
    /// <summary>
    /// Implementation for computing hash values using <c>System.Security.Cryptography</c>.
    /// </summary>
    internal class MessageDigestProvider : IMessageDigestProvider
    {
        /// <inheritdoc />
        /// <remarks>
        /// If used in a more public manner, then a copy should be returned.
        /// </remarks>
        public IReadOnlyList<string> SupportedAlgorithms { get; }

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
        public Task<(byte[] HashedBytes, string HashedValue)>
            ComputeAsync(StorageFile file, string algorithmName)
        {
            return Task.Run(async () =>
            {
                using (var stream = await file.OpenStreamForReadAsync().ConfigureAwait(false))
                {
                    return ComputeHash(stream, algorithmName);
                }
            });
        }

        /// <inheritdoc />
        public Task<(byte[] HashedBytes, string HashedValue)>
            ComputeAsync(string value, string algorithmName)
        {
            return Task.Run(() =>
            {
                using (var stream = value.ToStream())
                {
                    return ComputeHash(stream, algorithmName);
                }
            });
        }

        private static (byte[] HashedBytes, string HashedValue)
            ComputeHash(Stream stream, string algorithmName)
        {
            using (var algorithm = GetHashAlgorithm(algorithmName))
            {
                var hashedBytes = algorithm.ComputeHash(stream);
                string hashedValue = ConvertHashValueToString(hashedBytes);
                return (hashedBytes, hashedValue);
            }
        }

        private static string ConvertHashValueToString(byte[] hashedBytes)
        {
            // convert to upper case hex format
            return BitConverter.ToString(hashedBytes).Replace("-", "", StringComparison.Ordinal);
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
