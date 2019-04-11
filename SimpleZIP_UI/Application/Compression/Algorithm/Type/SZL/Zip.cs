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
    /// introduced because of some bugs in the SharpCompress library.
    /// </summary>
    public class Zip : AbstractAlgorithm
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
                        //await writer.WriteAsync(file.Name, inputStream, Token);
                        ulong size = await FileUtils.GetFileSizeAsync(file);
                        var zipEntry = new ZipEntry(file.Name)
                        {
                            DateTime = file.DateCreated.DateTime,
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

                            await fileStream.FlushAsync(Token);
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

                using (var zipFile = new ZipFile(archiveStream))
                {
                    zipFile.Password = options.Password;
                    foreach (ZipEntry zipEntry in zipFile)
                    {
                        Token.ThrowIfCancellationRequested();
                        if (!zipEntry.IsDirectory)
                        {
                            (_, totalBytesWritten) = await WriteEntry(
                                zipFile, zipEntry, location, totalBytesWritten);
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

        #region Private Helper Methods
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

                using (var zipFile = new ZipFile(archiveStream))
                {
                    zipFile.Password = options.Password;
                    foreach (ZipEntry zipEntry in zipFile)
                    {
                        Token.ThrowIfCancellationRequested();
                        string key = Archives.NormalizeName(zipEntry.Name);
                        if (entriesMap.ContainsKey(key))
                        {
                            if (collectFileNames)
                            {
                                string fileName;
                                (fileName, totalBytesWritten) = await WriteEntry(
                                    zipFile, zipEntry, location, totalBytesWritten);
                                var entry = entriesMap[key];
                                entry.FileName = fileName; // save name
                            }
                            else
                            {
                                (_, totalBytesWritten) = await WriteEntry(
                                    zipFile, zipEntry, location, totalBytesWritten);
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

        private async Task<(string, long)> WriteEntry(ZipFile zip,
            ZipEntry entry, StorageFolder location, long totalBytesWritten)
        {
            string fileName;

            using (var entryStream = zip.GetInputStream(entry))
            {
                var file = await FileUtils.CreateFileAsync(location, entry.Name);
                if (file == null) return (null, totalBytesWritten); // file could not be created

                fileName = file.Path;

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

                    await entryStream.FlushAsync(Token);
                }
            }

            return (fileName, totalBytesWritten);
        }
        #endregion
    }
}
