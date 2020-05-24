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

using SharpCompress.Common;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;
using SimpleZIP_UI.Application.Compression.Algorithm.Factory;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type
{
    /// <inheritdoc />
    /// <summary>
    /// Represents the ZIP archiving algorithm.
    /// </summary>
    public sealed class Zip : ArchivingAlgorithm
    {
        /// <inheritdoc />
        public Zip(AlgorithmOptions options) : base(ArchiveType.Zip, options)
        {
        }

        /// <inheritdoc />
        protected override WriterOptions GetWriterOptions()
        {
            return new ZipWriterOptions(CompressionType.Deflate)
            {
                DeflateCompressionLevel = CompressionLevel.Default
            };
        }
    }
}