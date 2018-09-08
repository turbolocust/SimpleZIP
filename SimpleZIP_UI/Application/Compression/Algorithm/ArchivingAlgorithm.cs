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
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Streams;
using SimpleZIP_UI.Application.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public override async Task<Stream> Decompress(StorageFile archive, StorageFolder location,
            ReaderOptions options = null)
        {
            if (archive == null || location == null) return Stream.Null;

            options = options ?? new ReaderOptions { LeaveStreamOpen = false };
            var progressStream = Stream.Null;
            try
            {
                var archiveStream = await archive.OpenStreamForReadAsync();
                progressStream = new ProgressObservableStream(this, archiveStream);

                using (var reader = ReaderFactory.Open(progressStream, options))
                {
                    while (reader.MoveToNextEntry())
                    {
                        Token.ThrowIfCancellationRequested();
                        if (!reader.Entry.IsDirectory)
                        {
                            await WriteEntry(reader, location);
                        }
                    }
                }
            }
            finally
            {
                if (!options.LeaveStreamOpen)
                {
                    progressStream.Dispose();
                }
            }
            return progressStream;
        }

        /// <inheritdoc />
        public override async Task<Stream> Decompress(StorageFile archive, StorageFolder location,
            IReadOnlyList<FileEntry> entries, ReaderOptions options = null)
        {
            if (archive == null || entries.IsNullOrEmpty() || location == null) return Stream.Null;

            var entriesSet = new HashSet<string>(entries.Select(entry => entry.Id).ToArray());

            options = options ?? new ReaderOptions { LeaveStreamOpen = false };
            var progressStream = Stream.Null;
            try
            {
                var archiveStream = await archive.OpenStreamForReadAsync();
                progressStream = new ProgressObservableStream(this, archiveStream);

                using (var reader = ReaderFactory.Open(progressStream, options))
                {
                    while (reader.MoveToNextEntry())
                    {
                        Token.ThrowIfCancellationRequested();
                        if (entriesSet.Contains(reader.Entry.Key))
                        {
                            await WriteEntry(reader, location);
                            entriesSet.Remove(reader.Entry.Key);
                        }

                        if (entriesSet.IsEmpty()) break;
                    }
                }
            }
            finally
            {
                if (!options.LeaveStreamOpen)
                {
                    progressStream.Dispose();
                }
            }
            return progressStream;
        }

        private async Task WriteEntry(IReader reader, StorageFolder location)
        {
            var entry = reader.Entry;
            var file = await FileUtils.CreateFileAsync(location, entry.Key);
            if (file == null) return; // file could not be created

            using (var entryStream = reader.OpenEntryStream())
            {
                using (var outputStream = await file.OpenStreamForWriteAsync())
                {
                    var bytes = new byte[DefaultBufferSize];
                    int readBytes;
                    while ((readBytes = entryStream.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        await outputStream.WriteAsync(bytes, 0, readBytes, Token);
                    }
                }
            }
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
            }

            var progressStream = Stream.Null;
            try
            {
                var archiveStream = await archive.OpenStreamForWriteAsync();
                progressStream = new ProgressObservableStream(this, archiveStream);

                using (var writer = WriterFactory.Open(progressStream, _type, options))
                {
                    foreach (var file in files)
                    {
                        Token.ThrowIfCancellationRequested();
                        using (var inputStream = await file.OpenStreamForReadAsync())
                        {
                            await writer.WriteAsync(file.Name, inputStream, Token);
                        }
                    }
                }
            }
            finally
            {
                if (!options.LeaveStreamOpen)
                {
                    progressStream.Dispose();
                }
            }
            return progressStream;
        }

        /// <summary>
        /// Returns the writer instance with the default fallback compression type.
        /// </summary>
        /// <returns>The writer options instance for the corresponding algorithm.</returns>
        protected abstract WriterOptions GetWriterOptions();
    }
}
