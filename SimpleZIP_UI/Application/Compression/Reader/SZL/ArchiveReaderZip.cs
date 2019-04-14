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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Reader.SZL
{
    /// <inheritdoc />
    /// <summary>
    /// Archive reader implementation for ZIP files only,
    /// which uses capabilities of the SharpZipLib library.
    /// This class was introduced because of some bugs in
    /// the SharpCompress library.
    /// </summary>
    internal sealed class ArchiveReaderZip : IArchiveReader
    {
        /// <summary>
        /// The currently opened ZIP archive.
        /// Is <code>null</code> if not yet opened.
        /// </summary>
        private ZipFile _zipFile;

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
        /// Creates a new instance of a reader for ZIP archives.
        /// </summary>
        /// <param name="archive">The archive to be read.</param>
        /// <param name="cancellationToken">Token used for cancellation.</param>
        public ArchiveReaderZip(IStorageFile archive, CancellationToken cancellationToken)
        {
            _archive = archive;
            _cancellationToken = cancellationToken;
            _cancellationToken.Register(() => _closed = true);
        }

        /// <inheritdoc />
        public async Task OpenArchiveAsync(string password = null)
        {
            if (_closed) throw new ObjectDisposedException(ToString());

            ZipStrings.CodePage = Encoding.UTF8.CodePage;
            var stream = await _archive.OpenStreamForReadAsync();
            _zipFile = new ZipFile(stream)
            {
                IsStreamOwner = true,
                Password = password
            };
        }

        /// <inheritdoc />
        public IEnumerable<IArchiveEntry> ReadArchive()
        {
            if (_closed) throw new ObjectDisposedException(ToString());

            var entries = _zipFile.GetEnumerator();
            while (!_closed && entries.MoveNext())
            {
                _cancellationToken.ThrowIfCancellationRequested();
                var entry = (ZipEntry)entries.Current;
                yield return new ArchiveEntry(
                    entry.Name,
                    entry.IsDirectory,
                    (ulong)entry.Size);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _zipFile?.Close();
            _closed = true;
        }
    }
}
