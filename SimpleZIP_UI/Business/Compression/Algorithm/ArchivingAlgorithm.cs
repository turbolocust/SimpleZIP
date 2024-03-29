﻿// ==++==
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
using SharpCompress.Readers;
using SharpCompress.Writers;
using SimpleZIP_UI.Business.Compression.Algorithm.Options;
using SimpleZIP_UI.Business.Compression.Reader;
using SimpleZIP_UI.Business.Streams;
using SimpleZIP_UI.Business.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Serilog;
using SimpleZIP_UI.Business.Compression.Algorithm.Factory;

namespace SimpleZIP_UI.Business.Compression.Algorithm
{
    /// <inheritdoc />
    /// <summary>
    /// Offers archiving operations using SharpCompress' Reader and Writer API.
    /// </summary>
    public abstract class ArchivingAlgorithm : AbstractAlgorithm
    {
        private readonly ILogger _logger = Log.ForContext<ArchivingAlgorithm>();

        /// <summary>
        /// The concrete archive type to be used.
        /// </summary>
        private readonly ArchiveType _type;

        /// <inheritdoc />
        protected ArchivingAlgorithm(ArchiveType type, AlgorithmOptions options) : base(options)
        {
            _type = type;
            Token = CancellationToken.None;
        }

        /// <inheritdoc />
        public override async Task DecompressAsync(
            StorageFile archive,
            StorageFolder location,
            IDecompressionOptions options = null)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (location == null) throw new ArgumentNullException(nameof(location));

            var readerOptions = options != null
                ? ConvertDecompressionOptions(options)
                : new ReaderOptions
                {
                    LeaveStreamOpen = false,
                    ArchiveEncoding = ConvertEncoding(GetDefaultEncoding())
                };

            try
            {
                var writeInfo = new WriteEntryInfo { Location = location, IgnoreDirectories = false };

                using (var archiveStream = await archive.OpenStreamForReadAsync().ConfigureAwait(false))
                using (var reader = ReaderFactory.Open(archiveStream, readerOptions))
                {
                    writeInfo.Reader = reader;
                    while (reader.MoveToNextEntry())
                    {
                        Token.ThrowIfCancellationRequested();
                        if (!reader.Entry.IsDirectory)
                        {
                            await WriteEntry(writeInfo).ConfigureAwait(false);
                        }
                    }
                }

                FlushBytesProcessedBuffer();
            }
            catch (CryptographicException ex)
            {
                _logger.Error(ex, "Decompression of archive {ArchiveName} failed", archive.Name);
                throw new ArchiveEncryptedException(ex.Message, ex);
            }
        }

        /// <inheritdoc />
        public override async Task DecompressAsync(
            StorageFile archive,
            StorageFolder location,
            IReadOnlyList<IArchiveEntry> entries,
            bool collectFileNames,
            IDecompressionOptions options = null)
        {
            await DecompressEntries(archive, location, entries, collectFileNames, options).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override async Task CompressAsync(
            IReadOnlyList<StorageFile> files,
            StorageFile archive,
            StorageFolder location,
            ICompressionOptions options = null)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (location == null) throw new ArgumentNullException(nameof(location));

            if (files.IsNullOrEmpty()) return; // nothing to do

            var writerOptions = ConvertCompressionOptions(options);

            using (var archiveStream = await archive.OpenStreamForWriteAsync().ConfigureAwait(false))
            using (var writer = WriterFactory.Open(archiveStream, _type, writerOptions))
            {
                foreach (var file in files)
                {
                    Token.ThrowIfCancellationRequested();
                    using (var inputStream = await file.OpenStreamForReadAsync().ConfigureAwait(false))
                    using (var progressStream = new ProgressObservableStream(this, inputStream))
                    {
                        await writer.WriteAsync(file.Name, progressStream, Token).ConfigureAwait(false);
                    }
                }
            }

            FlushBytesProcessedBuffer();
        }

        #region Private Members

        private async Task DecompressEntries(
            IStorageFile archive,
            StorageFolder location,
            IReadOnlyCollection<IArchiveEntry> entries,
            bool collectFileNames,
            IDecompressionOptions options = null)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (location == null) throw new ArgumentNullException(nameof(location));

