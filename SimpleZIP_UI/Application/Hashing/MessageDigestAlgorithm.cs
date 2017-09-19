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
using System.Security.Cryptography;
using System.Text;

namespace SimpleZIP_UI.Application.Hashing
{
    internal class MessageDigestAlgorithm : IMessageDigestAlgorithm
    {
        /// <inheritdoc />
        public byte[] CalculateHashValue(byte[] data, string algorithmName, out string hashString)
        {
            var algorithm = GetHashAlgorithm(algorithmName);
            var hashedData = algorithm.ComputeHash(data);
            hashString = ConvertHashValueToString(hashedData);
            return hashedData;
        }

        private HashAlgorithm GetHashAlgorithm(string name)
        {
            HashAlgorithm algorithm;
            switch (name)
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
                    throw new ArgumentOutOfRangeException(nameof(name), name, null);
            }
            return algorithm;
        }

        private static string ConvertHashValueToString(IEnumerable<byte> hashedData)
        {
            var stringBuilder = new StringBuilder();
            foreach (var byteVal in hashedData)
            {
                // convert to upper case hex format
                stringBuilder.AppendFormat("{0:X2}", byteVal);
            }
            return stringBuilder.ToString();
        }
    }
}
