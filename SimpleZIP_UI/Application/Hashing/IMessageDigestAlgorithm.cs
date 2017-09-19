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
using Windows.Security.Cryptography.Core;

namespace SimpleZIP_UI.Application.Hashing
{
    /// <summary>
    /// Offers functionality for calculating hash values.
    /// </summary>
    public interface IMessageDigestAlgorithm
    {
        /// <summary>
        /// Calculates a hash value using the specified message digest algorithm.
        /// See <see cref="HashAlgorithmNames"/> for the correct string representations.
        /// </summary>
        /// <param name="data">The data to be hashed.</param>
        /// <param name="algorithmName">The name of the message digest algorithm.</param>
        /// <param name="hashString">Hexadecimal string representation (in upper case) of the hash value.</param>
        /// <returns>The hashed data.</returns>
        byte[] CalculateHashValue(byte[] data, string algorithmName, out string hashString);
    }
}
