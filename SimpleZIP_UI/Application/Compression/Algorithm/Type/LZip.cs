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
using System.IO;
using SharpCompress.Compressors;
using SharpCompress.Compressors.LZMA;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type
{
    /// <inheritdoc />
    /// <summary>
    /// Represents the LZMA compressor algorithm.
    /// </summary>
    public class LZip : CompressorAlgorithm
    {
        /// <inheritdoc />
        protected override Stream GetCompressorStream(Stream stream, CompressorOptions options)
        {
            return options.IsCompression
                ? new LZipStream(stream, CompressionMode.Compress)
                : new LZipStream(stream, CompressionMode.Decompress);
        }
    }
}
