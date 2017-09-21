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
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Hashing
{
    /// <summary>
    /// Offers functionality for computing hash values.
    /// </summary>
    public interface IMessageDigestProvider
    {
        /// <summary>
        /// List of supported message digest algorithms.
        /// </summary>
        List<string> SupportedAlgorithms { get; }

        /// <summary>
        /// Computes a hash value using the specified message digest algorithm.
        /// See <see cref="HashAlgorithmNames"/> for the correct string representations.
        /// </summary>
        /// <param name="file">The file of which to compute a hash value.</param>
        /// <param name="algorithmName">The name of the message digest algorithm.</param>
        /// <returns>The hashed data in bytes and as hexadecimal string (in upper case).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if provided algorithm name is 
        /// either unknown or not supported.</exception>
        Task<(byte[] HashedBytes, string HashedValue)> ComputeHashValue(StorageFile file, string algorithmName);
    }
}
