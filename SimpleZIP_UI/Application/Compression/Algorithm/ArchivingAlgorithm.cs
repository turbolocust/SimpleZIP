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
using SimpleZIP_UI.Application.Compression.Algorithm.Options;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Streams;
using SimpleZIP_UI.Application.Util;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            StorageFolder location, IDecompressionOptions options = null)
        {
            if (archive == null || location == null) return Stream.Null;

            var readerOptions = options != null
                ? ConvertDecompressionOptions(options)
                : new ReaderOptions
                {
                    LeaveStreamOpen = false,
                    ArchiveEncoding = ConvertEncoding(GetDefaultEncoding())
                };

            var archiveStream = Stream.Null;
            long totalBytesWritten = 0; // for accurate progress update

            try
            {
                archiveStream = await archive.OpenStreamForReadAsync();
                using (var reader = ReaderFactory.Open(archiveStream, readerOptions))
                {
                    while (reader.MoveToNextEntry())
                    {
                        Token.ThrowIfCancellationRequested();
                        if (!reader.Entry.IsDirectory)
                        {
                            (_, totalBytesWritten) = await WriteEntry(reader, location, totalBytesWritten);
                        }
                    }
                }
            }
            catch (CryptographicException ex)
            {
                throw new ArchiveEncryptedException(ex.Message, ex);
            }
            finally
            {
                if (!readerOptions.LeaveStreamOpen)
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

        /// <inheritdoc />
        public override async Task<Stream> Compress(IReadOnlyList<StorageFile> files,
            StorageFile archive, StorageFolder location, ICompressionOptions options = null)
        {
            if (files.IsNullOrEmpty() | archive == null | location == null) return Stream.Null;

            var writerOptions = ConvertCompressionOptions(options);
            var progressStream = Stream.Null;

            try
            {
                var archiveStream = await archive.OpenStreamForWriteAsync();
                progressStream = new ProgressObservableStream(this, archiveStream);

                using (var writer = WriterFactory.Open(progressStream, _type, writerOptions))
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
                if (!writerOptions.LeaveStreamOpen)
                {
                    progressStream.Dispose();
                }
            }

            return progressStream;
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

            var readerOptions = options != null
                ? ConvertDecompressionOptions(options)
                : new ReaderOptions
                {
                    LeaveStreamOpen = false,
                    ArchiveEncoding = ConvertEncoding(GetDefaultEncoding())
                };

            try
            {
                archiveStream = await archive.OpenStreamForReadAsync();
                using (var reader = ReaderFactory.Open(archiveStream, readerOptions))
                {
                    while (reader.MoveToNextEntry())
                    {
                        Token.ThrowIfCancellationRequested();
                        string key = Archives.NormalizeName(reader.Entry.Key);
                        if (entriesMap.ContainsKey(key))
                        {
                            if (collectFileNames)
                            {
                                string fileName;
                                (fileName, totalBytesWritten) = await WriteEntry(
                                    reader, location, totalBytesWritten);
                                var entry = entriesMap[key];
                                entry.FileName = fileName; // save name
                            }
                            else
                            {
                                (_, totalBytesWritten) = await WriteEntry(
                                    reader, location, totalBytesWritten);
                            }

                            ++processedEntries;
                        }

                        if (processedEntries == entries.Count) break;
                    }
                }
            }
            catch (CryptographicException ex)
            {
                throw new ArchiveEncryptedException(ex.Message, ex);
            }
            finally
            {
                if (!readerOptions.LeaveStreamOpen)
                {
                    archiveStream.Dispose();
                }
            }

            return archiveStream;
        }

        private async Task<(string, long)> WriteEntry(IReader reader,
            StorageFolder location, long totalBytesWritten)
        {
            string fileName;

            using (var entryStream = reader.OpenEntryStream())
            {
                var file = await FileUtils.CreateFileAsync(location, reader.Entry.Key);
                if (file == null) return (null, totalBytesWritten); // file could not be created

                fileName = file.Name;

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

        private ArchiveEncoding ConvertEncoding(Encoding encoding)
        {
            var enc = encoding ?? GetDefaultEncoding();
            return new ArchiveEncoding
            {
                Default = enc,
                Password = enc
            };
        }

        private ReaderOptions ConvertDecompressionOptions(IDecompressionOptions options)
        {
            return new ReaderOptions
            {
                LeaveStreamOpen = options.LeaveStreamOpen,
                ArchiveEncoding = ConvertEncoding(options.ArchiveEncoding),
                Password = options.Password
            };
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private WriterOptions ConvertCompressionOptions(ICompressionOptions options)
        {
            var writerOptions = GetWriterOptions();

            if (options != null)
            {
                writerOptions.LeaveStreamOpen = options.LeaveStreamOpen;
                writerOptions.ArchiveEncoding = ConvertEncoding(options.ArchiveEncoding);
            }
            else
            {
                writerOptions.LeaveStreamOpen = false;
                writerOptions.ArchiveEncoding = ConvertEncoding(GetDefaultEncoding());
            }

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
