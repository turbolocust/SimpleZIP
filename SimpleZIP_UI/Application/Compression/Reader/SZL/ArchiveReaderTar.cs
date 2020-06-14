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

using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Reader.SZL
{
    /// <inheritdoc />
    internal class ArchiveReaderTar : IArchiveReader
    {
        /// <summary>
        /// The input stream for the TAR archive.
        /// Is <c>null</c> if not yet opened.
        /// </summary>
        private TarInputStream _tarInputStream;

        /// <summary>
        /// True if this reader is closed/disposed, false otherwise.
        /// </summary>
        private bool _closed;

        /// <summary>
        /// Token used for cancelling archive reading.
        /// </summary>
        private CancellationToken _cancellationToken;

        /// <summary>
        /// The archive to be read by this reader.
        /// </summary>
        private readonly IStorageFile _archive;

        /// <summary>
        /// Creates a new instance of a reader for TAR archives.
        /// </summary>
        /// <param name="archive">The archive to be read.</param>
        /// <param name="cancellationToken">Token used for cancellation.</param>
        public ArchiveReaderTar(IStorageFile archive, CancellationToken cancellationToken)
        {
            _archive = archive;
            _cancellationToken = cancellationToken;
            _cancellationToken.Register(() => _closed = true);
        }

        /// <summary>
        /// Returns an input stream for reading the specified archive.
        /// This may be overridden by subclasses in case the archive
        /// is compressed (e.g. GZIP compressed entries)
        /// </summary>
        /// <param name="stream">The stream of the archive to be read.</param>
        /// <returns>The compressor stream to be used.</returns>
        protected virtual Stream GetCompressorStream(Stream stream)
        {
            return stream; // is uncompressed
        }

        /// <inheritdoc />
        public async Task OpenArchiveAsync(string password = null)
        {
            if (_closed) throw new ObjectDisposedException(ToString());

            var fileStream = await _archive.OpenStreamForReadAsync().ConfigureAwait(false);
            var stream = GetCompressorStream(fileStream);
            _tarInputStream = new TarInputStream(stream)
            {
                IsStreamOwner = true
            };
        }

        /// <inheritdoc />
        public IEnumerable<IArchiveEntry> ReadArchive()
        {
            if (_closed) throw new ObjectDisposedException(ToString());

            TarEntry entry;
            while ((entry = _tarInputStream.GetNextEntry()) != null)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                yield return new ArchiveEntry(
                    entry.Name,
                    entry.IsDirectory,
                    (ulong)entry.Size,
                    entry.ModTime);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _tarInputStream?.Close();
            _closed = true;
        }
    }
}
