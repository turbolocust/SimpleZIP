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

using System;

namespace SimpleZIP_UI.Business.Compression.Algorithm.Factory
{
    internal static class AlgorithmFactory
    {
        /// <summary>
        /// Returns the algorithm instance that is mapped to the specified archive type.
        /// </summary>
        /// <param name="value">The enum value of the archive type to be determined.</param>
        /// <param name="options">Options to be passed to the instance of type <see cref="ICompressionAlgorithm"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when archive type matched <b>no</b> algorithm.</exception>
        /// <returns>An instance of the compressor algorithm that is mapped to the specified value.</returns>
        internal static ICompressionAlgorithm DetermineAlgorithm(Archives.ArchiveType value, AlgorithmOptions options)
        {
            ICompressionAlgorithm algorithm;

            switch (value)
            {
                case Archives.ArchiveType.Zip:
                    algorithm = new Type.SZL.Zip(options); // use SharpZipLib
                    break;
                case Archives.ArchiveType.GZip:
                    algorithm = new Type.GZip(options);
                    break;
                case Archives.ArchiveType.BZip2:
                    algorithm = new Type.BZip2(options);
                    break;
                case Archives.ArchiveType.LZip:
                    algorithm = new Type.LZip(options);
                    break;
                case Archives.ArchiveType.Tar:
                    algorithm = new Type.Tar(options);
                    break;
                case Archives.ArchiveType.TarGz:
                    algorithm = new Type.TarGzip(options);
                    break;
                case Archives.ArchiveType.TarBz2:
                    algorithm = new Type.TarBzip2(options);
                    break;
                case Archives.ArchiveType.TarLz:
                    algorithm = new Type.TarLzip(options);
                    break;
                case Archives.ArchiveType.Rar:
                    algorithm = new Type.Rar(options);
                    break;
                // ReSharper disable once RedundantCaseLabel
                case Archives.ArchiveType.Unknown:
                // fall-through
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }

            return algorithm;
        }
    }
}
