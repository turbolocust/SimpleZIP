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
using SimpleZIP_UI.Application.Compression.Algorithm.Type;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression
{
    internal static class Archives
    {
        private const int Rar5HeaderSize = 8;

        private const string Rar5HeaderSignature = "526172211A070100";

        /// <summary>
        /// Enumeration to identify archive types.
        /// </summary>
        public enum ArchiveType
        {
            Zip, GZip, BZip2, LZip, Tar, TarGz, TarBz2, TarLz, Rar
        }

        /// <summary>
        /// Maps file types for each archive. Consists only of 
        /// file types with a single file name extension.
        /// </summary>
        internal static readonly IDictionary<string, ArchiveType> ArchiveFileTypes;

        /// <summary>
        /// Maps file types for each archive. Consists only of 
        /// file types with multiple file name extensions.
        /// </summary>
        internal static readonly IDictionary<string, ArchiveType> ArchiveExtendedFileTypes;

        static Archives()
        {
            // populate dictionary that maps file types to archive types
            ArchiveFileTypes = new Dictionary<string, ArchiveType>(Enum.GetNames(typeof(ArchiveType)).Length * 2)
            {
                {".zip", ArchiveType.Zip},
                {".tar", ArchiveType.Tar},
                {".gzip", ArchiveType.GZip},
                {".gz", ArchiveType.GZip},
                {".tgz", ArchiveType.TarGz},
                {".bz2", ArchiveType.BZip2},
                {".bzip2", ArchiveType.BZip2},
                {".tbz2", ArchiveType.TarBz2},
                {".lz", ArchiveType.LZip },
                {".lzip", ArchiveType.LZip },
                {".lzma", ArchiveType.LZip },
                {".tlz", ArchiveType.TarLz},
                {".rar", ArchiveType.Rar }
            };

            // populate dictionary that maps extended file types to archive types
            ArchiveExtendedFileTypes = new Dictionary<string, ArchiveType>
            {
                { ".tar.gz", ArchiveType.TarGz },
                { ".tar.gzip", ArchiveType.TarGz },
                { ".tar.bz2", ArchiveType.TarBz2 },
                { ".tar.bzip2", ArchiveType.TarBz2 },
                { ".tar.lz", ArchiveType.TarLz },
                { ".tar.lzip", ArchiveType.TarLz },
                { ".tar.lzma", ArchiveType.TarLz }
            };
        }

        /// <summary>
        /// Returns the algorithm instance that is mapped to the specified archive type.
        /// </summary>
        /// <param name="value">The enum value of the archive type to be determined.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when archive type matched no algorithm.</exception>
        /// <returns>An instance of the compressor algorithm that is mapped to the specified value.</returns>
        internal static ICompressionAlgorithm DetermineAlgorithm(ArchiveType value)
        {
            ICompressionAlgorithm algorithm;
            switch (value)
            {
                case ArchiveType.Zip:
                    algorithm = new Zip();
                    break;
                case ArchiveType.GZip:
                    algorithm = new GZip();
                    break;
                case ArchiveType.BZip2:
                    algorithm = new BZip2();
                    break;
                case ArchiveType.LZip:
                    algorithm = new LZip();
                    break;
                case ArchiveType.Tar:
                    algorithm = new Tar();
                    break;
                case ArchiveType.TarGz:
                    algorithm = new TarGzip();
                    break;
                case ArchiveType.TarBz2:
                    algorithm = new TarBzip2();
                    break;
                case ArchiveType.TarLz:
                    algorithm = new TarLzip();
                    break;
                case ArchiveType.Rar:
                    algorithm = new Rar();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
            return algorithm;
        }

        /// <summary>
        /// Checks whether the file is a RAR4 or RAR5 archive.
        /// </summary>
        /// <param name="file">The file to be checked.</param>
        /// <returns>True if file is RAR4 or RAR5 archive, false otherwise.</returns>
        internal static async Task<bool> IsRarArchive(StorageFile file)
        {
            bool isRarArchive;
            try
            {
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    isRarArchive = SharpCompress.Archives.Rar.RarArchive.IsRarFile(stream);
                }
                if (!isRarArchive) // check if RAR5 format
                {
                    using (var stream = await file.OpenStreamForReadAsync())
                    {
                        var buffer = new byte[Rar5HeaderSize];
                        var readBytes = stream.Read(buffer, 0, Rar5HeaderSize);
                        if (readBytes > 0)
                        {
                            var header = new StringBuilder(Rar5HeaderSize * 2);
                            foreach (var value in buffer)
                            {
                                header.AppendFormat("{0:X2}", value); // convert to HEX
                            }
                            isRarArchive = header.ToString().Equals(Rar5HeaderSignature);
                        }
                    }
                }
            }
            catch (IOException)
            {
                isRarArchive = false;
            }
            return isRarArchive;
        }
    }
}
