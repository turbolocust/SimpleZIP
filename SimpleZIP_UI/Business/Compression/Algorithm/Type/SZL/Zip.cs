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

using ICSharpCode.SharpZipLib.Zip;
using SimpleZIP_UI.Business.Compression.Algorithm.Options;
using SimpleZIP_UI.Business.Compression.Reader;
using SimpleZIP_UI.Business.Streams;
using SimpleZIP_UI.Business.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Serilog;
using SimpleZIP_UI.Business.Compression.Algorithm.Factory;
using System.Text;

namespace SimpleZIP_UI.Business.Compression.Algorithm.Type.SZL
{
    /// <inheritdoc />
    /// <summary>
    /// Implements <see cref="ICompressionAlgorithm"/> and offers
    /// compression and extraction of ZIP files. This class was
    /// introduced because it's more reliable than SharpCompress.
    /// </summary>
    internal class Zip : AbstractAlgorithm
    {
        private readonly ILogger _logger = Log.ForContext<Zip>();

        /// <inheritdoc />
        public Zip(AlgorithmOptions options) : base(options)
        {
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
            if (options == null) options = new CompressionOptions(GetDefaultEncoding());

            var stringCodec = StringCodec.FromCodePage(options.ArchiveEncoding.CodePage);

            using (var archiveStream = await archive.OpenStreamForWriteAsync().ConfigureAwait(false))
            using (var progressStream = new ProgressObservableStream(this, archiveStream))
            using (var zipStream = new ZipOutputStream(progressStream, stringCodec))
            {
                zipStream.UseZip64 = UseZip64.Dynamic;
                zipStream.SetLevel(6); // default

                foreach (var file in files)
                {
                    Token.ThrowIfCancellationRequested();
                    ulong size = await FileUtils.GetFileSizeAsync(file).ConfigureAwait(false);
                    var properties = await file.GetBasicPropertiesAsync();

                    var zipEntry = new ZipEntry(file.Name)
                    {
                        DateTime = properties.DateModified.DateTime,
                        CompressionMethod = CompressionMethod.Deflated,
                        Size = (long)size
                    };

                    zipStream.PutNextEntry(zipEntry);
                    var buffer = new byte[BufferSize];

                    using (var fileStream = await file.OpenStreamForReadAsync().ConfigureAwait(false))
                    {
                        int readBytes;
                        while ((readBytes = await fileStream
                            .ReadAsync(buffer, 0, buffer.Length, Token)
                            .ConfigureAwait(false)) > 0)
                        {
                            await zipStream.WriteAsync(buffer, 0, readBytes, Token).ConfigureAwait(false);
                            Update(readBytes);
                        }

                        await zipStream.FlushAsync(Token).ConfigureAwait(false);
                    }

                    zipStream.CloseEntry();
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
            if (options == null) options = new DecompressionOptions(GetDefaultEncoding());

            try
            {
                var writeInfo = new WriteEntryInfo { Location = location, IgnoreDirectories = false };

                using (var archiveStream = await archive.OpenStreamForReadAsync().ConfigureAwait(false))
                using (var zipFile = new ZipFile(archiveStream))
                {
                    writeInfo.Zip = zipFile;
                    zipFile.Password = options.Password;
                    zipFile.StringCodec = StringCodec.FromCodePage(options.ArchiveEncoding.CodePage);

                    foreach (ZipEntry zipEntry in zipFile)
                    {
                        Token.ThrowIfCancellationRequested();
                        if (!zipEntry.IsDirectory)
                        {
                            writeInfo.Entry = zipEntry;
                            await WriteEntry(writeInfo).ConfigureAwait(false);
                        }
                    }
                }

                FlushBytesProcessedBuffer();
            }
            catch (ICSharpCode.SharpZipLib.SharpZipBaseException ex)
            {
                _logger.Error(ex, "Decompression of {ArchiveName} failed", archive.Name);

                const string noPasswordPrefix = "No password available"; // is unit tested
                if (!ex.Message.Contains(noPasswordPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    throw; // non-security related exception (likely)
                }

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

        #region Private Members

        private async Task DecompressEntries(IStorageFile archive, StorageFolder location,
            IReadOnlyCollection<IArchiveEntry> entries, bool collectFileNames, IDecompressionOptions options = null)
        {
            if (archive == null) throw new ArgumentNullException(nameof(archive));
            if (location == null) throw new ArgumentNullException(nameof(location));

            if (entries.IsNullOrEmpty()) return; // nothing to do
            if (options == null) options = new DecompressionOptions(GetDefaultEncoding());

            try
            {
                var writeInfo = new WriteEntryInfo { Location = location, IgnoreDirectories = true };

                using (var archiveStream = await archive.OpenStreamForReadAsync().ConfigureAwait(false))
                using (var zipFile = new ZipFile(archiveStream))
                {
                    writeInfo.Zip = zipFile;
                    zipFile.Password = options.Password;
                    zipFile.StringCodec = StringCodec.FromCodePage(options.ArchiveEncoding.CodePage);

                    foreach (var entry in entries)
                    {
                        Token.ThrowIfCancellationRequested();
                        string key = Archives.NormalizeName(entry.Key);
                        var zipEntry = zipFile.GetEntry(key);

                        if (zipEntry == null)
                        {
                            const string msg = "Entry {0} does not exist in archive";
                            throw new ReadingArchiveException(string.Format(CultureInfo.CurrentCulture, msg, key));
                        }

                        writeInfo.Entry = zipEntry;

                        if (collectFileNames)
                        {
                            string fileName = await WriteEntry(writeInfo).ConfigureAwait(false);
                            entry.FileName = fileName; // save name
                        }
                        else
                        {
                            await WriteEntry(writeInfo).ConfigureAwait(false);
                        }
                    }
                }

                FlushBytesProcessedBuffer();
            }
            catch (ICSharpCode.SharpZipLib.SharpZipBaseException ex)
            {
                _logger.Error(ex, "Decompression of {ArchiveName} failed", archive.Name);

                const string noPasswordPrefix = "No password available"; // is unit tested
                if (!ex.Message.Contains(noPasswordPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    throw; // non-security related exception (likely)
                }

                throw new ArchiveEncryptedException(ex.Message, ex);
            }
        }

        private async Task<string> WriteEntry(WriteEntryInfo info)
        {
            string fileName;

            using (var entryStream = info.Zip.GetInputStream(info.Entry))
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
                fileName = file.Path; // use path in case of sub-directories

                using (var outputStream = await file.OpenStreamForWriteAsync().ConfigureAwait(false))
                {
                    var buffer = new byte[BufferSize];
                    int readBytes;
                    while ((readBytes = await entryStream
                        .ReadAsync(buffer, 0, buffer.Length)
                        .ConfigureAwait(false)) > 0)
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
            internal ZipFile Zip { get; set; }
            internal ZipEntry Entry { get; set; }
            internal StorageFolder Location { get; set; }
            internal bool IgnoreDirectories { get; set; }
        }

        #endregion
    }
}