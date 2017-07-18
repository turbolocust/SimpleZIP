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
using SimpleZIP_UI.Application.Compression.Algorithm;

namespace SimpleZIP_UI.Application.Compression.Model
{
    /// <summary>
    /// Options for compressor streams used in <see cref="CompressorAlgorithm"/>.
    /// </summary>
    public class CompressorOptions
    {
        /// <summary>
        /// File name to be set for compression stream.
        /// </summary>
        internal string FileName { get; set; }

        /// <summary>
        /// Comment to be set for compression stream.
        /// </summary>
        internal string Comment { get; set; }

        /// <summary>
        /// True indicates a stream for compression, false for decompression.
        /// </summary>
        internal bool IsCompression { get; }

        internal CompressorOptions(bool isCompression)
        {
            IsCompression = isCompression;
        }
    }
}
