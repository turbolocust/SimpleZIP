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

using System;
using System.IO;
using Serilog;
using SharpCompress.Common;
using SharpCompress.Readers;

namespace SimpleZIP_UI.Business.Compression.Reader
{
    internal sealed class DecoratorReader : IReader
    {
        private readonly ILogger _logger = Log.ForContext<DecoratorReader>();

        /// <summary>
        /// The decorated <see cref="IReader"/> instance.
        /// </summary>
        private readonly IReader _reader;

        public DecoratorReader(IReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        #region Properties

        /// <inheritdoc />
        public ArchiveType ArchiveType => _reader.ArchiveType;

        /// <inheritdoc />
        public IEntry Entry => _reader.Entry;

        /// <inheritdoc />
        public bool Cancelled => _reader.Cancelled;

        #endregion

        #region Methods

        /// <inheritdoc />
        public void WriteEntryTo(Stream writableStream)
        {
            _reader.WriteEntryTo(writableStream);
        }

        /// <inheritdoc />
        public void Cancel()
        {
            _reader.Cancel();
        }

        /// <inheritdoc />
        public bool MoveToNextEntry()
        {
            try
            {
                return _reader.MoveToNextEntry();
            }
            catch (CryptographicException ex)
            {
                _logger.Error(ex, "Archive of type {ArchiveType} is encrypted.", ArchiveType);
                throw new ArchiveEncryptedException(ex.Message, ex);
            }
        }

        /// <inheritdoc />
        public EntryStream OpenEntryStream()
        {
            return _reader.OpenEntryStream();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _reader.Dispose();
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public event EventHandler<ReaderExtractionEventArgs<IEntry>> EntryExtractionProgress
        {
            add => _reader.EntryExtractionProgress += value;
            remove => _reader.EntryExtractionProgress -= value;
        }

        /// <inheritdoc />
        public event EventHandler<CompressedBytesReadEventArgs> CompressedBytesRead
        {
            add => _reader.CompressedBytesRead += value;
            remove => _reader.CompressedBytesRead -= value;
        }

        /// <inheritdoc />
        public event EventHandler<FilePartExtractionBeginEventArgs> FilePartExtractionBegin
        {
            add => _reader.FilePartExtractionBegin += value;
            remove => _reader.FilePartExtractionBegin -= value;
        }

        #endregion
    }
}
