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

using SimpleZIP_UI.Application.Compression.Model;

namespace SimpleZIP_UI.Application.Compression.Operation
{
    internal static class Operations
    {
        /// <summary>
        /// Returns a new operation for compression.
        /// </summary>
        /// <returns>A new operation for compression.</returns>
        internal static ArchivingOperation<CompressionInfo> ForCompression()
        {
            return new CompressionOperation();
        }

        /// <summary>
        /// Returns a new operation for decompression.
        /// </summary>
        /// <returns>A new operation for decompression.</returns>
        internal static ArchivingOperation<DecompressionInfo> ForDecompression()
        {
            return new DecompressionOperation();
        }
    }
}
