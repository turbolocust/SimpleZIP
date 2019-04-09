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

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using ICSharpCode.SharpZipLib.Zip;

namespace SimpleZIP_UI.Application.Compression.Reader.SZL
{
    internal class ArchiveReader : IArchiveReader
    {
        /// <summary>
        /// The currently opened ZIP archive.
        /// </summary>
        private ZipFile _zipFile;

        /// <summary>
        /// True if this reader is closed, false otherwise.
        /// </summary>
        private bool _closed;

        /// <summary>
        /// Token used for cancellation of reading archive.
        /// </summary>
        private CancellationToken _cancellationToken;

        /// <summary>
        /// The archive as <see cref="IStorageFile"/>.
        /// </summary>
        private readonly IStorageFile _archive;

        public ArchiveReader(IStorageFile archive, CancellationToken cancellationToken)
        {
            _archive = archive;
            _cancellationToken = cancellationToken;
            _cancellationToken.Register(() => _closed = true);
        }

        /// <inheritdoc />
        public async Task OpenArchiveAsync(string password = null)
        {
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
        }
    }
}
