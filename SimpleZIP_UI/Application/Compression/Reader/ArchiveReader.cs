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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Reader
{
    /// <inheritdoc />
    /// <summary>
    /// Archive reader implementation which uses capabilities
    /// of the SharpCompress library.
    /// </summary>
    internal sealed class ArchiveReader : IArchiveReader
    {
        /// <summary>
        /// The reader (SharpCompress) associated with this instance.
        /// </summary>
        private IReader _reader;

        /// <summary>
        /// True if this reader is closed/disposed, false otherwise.
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
            if (_closed) throw new ObjectDisposedException(ToString());

            var options = new ReaderOptions
            {
                LeaveStreamOpen = false,
                Password = password,
                ArchiveEncoding = new ArchiveEncoding
                {
                    Default = Encoding.UTF8,
                    Password = Encoding.UTF8
                }
            };

            var stream = await _archive.OpenStreamForReadAsync().ConfigureAwait(false);
            _reader = ReaderFactory.Open(stream, options);
        }

        /// <inheritdoc />
        public IEnumerable<IArchiveEntry> ReadArchive()
        {
            if (_closed) throw new ObjectDisposedException(ToString());

            while (!_closed && _reader.MoveToNextEntry())
            {
                _cancellationToken.ThrowIfCancellationRequested();
                yield return new ArchiveEntry(
                    _reader.Entry.Key,
                    _reader.Entry.IsDirectory,
                    (ulong)_reader.Entry.Size);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _reader?.Dispose();
            _closed = true;
        }
    }
}