            if (entries.IsNullOrEmpty()) return; // nothing to do

            int processedEntries = 0; // to count number of found entries
            var entriesMap = ConvertToMap(entries); // for faster access

            var readerOptions = options != null
                ? ConvertDecompressionOptions(options)
                : new ReaderOptions
                {
                    LeaveStreamOpen = false,
                    ArchiveEncoding = ConvertEncoding(GetDefaultEncoding())
                };

            var writeInfo = new WriteEntryInfo
            {
                Location = location,
                IgnoreDirectories = true
            };

            try
            {
                using (var archiveStream = await archive.OpenStreamForReadAsync().ConfigureAwait(false))
                using (var reader = ReaderFactory.Open(archiveStream, readerOptions))
                {
                    writeInfo.Reader = reader;
                    while (reader.MoveToNextEntry())
                    {
                        Token.ThrowIfCancellationRequested();
                        string key = Archives.NormalizeName(reader.Entry.Key);
                        if (entriesMap.ContainsKey(key))
                        {
                            if (collectFileNames)
                            {
                                string fileName = await WriteEntry(writeInfo).ConfigureAwait(false);
                                var entry = entriesMap[key];
                                entry.FileName = fileName; // save name
                            }
                            else
                            {
                                await WriteEntry(writeInfo).ConfigureAwait(false);
                            }

                            ++processedEntries;
                        }

                        if (processedEntries == entries.Count) break;
                    }
                }

                FlushBytesProcessedBuffer();
            }
            catch (CryptographicException ex)
            {
                _logger.Error(ex, "Decompression of archive {ArchiveName} failed", archive.Name);
                throw new ArchiveEncryptedException(ex.Message, ex);
            }
        }

        private async Task<string> WriteEntry(WriteEntryInfo info)
        {
            string fileName;

            using (var entryStream = info.Reader.OpenEntryStream())
            {
                StorageFile file;
                if (info.IgnoreDirectories)
                {
                    string name = Path.GetFileName(info.Reader.Entry.Key);
                    file = await info.Location.CreateFileAsync(
                        name, CreationCollisionOption.GenerateUniqueName);
                }
                else
                {
                    file = await FileUtils.CreateFileAsync(info.Location,
                        info.Reader.Entry.Key).ConfigureAwait(false);
                }


                if (file == null) return null; // could not be created
                fileName = file.Path; // use path in case of sub-directories

                using (var outputStream = await file.OpenStreamForWriteAsync().ConfigureAwait(false))
                {
                    var buffer = new byte[BufferSize];
                    int readBytes;
                    while ((readBytes = entryStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        await outputStream.WriteAsync(buffer, 0, readBytes, Token).ConfigureAwait(false);
                        Update(readBytes);
                    }

                    await outputStream.FlushAsync(Token).ConfigureAwait(false);
                }
            }

            return fileName;
        }

        private struct WriteEntryInfo
        {
            internal IReader Reader { get; set; }
            internal StorageFolder Location { get; set; }
            internal bool IgnoreDirectories { get; set; }
        }

        private ArchiveEncoding ConvertEncoding(Encoding encoding)
        {
            var defaultEncoding = encoding ?? GetDefaultEncoding();
            return new ArchiveEncoding { Default = defaultEncoding, Password = defaultEncoding };
        }

        private ReaderOptions ConvertDecompressionOptions(IDecompressionOptions options)
        {
            return new ReaderOptions
            {
                ArchiveEncoding = ConvertEncoding(options.ArchiveEncoding),
                Password = options.Password
            };
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private WriterOptions ConvertCompressionOptions(ICompressionOptions options)
        {
            var writerOptions = GetWriterOptions();
            writerOptions.LeaveStreamOpen = false;

            var encoding = options != null ? options.ArchiveEncoding : GetDefaultEncoding();
            writerOptions.ArchiveEncoding = ConvertEncoding(encoding);

            return writerOptions;
        }

        #endregion

        /// <summary>
        /// Returns the writer instance with the default fallback compression type.
        /// </summary>
        /// <returns>The writer options instance for the corresponding algorithm.</returns>
        protected abstract WriterOptions GetWriterOptions();
    }
}
