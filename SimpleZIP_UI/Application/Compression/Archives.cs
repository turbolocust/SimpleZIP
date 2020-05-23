// ==++==
// 
// Copyright (C) 2019 Matthias Fussenegger
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

using SharpCompress.Archives.GZip;
using SharpCompress.Archives.Rar;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.LZMA;
using SharpCompress.Readers;
using SimpleZIP_UI.Application.Compression.Algorithm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression
{
    public static class Archives
    {
        /// <summary>
        /// Default separator for entry names.
        /// </summary>
        public const char NameSeparatorChar = '/';

        /// <summary>
        /// Enumeration to identify archive types.
        /// </summary>
        public enum ArchiveType
        {
            Unknown,
            Zip,
            GZip,
            BZip2,
            LZip,
            Tar,
            TarGz,
            TarBz2,
            TarLz,
            Rar
        }

        /// <summary>
        /// Maps file types to archive types. Consists only of 
        /// file types with a single file name extension.
        /// </summary>
        internal static readonly IDictionary<string, ArchiveType> ArchiveFileTypes =
            new Dictionary<string, ArchiveType>(Enum.GetNames(typeof(ArchiveType)).Length * 2)
            {
                {".zip", ArchiveType.Zip},
                {".tar", ArchiveType.Tar},
                {".gzip", ArchiveType.GZip},
                {".gz", ArchiveType.GZip},
                {".tgz", ArchiveType.TarGz},
                {".bz2", ArchiveType.BZip2},
                {".bzip2", ArchiveType.BZip2},
                {".tbz2", ArchiveType.TarBz2},
                {".lz", ArchiveType.LZip},
                {".lzip", ArchiveType.LZip},
                {".lzma", ArchiveType.LZip},
                {".tlz", ArchiveType.TarLz},
                {".rar", ArchiveType.Rar}
            };

        /// <summary>
        /// Maps file types to extended archive types. Consists only of 
        /// file types with multiple file name extensions.
        /// </summary>
        internal static readonly IDictionary<string, ArchiveType> ArchiveExtendedFileTypes =
            new Dictionary<string, ArchiveType>
            {
                {".tar.gz", ArchiveType.TarGz},
                {".tar.gzip", ArchiveType.TarGz},
                {".tar.bz2", ArchiveType.TarBz2},
                {".tar.bzip2", ArchiveType.TarBz2},
                {".tar.lz", ArchiveType.TarLz},
                {".tar.lzip", ArchiveType.TarLz},
                {".tar.lzma", ArchiveType.TarLz}
            };

        /// <summary>
        /// Maps <see cref="SharpCompress.Common.ArchiveType"/> to <see cref="ArchiveType"/>.
        /// </summary>
        private static readonly IDictionary<SharpCompress.Common.ArchiveType, ArchiveType> ArchiveTypes =
            new Dictionary<SharpCompress.Common.ArchiveType, ArchiveType>
            {
                {SharpCompress.Common.ArchiveType.Zip, ArchiveType.Zip},
                {SharpCompress.Common.ArchiveType.Tar, ArchiveType.Tar},
                {SharpCompress.Common.ArchiveType.Rar, ArchiveType.Rar},
                {SharpCompress.Common.ArchiveType.GZip, ArchiveType.GZip}
            };

        /// <summary>
        /// Regular expression used to match drive letter in paths.
        /// </summary>
        private static readonly Regex RegexDriveLetter = new Regex(@"[A-Za-z]{1}:(\/|\\)");

        /// <summary>
        /// Removes drive letters in the specified name and
        /// replaces backslashes with <see cref="NameSeparatorChar"/>.
        /// </summary>
        /// <param name="name">The name to be normalized.</param>
        /// <returns>A normalized name.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the specified name is <c>null</c>.</exception>
        public static string NormalizeName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            string normalized = name.Replace('\\', NameSeparatorChar);
            var match = RegexDriveLetter.Match(normalized);

            if (match.Success)
            {
                normalized = normalized.Replace(match.Value, string.Empty, StringComparison.Ordinal);
            }

            return normalized;
        }

        /// <summary>
        /// Returns the algorithm instance that is mapped to the specified archive type.
        /// </summary>
        /// <param name="value">The enum value of the archive type to be determined.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when archive type matched no algorithm.</exception>
        /// <returns>An instance of the compressor algorithm that is mapped to the specified value.</returns>
        public static ICompressionAlgorithm DetermineAlgorithm(ArchiveType value)
        {
            ICompressionAlgorithm algorithm;

            switch (value)
            {
                case ArchiveType.Zip: // use SharpZipLib
                    algorithm = new Algorithm.Type.SZL.Zip();
                    break;
                case ArchiveType.GZip:
                    algorithm = new Algorithm.Type.GZip();
                    break;
                case ArchiveType.BZip2:
                    algorithm = new Algorithm.Type.BZip2();
                    break;
                case ArchiveType.LZip:
                    algorithm = new Algorithm.Type.LZip();
                    break;
                case ArchiveType.Tar:
                    algorithm = new Algorithm.Type.Tar();
                    break;
                case ArchiveType.TarGz:
                    algorithm = new Algorithm.Type.TarGzip();
                    break;
                case ArchiveType.TarBz2:
                    algorithm = new Algorithm.Type.TarBzip2();
                    break;
                case ArchiveType.TarLz:
                    algorithm = new Algorithm.Type.TarLzip();
                    break;
                case ArchiveType.Rar:
                    algorithm = new Algorithm.Type.Rar();
                    break;
                // ReSharper disable once RedundantCaseLabel
                case ArchiveType.Unknown:
                // fall-through
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }

            return algorithm;
        }

        /// <summary>
        /// Determines the <see cref="ArchiveType"/> of the specified file.
        /// </summary>
        /// <param name="file">The archive to be checked.</param>
        /// <returns>A task which returns the determined <see cref="ArchiveType"/>.</returns>
        /// <exception cref="InvalidArchiveTypeException">
        /// Thrown if archive type is not supported.</exception>
        public static async Task<ArchiveType> DetermineArchiveType(StorageFile file)
        {
            var archiveType = ArchiveType.Unknown;

            try
            {
                bool seekableStream;
                // let SharpCompress' reader do the archive detection
                using (var stream = await file.OpenStreamForReadAsync().ConfigureAwait(false))
                using (var reader = ReaderFactory.Open(stream))
                {
                    seekableStream = stream.CanSeek;
                    if (ArchiveTypes.TryGetValue(reader.ArchiveType, out var localType))
                    {
                        archiveType = localType;
                    }
                }

                // check for compressed TAR archive
                if (archiveType == ArchiveType.Tar && seekableStream)
                {
                    using (var stream = await file.OpenStreamForReadAsync().ConfigureAwait(false))
                    {
                        bool determined = false;
                        if (GZipArchive.IsGZipFile(stream))
                        {
                            determined = true;
                            archiveType = ArchiveType.TarGz;
                        }

                        if (!determined)
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            determined = BZip2Stream.IsBZip2(stream);
                            archiveType = ArchiveType.TarBz2;
                        }

                        if (!determined)
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            determined = LZipStream.IsLZipFile(stream);
                            archiveType = ArchiveType.TarLz;
                        }

                        if (!determined) // leave as TAR
                        {
                            archiveType = ArchiveType.Tar;
                        }
                    }
                }
            }
            catch (Exception ex) // due to missing documentation in SharpCompress
            {
                throw new InvalidArchiveTypeException(I18N
                    .Resources.GetString("UnknownArchiveType/Text"), ex);
            }

            return archiveType;
        }

        /// <summary>
        /// Determines the <see cref="ArchiveType"/> of the specified file
        /// only considering its filename extension. If the type cannot be
        /// determined then <see cref="ArchiveType.Unknown"/> is returned.
        /// </summary>
        /// <param name="ext">The filename extension of the archive.</param>
        /// <returns>The determined <see cref="ArchiveType"/>.</returns>
        public static ArchiveType DetermineArchiveTypeByFileExtension(string ext)
        {
            bool isArchive;
            if (!(isArchive = ArchiveFileTypes.TryGetValue(ext, out var archiveType)))
            {
                isArchive = ArchiveExtendedFileTypes.TryGetValue(ext, out archiveType);
            }

            return isArchive ? archiveType : ArchiveType.Unknown;
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
        /// <returns>True if archive is RAR, false otherwise.</returns>
        private static bool IsRarArchive(Stream stream, string password = null, bool rar5Only = false)
        {
            const int rar5HeaderSize = 8; // bytes
            const string rar5HeaderSignature = "526172211A070100";

            bool isRarArchive = false;
            try
            {
                if (!rar5Only)
                {
                    isRarArchive = RarArchive.IsRarFile(stream,
                        new ReaderOptions { Password = password, LeaveStreamOpen = true });
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
                        string header = BitConverter.ToString(buffer).Replace("-", string.Empty, StringComparison.Ordinal);
                        isRarArchive = header.Equals(rar5HeaderSignature, StringComparison.Ordinal);
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
            using (var stream = await file.OpenStreamForReadAsync().ConfigureAwait(false))
            {
                if (!stream.CanSeek)
                {
                    bool isRarArchive;
                    if (!(isRarArchive = IsRarArchive(stream, password)))
                    {
                        // not RAR4, hence check for RAR5 only
                        using (var stream2 = await file.OpenStreamForReadAsync().ConfigureAwait(false))
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