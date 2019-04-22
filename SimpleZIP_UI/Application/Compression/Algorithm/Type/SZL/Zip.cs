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

using ICSharpCode.SharpZipLib.Zip;
using SimpleZIP_UI.Application.Compression.Algorithm.Options;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Streams;
using SimpleZIP_UI.Application.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type.SZL
{
    /// <inheritdoc />
    /// <summary>
    /// Implements <see cref="ICompressionAlgorithm"/> and offers
    /// compression and extraction of ZIP files. This class was
    /// introduced because it's more reliable than SharpCompress.
    /// </summary>
    internal class Zip : AbstractAlgorithm
    {
        /// <inheritdoc />
        public override async Task<Stream> Compress(IReadOnlyList<StorageFile> files,
            StorageFile archive, StorageFolder location, ICompressionOptions options = null)
        {
            if (files.IsNullOrEmpty() | archive == null | location == null) return Stream.Null;

            if (options == null)
            {
                options = new CompressionOptions(false, GetDefaultEncoding());
            }

            var progressStream = Stream.Null;
            long totalBytesWritten = 0;

            try
            {
                ZipStrings.CodePage = options.ArchiveEncoding.CodePage;
                var archiveStream = await archive.OpenStreamForWriteAsync();
                progressStream = new ProgressObservableStream(this, archiveStream);

                using (var zipStream = new ZipOutputStream(progressStream))
                {
                    zipStream.UseZip64 = UseZip64.Dynamic;
                    zipStream.SetLevel(6); // default

                    foreach (var file in files)
                    {
                        Token.ThrowIfCancellationRequested();
                        ulong size = await FileUtils.GetFileSizeAsync(file);
                        var properties = await file.GetBasicPropertiesAsync();

                        var zipEntry = new ZipEntry(file.Name)
                        {
                            DateTime = properties.DateModified.DateTime,
                            CompressionMethod = CompressionMethod.Deflated,
                            Size = (long)size
                        };

                        zipStream.PutNextEntry(zipEntry);
                        var buffer = new byte[DefaultBufferSize];

                        using (var fileStream = await file.OpenStreamForReadAsync())
                        {
                            int readBytes;
                            while ((readBytes = await fileStream.ReadAsync(
                                       buffer, 0, buffer.Length, Token)) > 0)
                            {
                                await zipStream.WriteAsync(buffer, 0, readBytes, Token);
                                totalBytesWritten += readBytes;
                                Update(totalBytesWritten);
                            }

                            await zipStream.FlushAsync(Token);
                        }

                        zipStream.CloseEntry();
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
        public override async Task<Stream> Decompress(StorageFile archive,
            StorageFolder location, IDecompressionOptions options = null)
        {
            if (archive == null || location == null) return Stream.Null;

            if (options == null)
            {
                options = new DecompressionOptions(false, GetDefaultEncoding());
            }

            var archiveStream = Stream.Null;
            long totalBytesWritten = 0; // for accurate progress update

            try
            {
                ZipStrings.CodePage = options.ArchiveEncoding.CodePage;
                archiveStream = await archive.OpenStreamForReadAsync();

                var writeInfo = new WriteEntryInfo
                {
                    Location = location,
                    IgnoreDirectories = false
                };

                using (var zipFile = new ZipFile(archiveStream))
                {
                    writeInfo.Zip = zipFile;
                    zipFile.Password = options.Password;
                    foreach (ZipEntry zipEntry in zipFile)
                    {
                        Token.ThrowIfCancellationRequested();
                        if (!zipEntry.IsDirectory)
                        {
                            writeInfo.Entry = zipEntry;
                            writeInfo.TotalBytesWritten = totalBytesWritten;
                            (_, totalBytesWritten) = await WriteEntry(writeInfo);
                        }
                    }
                }
            }
            catch (ICSharpCode.SharpZipLib.SharpZipBaseException ex)
            {
                if (!ex.Message.StartsWith("No password available")) throw;
                throw new ArchiveEncryptedException(ex.Message, ex);
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
            IReadOnlyList<IArchiveEntry> entries, IDecompressionOptions options = null)
        {
            return await DecompressEntries(archive, location, entries, false, options);
        }

        /// <inheritdoc />
        public override async Task<Stream> Decompress(StorageFile archive, StorageFolder location,
            IReadOnlyList<IArchiveEntry> entries, bool collectFileNames, IDecompressionOptions options = null)
        {
            return await DecompressEntries(archive, location, entries, collectFileNames, options);
        }

        #region Private Members
        private async Task<Stream> DecompressEntries(IStorageFile archive, StorageFolder location,
            IReadOnlyCollection<IArchiveEntry> entries, bool collectFileNames, IDecompressionOptions options = null)
        {
            if (archive == null || entries.IsNullOrEmpty() || location == null) return Stream.Null;

            int processedEntries = 0; // to count number of found entries
            var entriesMap = ConvertToMap(entries); // for faster access
            long totalBytesWritten = 0; // for accurate progress update
            var archiveStream = Stream.Null;

            if (options == null)
            {
                options = new DecompressionOptions(false, GetDefaultEncoding());
            }

            try
            {
                ZipStrings.CodePage = options.ArchiveEncoding.CodePage;
                archiveStream = await archive.OpenStreamForReadAsync();

                var writeInfo = new WriteEntryInfo
                {
                    Location = location,
                    IgnoreDirectories = true
                };

                using (var zipFile = new ZipFile(archiveStream))
                {
                    writeInfo.Zip = zipFile;
                    zipFile.Password = options.Password;
                    foreach (ZipEntry zipEntry in zipFile)
                    {
                        Token.ThrowIfCancellationRequested();
                        string key = Archives.NormalizeName(zipEntry.Name);
                        if (entriesMap.ContainsKey(key))
                        {
                            writeInfo.Entry = zipEntry;
                            writeInfo.TotalBytesWritten = totalBytesWritten;

                            if (collectFileNames)
                            {
                                string fileName;
                                (fileName, totalBytesWritten) = await WriteEntry(writeInfo);
                                var entry = entriesMap[key];
                                entry.FileName = fileName; // save name
                            }
                            else
                            {
                                (_, totalBytesWritten) = await WriteEntry(writeInfo);
                            }

                            ++processedEntries;
                        }

                        if (processedEntries == entries.Count) break;
                    }
                }
            }
            catch (ICSharpCode.SharpZipLib.SharpZipBaseException ex)
            {
                if (!ex.Message.StartsWith("No password available")) throw;
                throw new ArchiveEncryptedException(ex.Message, ex);
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

        private async Task<(string, long)> WriteEntry(WriteEntryInfo info)
        {
            string fileName;
            long totalBytesWritten = info.TotalBytesWritten;

            using (var entryStream = info.Zip.GetInputStream(info.Entry))
            {
                StorageFile file;
                if (info.IgnoreDirectories)
                {
                    string name = Path.GetFileName(info.Entry.Name);
                    file = await info.Location.CreateFileAsync(
                        name, CreationCollisionOption.GenerateUniqueName);
                }
                else
                {
                    file = await FileUtils.CreateFileAsync(info.Location, info.Entry.Name);
                }

                if (file == null) return (null, totalBytesWritten); // file could not be created
                fileName = file.Path; // use path in case of sub-directories

                using (var outputStream = await file.OpenStreamForWriteAsync())
                {
                    var buffer = new byte[DefaultBufferSize];
                    int readBytes;
                    while ((readBytes = entryStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        await outputStream.WriteAsync(buffer, 0, readBytes, Token);
                        totalBytesWritten += readBytes;
                        Update(totalBytesWritten);
                    }

                    await outputStream.FlushAsync(Token);
                }
            }

            return (fileName, totalBytesWritten);
        }

        private struct WriteEntryInfo
        {
            internal ZipFile Zip { get; set; }
            internal ZipEntry Entry { get; set; }
            internal StorageFolder Location { get; set; }
            internal bool IgnoreDirectories { get; set; }
            internal long TotalBytesWritten { get; set; }
        }
        #endregion
    }
}
