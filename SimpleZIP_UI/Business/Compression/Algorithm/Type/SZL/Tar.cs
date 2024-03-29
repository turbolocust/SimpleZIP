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

using ICSharpCode.SharpZipLib.Tar;
using SimpleZIP_UI.Business.Compression.Algorithm.Options;
using SimpleZIP_UI.Business.Compression.Reader;
using SimpleZIP_UI.Business.Streams;
using SimpleZIP_UI.Business.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleZIP_UI.Business.Compression.Algorithm.Factory;

namespace SimpleZIP_UI.Business.Compression.Algorithm.Type.SZL
{
    /// <inheritdoc />
    /// <summary>
    /// Implements <see cref="ICompressionAlgorithm"/> and offers
    /// compression and extraction of TAR files. This class was
    /// introduced because it's more reliable than SharpCompress.
    /// </summary>
    internal class Tar : AbstractAlgorithm
    {
        /// <inheritdoc />
        public Tar(AlgorithmOptions options) : base(options)
        {
        }

        /// <summary>
        /// May be overridden by subclasses to provide an additional
        /// compressor input stream if entries are compressed.
        /// </summary>
        /// <param name="stream">The archive stream.</param>
        /// <returns>The compressor input stream.</returns>
        protected virtual Stream GetCompressorInputStream(Stream stream)
        {
            return stream;
        }

        /// <summary>
        /// May be overridden by subclasses to provide an additional
        /// compressor output stream if entries are to be compressed.
        /// </summary>
        /// <param name="stream">The archive stream.</param>
        /// <returns>The compressor output stream.</returns>
        protected virtual Stream GetCompressorOutputStream(Stream stream)
        {
            return stream;
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
            if (options == null) options = new CompressionOptions(Encoding.UTF8);

            using (var archiveStream = await archive.OpenStreamForWriteAsync().ConfigureAwait(false))
            using (var progressStream = new ProgressObservableStream(this, archiveStream))
            using (var compressorStream = GetCompressorOutputStream(progressStream))
            using (var tarStream = new TarOutputStream(compressorStream, options.ArchiveEncoding))
            {
                foreach (var file in files)
                {
                    Token.ThrowIfCancellationRequested();
                    ulong size = await FileUtils.GetFileSizeAsync(file).ConfigureAwait(false);
                    var properties = await file.GetBasicPropertiesAsync();

                    var tarEntry = TarEntry.CreateTarEntry(file.Name);
                    tarEntry.ModTime = properties.DateModified.DateTime;
                    tarEntry.Size = (long)size;
                    tarEntry.TarHeader.DevMajor = 0;
                    tarEntry.TarHeader.DevMinor = 0;
                    tarEntry.TarHeader.Mode = 33216; // magic number for UNIX security access
                    tarEntry.TarHeader.LinkName = string.Empty;
                    tarEntry.TarHeader.TypeFlag = TarHeader.LF_NORMAL;

                    tarStream.PutNextEntry(tarEntry);
                    var buffer = new byte[BufferSize];

                    using (var fileStream = await file.OpenStreamForReadAsync().ConfigureAwait(false))
                    {
                        int readBytes;
                        while ((readBytes = await fileStream
                            .ReadAsync(buffer, 0, buffer.Length, Token)
                            .ConfigureAwait(false)) > 0)
                        {
                            await tarStream.WriteAsync(buffer, 0, readBytes, Token).ConfigureAwait(false);
                            Update(readBytes);
                        }

                        await tarStream.FlushAsync().ConfigureAwait(false);
                    }

                    tarStream.CloseEntry();
                }
            }

            FlushBytesProcessedBuffer();
        }

        /// <inheritdoc />
        public override async Task DecompressAsync(
            StorageFile archive,
            StorageFolder location,
            IDecompressionOptions options = null)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (location == null) throw new ArgumentNullException(nameof(location));

            var writeInfo = new WriteEntryInfo { Location = location, IgnoreDirectories = false };
            if (options == null) options = new DecompressionOptions(Encoding.UTF8);

            using (var archiveStream = await archive.OpenStreamForReadAsync().ConfigureAwait(false))
            using (var compressorStream = GetCompressorInputStream(archiveStream))
            using (var tarStream = new TarInputStream(compressorStream, options.ArchiveEncoding))
            {
                TarEntry entry;
                writeInfo.TarStream = tarStream;
                while ((entry = tarStream.GetNextEntry()) != null)
                {
                    Token.ThrowIfCancellationRequested();
                    if (!entry.IsDirectory)
                    {
                        writeInfo.Entry = entry;
                        await WriteEntry(writeInfo).ConfigureAwait(false);
                    }
                }
            }

            FlushBytesProcessedBuffer();
        }

        /// <inheritdoc />
        public override async Task DecompressAsync(
            StorageFile archive,
            StorageFolder location,
            IReadOnlyList<IArchiveEntry> entries,
            bool collectFileNames,
            IDecompressionOptions options = null)
        {
            if (options == null) options = new DecompressionOptions(Encoding.UTF8);
            await DecompressEntries(archive, location, entries, collectFileNames, options).ConfigureAwait(false);
        }

        #region Private Members

        private async Task DecompressEntries(IStorageFile archive, StorageFolder location,
            IReadOnlyCollection<IArchiveEntry> entries, bool collectFileNames, IOptions options)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (location == null) throw new ArgumentNullException(nameof(location));

            if (entries.IsNullOrEmpty()) return; // nothing to do

            int processedEntries = 0; // to count number of found entries
            var entriesMap = ConvertToMap(entries); // for faster access
            var writeInfo = new WriteEntryInfo { Location = location, IgnoreDirectories = true };

            using (var archiveStream = await archive.OpenStreamForReadAsync().ConfigureAwait(false))
            using (var compressorStream = GetCompressorInputStream(archiveStream))
            using (var tarStream = new TarInputStream(compressorStream, options.ArchiveEncoding))
            {
                TarEntry tarEntry;
                writeInfo.TarStream = tarStream;
                while ((tarEntry = tarStream.GetNextEntry()) != null)
                {
                    Token.ThrowIfCancellationRequested();
                    string key = Archives.NormalizeName(tarEntry.Name);
                    if (entriesMap.ContainsKey(key))
                    {
                        writeInfo.Entry = tarEntry;

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

        private async Task<string> WriteEntry(WriteEntryInfo info)
        {
            StorageFile file;
            if (info.IgnoreDirectories)
            {
                string name = Path.GetFileName(info.Entry.Name);
                file = await info.Location.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
            }
            else
            {
                file = await FileUtils.CreateFileAsync(info.Location, info.Entry.Name).ConfigureAwait(false);
            }


            if (file == null) return null; // file could not be created
            string fileName = file.Path; // use path in case of sub-directories

            using (var outputStream = await file.OpenStreamForWriteAsync().ConfigureAwait(false))
            {
                var buffer = new byte[BufferSize];
                int readBytes;
                while ((readBytes = await info.TarStream
                    .ReadAsync(buffer, 0, buffer.Length, Token)
                    .ConfigureAwait(false)) > 0)
                {
                    await outputStream.WriteAsync(buffer, 0, readBytes, Token).ConfigureAwait(false);
                    Update(readBytes);
                }

                await outputStream.FlushAsync().ConfigureAwait(false);
            }

            return fileName;
        }

        private struct WriteEntryInfo
        {
            internal Stream TarStream { get; set; }
            internal TarEntry Entry { get; set; }
            internal StorageFolder Location { get; set; }
            internal bool IgnoreDirectories { get; set; }
        }

        #endregion
    }
}