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
using SimpleZIP_UI.Application.Compression.Algorithm;
using SimpleZIP_UI.Application.Compression.Algorithm.Type;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace SimpleZIP_UI.Application.Compression
{
    internal static class Archives
    {
        /// <summary>
        /// Enumeration to identify archive types.
        /// </summary>
        public enum ArchiveType
        {
            Unknown, Zip, GZip, BZip2, LZip, Tar, TarGz, TarBz2, TarLz, Rar
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

        /// <summary>
        /// Maps <see cref="SharpCompress.Common.ArchiveType"/> to <see cref="ArchiveType"/>.
        /// </summary>
        internal static readonly IDictionary<SharpCompress.Common.ArchiveType, ArchiveType> ArchiveTypes;

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
            // populate dictionary which maps third-party archive types to local ones
            ArchiveTypes = new Dictionary<SharpCompress.Common.ArchiveType, ArchiveType>
            {
                { SharpCompress.Common.ArchiveType.Zip, ArchiveType.Zip },
                { SharpCompress.Common.ArchiveType.Tar, ArchiveType.Tar },
                { SharpCompress.Common.ArchiveType.Rar, ArchiveType.GZip },
                { SharpCompress.Common.ArchiveType.GZip, ArchiveType.GZip }
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
            // ReSharper disable once SwitchStatementMissingSomeCases
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
        /// 
        /// </summary>
        /// <param name="file">The archive to be checked.</param>
        /// <param name="password">The password of the file if encrypted.</param>
        /// <returns></returns>
        /// <exception cref="CryptographicException">
        /// Thrown if no or wrong password has been provided.</exception>
        /// <exception cref="InvalidArchiveTypeException">
        /// Thrown if archive type is not supported.</exception>
        internal static async Task<ArchiveType> DetermineArchiveType(StorageFile file, string password = null)
        {
            var archiveType = ArchiveType.Unknown;

            try
            {
                // let SharpCompress' reader do the archive detection
                var options = new ReaderOptions { Password = password };
                using (var stream = await file.OpenStreamForReadAsync())
                using (var reader = ReaderFactory.Open(stream, options))
                {
                    var type = reader.ArchiveType;
                    if (ArchiveTypes.TryGetValue(type, out var localType))
                    {
                        archiveType = localType;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(CryptographicException))
                {
                    throw; // just re-throw
                }

                throw new InvalidArchiveTypeException("Archive type is unknown or not supported.", ex);
            }

            return archiveType;
        }

        /// <summary>
        /// Checks whether the file is a RAR4 or RAR5 archive. The specified
        /// stream is expected to be seekable. Only then the check for
        /// RAR5 will be performed in addition to the RAR4 check (if not RAR4).
        /// Otherwise the check for the RAR5 format will only be performed
        /// if explicitly set via the optional parameter.
        /// </summary>
        /// <param name="stream">Archive stream to be checked.</param>
        /// <param name="password">The password of the file if encrypted.</param>
        /// <param name="rar5Only">True to only check for RAR5 format, false otherwise.</param>
        /// <returns></returns>
        internal static bool IsRarArchive(Stream stream,
            string password = null, bool rar5Only = false)
        {
            const int rar5HeaderSize = 8; // bytes
            const string rar5HeaderSignature = "526172211A070100";

            bool isRarArchive = false;
            try
            {
                if (!rar5Only)
                {
                    isRarArchive = RarArchive.IsRarFile(stream,
                        new ReaderOptions
                        {
                            Password = password,
                            LeaveStreamOpen = true
                        });
                }
                // check if RAR5 format if not RAR4
                if (rar5Only || !isRarArchive && stream.CanSeek)
                {
                    if (!rar5Only)
                    {
                        stream.Seek(0, SeekOrigin.Begin); // reset stream
                    }
                    var buffer = new byte[rar5HeaderSize];
                    var readBytes = stream.Read(buffer, 0, rar5HeaderSize);
                    if (readBytes > 0)
                    {
                        var header = new StringBuilder(rar5HeaderSize * 2);
                        foreach (var value in buffer)
                        {
                            header.AppendFormat("{0:X2}", value); // convert to HEX
                        }
                        isRarArchive = header.ToString().Equals(rar5HeaderSignature);
                    }
                }
            }
            catch (IOException)
            {
                isRarArchive = false;
            }
            return isRarArchive;
        }

        /// <summary>
        /// Checks whether the specified file is a RAR4 or RAR5 archive.
        /// </summary>
        /// <param name="file">The archive to be checked.</param>
        /// <param name="password">The password of the file if encrypted.</param>
        /// <returns>True if file is RAR4 or RAR5 archive, false otherwise.</returns>
        internal static async Task<bool> IsRarArchive(StorageFile file, string password = null)
        {
            using (var stream = await file.OpenStreamForReadAsync())
            {
                if (!stream.CanSeek)
                {
                    bool isRarArchive;
                    if (!(isRarArchive = IsRarArchive(stream, password)))
                    {
                        // not RAR4, hence check for RAR5 only
                        using (var stream2 = await file.OpenStreamForReadAsync())
                        {
                            isRarArchive = IsRarArchive(stream2, password, true);
                        }
                    }

                    return isRarArchive;
                }

                return IsRarArchive(stream, password);
            }
        }
    }
}
