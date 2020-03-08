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

using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Streams;
using SimpleZIP_UI.Application.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Algorithm
{
    /// <inheritdoc />
    /// <summary>
    /// Offers archiving operations using SharpCompress' Reader and Writer API.
    /// </summary>
    public abstract class ArchivingAlgorithm : AbstractAlgorithm
    {
        /// <summary>
        /// The concrete algorithm to be used.
        /// </summary>
        private readonly ArchiveType _type;

        /// <inheritdoc />
        protected ArchivingAlgorithm(ArchiveType type)
        {
            _type = type;
            Token = CancellationToken.None;
        }

        /// <inheritdoc />
        public override async Task<Stream> Decompress(StorageFile archive,
            StorageFolder location, ReaderOptions options = null)
        {
            if (archive == null || location == null) return Stream.Null;

            options = options ?? new ReaderOptions
            {
                LeaveStreamOpen = false,
                ArchiveEncoding = GetDefaultEncoding()
            };

            var archiveStream = Stream.Null;
            long totalBytesWritten = 0; // for accurate progress update

            try
            {
                archiveStream = await archive.OpenStreamForReadAsync().ConfigureAwait(false);
                using (var reader = ReaderFactory.Open(archiveStream, options))
                {
                    while (reader.MoveToNextEntry())
                    {
                        Token.ThrowIfCancellationRequested();
                        if (!reader.Entry.IsDirectory)
                        {
                            (_, totalBytesWritten) = await WriteEntry(reader, location,
                                totalBytesWritten, false).ConfigureAwait(false);
                        }
                    }
                }
            }
            finally
            {
                if (!options.LeaveStreamOpen)
                {
                    archiveStream.Dispose();
                }
            }

            return archiveStream;
        }

        /// <inheritdoc />
        public override async Task<Stream> Decompress(StorageFile archive, StorageFolder location,
            IReadOnlyList<FileEntry> entries, ReaderOptions options = null)
        {
            return await DecompressEntries(archive, location,
                entries, false, options).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override async Task<Stream> Decompress(StorageFile archive, StorageFolder location,
            IReadOnlyList<FileEntry> entries, bool collectFileNames, ReaderOptions options = null)
        {
            return await DecompressEntries(archive, location,
                entries, collectFileNames, options).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public override async Task<Stream> Compress(IReadOnlyList<StorageFile> files, StorageFile archive,
            StorageFolder location, WriterOptions options = null)
        {
            if (files.IsNullOrEmpty() | archive == null | location == null) return Stream.Null;

            if (options == null)
            {
                options = GetWriterOptions();
                options.LeaveStreamOpen = false;
                options.ArchiveEncoding = GetDefaultEncoding();
            }

            var archiveStream = Stream.Null;
            var progressStream = Stream.Null;
            try
            {
                archiveStream = await archive.OpenStreamForWriteAsync().ConfigureAwait(false);
                progressStream = new ProgressObservableStream(this, archiveStream);

                using (var writer = WriterFactory.Open(progressStream, _type, options))
                {
                    foreach (var file in files)
                    {
                        Token.ThrowIfCancellationRequested();
                        using (var inputStream = await file.OpenStreamForReadAsync().ConfigureAwait(false))
                        {
                            await writer.WriteAsync(file.Name, inputStream, Token).ConfigureAwait(false);
                        }
                    }
                }
            }
            finally
            {
                if (!options.LeaveStreamOpen)
                {
                    archiveStream.Dispose();
                    progressStream.Dispose();
                }
            }

            return progressStream;
        }

        private async Task<Stream> DecompressEntries(IStorageFile archive, StorageFolder location,
            IReadOnlyCollection<FileEntry> entries, bool collectFileNames, ReaderOptions options = null)
        {
            if (archive == null || entries.IsNullOrEmpty() || location == null) return Stream.Null;

            int processedEntries = 0; // to count number of found entries
            var entriesMap = ConvertToMap(entries); // for faster access
            long totalBytesWritten = 0; // for accurate progress update
            var archiveStream = Stream.Null;

            options = options ?? new ReaderOptions
            {
                LeaveStreamOpen = false,
                ArchiveEncoding = GetDefaultEncoding()
            };

            try
            {
                archiveStream = await archive.OpenStreamForReadAsync().ConfigureAwait(false);
                using (var reader = ReaderFactory.Open(archiveStream, options))
                {
                    while (reader.MoveToNextEntry())
                    {
                        Token.ThrowIfCancellationRequested();
                        if (entriesMap.ContainsKey(reader.Entry.Key))
                        {
                            if (collectFileNames)
                            {
                                string fileName;
                                (fileName, totalBytesWritten) = await WriteEntry(reader, location,
                                    totalBytesWritten, true).ConfigureAwait(false);
                                var entry = entriesMap[reader.Entry.Key];
                                entry.FileName = fileName; // save name
                            }
                            else
                            {
                                (_, totalBytesWritten) = await WriteEntry(reader, location,
                                    totalBytesWritten, true).ConfigureAwait(false);
                            }

                            ++processedEntries;
                        }

                        if (processedEntries == entries.Count) break;
                    }
                }
            }
            finally
            {
                if (!options.LeaveStreamOpen)
                {
                    archiveStream.Dispose();
                }
            }

            return archiveStream;
        }

        private static IDictionary<string, FileEntry> ConvertToMap(
            IReadOnlyCollection<FileEntry> entries)
        {
            int mapSize = entries.Count * 2;
            var map = new Dictionary<string, FileEntry>(mapSize);

            foreach (var entry in entries)
            {
                map.Add(entry.Id, entry);
            }

            return map;
        }

        private async Task<(string, long)> WriteEntry(IReader reader,
            StorageFolder location, long totalBytesWritten, bool ignoreDirectories)
        {
            string fileName;

            using (var entryStream = reader.OpenEntryStream())
            {
                StorageFile file;
                if (ignoreDirectories)
                {
                    string name = Path.GetFileName(reader.Entry.Key);
                    file = await location.CreateFileAsync(
                        name, CreationCollisionOption.GenerateUniqueName);
                }
                else
                {
                    file = await FileUtils.CreateFileAsync(location, reader.Entry.Key).ConfigureAwait(false);
                }

                if (file == null) return (null, totalBytesWritten); // file could not be created
                fileName = file.Path; // use path in case of sub-directories

                using (var outputStream = await file.OpenStreamForWriteAsync().ConfigureAwait(false))
                {
                    var bytes = new byte[DefaultBufferSize];
                    int readBytes;
                    while ((readBytes = entryStream.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        await outputStream.WriteAsync(bytes, 0, readBytes, Token).ConfigureAwait(false);
                        totalBytesWritten += readBytes;
                        Update(totalBytesWritten);
                    }

                    await outputStream.FlushAsync().ConfigureAwait(false);
                }
            }

            return (fileName, totalBytesWritten);
        }

        /// <summary>
        /// Returns the writer instance with the default fallback compression type.
        /// </summary>
        /// <returns>The writer options instance for the corresponding algorithm.</returns>
        protected abstract WriterOptions GetWriterOptions();
    }
}
