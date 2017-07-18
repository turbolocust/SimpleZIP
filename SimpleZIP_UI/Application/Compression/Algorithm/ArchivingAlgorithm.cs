﻿// ==++==
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Application.Compression.Algorithm
{
    /// <summary>
    /// Offers archiving operations using SharpCompress' Reader and Writer API.
    /// </summary>
    public abstract class ArchivingAlgorithm : ICompressionAlgorithm
    {
        /// <summary>
        /// Default buffer size for streams.
        /// </summary>
        protected const int DefaultBufferSize = 8192;

        /// <summary>
        /// The concrete algorithm to be used.
        /// </summary>
        private readonly ArchiveType _type;

        /// <inheritdoc cref="ICompressionAlgorithm.Token"/>
        public CancellationToken Token { get; set; }

        protected ArchivingAlgorithm(ArchiveType type)
        {
            _type = type;
            Token = CancellationToken.None;
        }

        public virtual async Task<bool> Decompress(StorageFile archive, StorageFolder location,
            ReaderOptions options = null)
        {
            if (archive == null | location == null) return false;

            options = options ?? new ReaderOptions
            {
                LeaveStreamOpen = false
            };

            using (var reader = ReaderFactory.Open(await archive.OpenStreamForReadAsync(), options))
            {
                while (!Token.IsCancellationRequested && reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        await WriteEntry(reader, location);
                    }
                }
            }
            return true;
        }

        public virtual async Task<bool> Decompress(StorageFile archive, StorageFolder location,
            IReadOnlyList<FileEntry> entries, ReaderOptions options = null)
        {
            if (archive == null | entries.IsNullOrEmpty() | location == null) return false;

            options = options ?? new ReaderOptions
            {
                LeaveStreamOpen = false
            };

            using (var reader = ReaderFactory.Open(await archive.OpenStreamForReadAsync(), options))
            {
                while (!Token.IsCancellationRequested && reader.MoveToNextEntry())
                {
                    if (entries.Any(entry => reader.Entry.Key.Equals(entry.Key)))
                    {
                        await WriteEntry(reader, location);
                    }
                }
            }
            return true;
        }

        private async Task<IEntry> WriteEntry(IReader reader, StorageFolder location)
        {
            var entry = reader.Entry;
            var file = await FileUtils.CreateFileAsync(location, entry.Key);
            if (file == null) return null;

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
            return entry;
        }

        public virtual async Task<bool> Compress(IReadOnlyList<StorageFile> files, StorageFile archive,
            StorageFolder location, WriterOptions options = null)
        {
            if (files.IsNullOrEmpty() | archive == null | location == null) return false;

            options = options ?? GetWriterOptions(); // get options if null
            options.LeaveStreamOpen = false;

            using (var writer = WriterFactory.Open(await archive.OpenStreamForWriteAsync(), _type, options))
            {
                foreach (var file in files)
                {
                    if (Token.IsCancellationRequested) break;

                    using (var inputStream = await file.OpenStreamForReadAsync())
                    {
                        await writer.WriteAsync(file.Name, inputStream, Token);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the writer instance with the default fallback compression type.
        /// </summary>
        /// <returns>The writer options instance for the corresponding algorithm.</returns>
        protected abstract WriterOptions GetWriterOptions();
    }
}
