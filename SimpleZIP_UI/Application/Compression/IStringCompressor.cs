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
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Application.Compression
{
    internal interface IStringCompressor : ICompressor<string, string>
    {
    }

    /// <summary>
    /// Compresses and decompresses strings using a GZIP compressor.
    /// </summary>
    internal class StringGzipCompressor : IStringCompressor
    {
        private readonly Encoding _encoding;

        /// <summary>
        /// Constructes a new instance of this class.
        /// </summary>
        /// <param name="charEncoding">Character encoding of strings
        /// to be processed by this instance. Defaults to
        /// <see cref="Encoding.UTF8"/>.</param>
        public StringGzipCompressor(Encoding charEncoding = null)
        {
            _encoding = charEncoding ?? Encoding.UTF8;
        }

        /// <inheritdoc />
        /// <summary>
        /// Compresses the specified string.
        /// </summary>
        /// <param name="value">The string to be compressed.</param>
        /// <returns>BASE64 encoded string representation of compressed data.</returns>
        /// <exception cref="IOException">Thrown by streams if something went wrong.</exception>
        /// <exception cref="ArgumentNullException">Thrown if specified argument is <code>null</code>.</exception>
        /// <exception cref="EncoderFallbackException">Thrown if a fallback occurred.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public string Compress(string value)
        {
            string base64;

            using (var inputStream = value.ToStream(_encoding))
            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress, true))
                {
                    inputStream.CopyTo(gzipStream);
                }
                var compressed = outputStream.ToArray();
                base64 = Convert.ToBase64String(compressed);
            }

            return base64;
        }

        /// <inheritdoc />
        /// <summary>
        /// Decompresses the specified string.
        /// </summary>
        /// <param name="input">BASE64 encoded string representation of compressed data.</param>
        /// <returns>The decompressed string.</returns>
        /// <exception cref="FormatException">Thrown if input is not BASE64 encoded.</exception>
        /// <exception cref="ArgumentException">Thrown if input is not compressed with GZIP.</exception>
        /// <exception cref="ArgumentNullException">Thrown if specified argument is <code>null</code>.</exception>
        /// <exception cref="DecoderFallbackException">Thrown if a fallback occurred.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public string Decompress(string input)
        {
            string output;

            var compressed = Convert.FromBase64String(input);

            using (var inputStream = new MemoryStream(compressed))
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                gzipStream.CopyTo(outputStream);
                var decompressed = outputStream.ToArray();
                output = _encoding.GetString(decompressed);
            }

            return output;
        }
    }
}
