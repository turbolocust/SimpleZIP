// ==++==
// 
// Copyright (C) 2018 Matthias Fussenegger
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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Readers;
using SharpCompress.Writers;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Compression.Algorithm.Event;

namespace SimpleZIP_UI.Application.Compression.Algorithm
{
    /// <inheritdoc cref="ICompressionAlgorithm" />
    public abstract class AbstractAlgorithm : ICompressionAlgorithm, IProgressObserver<long>
    {
        /// <summary>
        /// Default buffer size for streams.
        /// </summary>
        protected const int DefaultBufferSize = 8192;

        /// <inheritdoc />
        /// <summary>
        /// Event handler for total bytes processed.
        /// </summary>
        public event EventHandler<TotalBytesProcessedEventArgs> TotalBytesProcessed;

        /// <inheritdoc cref="ICompressionAlgorithm.Token"/>
        public CancellationToken Token { get; set; }

        /// <summary>
        /// Fires <see cref="ICompressionAlgorithm.TotalBytesProcessed"/> using the specified parameter.
        /// </summary>
        /// <param name="processedBytes">The bytes processed so far.</param>
        protected virtual void FireTotalBytesProcessed(long processedBytes)
        {
            var evtArgs = new TotalBytesProcessedEventArgs
            {
                TotalBytesProcessed = processedBytes
            };
            TotalBytesProcessed?.Invoke(this, evtArgs);
        }

        /// <inheritdoc cref="IProgressObserver{T}.Update"/>
        public void Update(long value)
        {
            FireTotalBytesProcessed(value);
        }

        /// <inheritdoc cref="ICompressionAlgorithm.Compress"/>
        public abstract Task<Stream> Compress(IReadOnlyList<StorageFile> files,
            StorageFile archive, StorageFolder location, WriterOptions options = null);

        /// <inheritdoc cref="ICompressionAlgorithm.Decompress(StorageFile,StorageFolder,ReaderOptions)"/>
        public abstract Task<Stream> Decompress(StorageFile archive,
            StorageFolder location, ReaderOptions options = null);

        /// <inheritdoc cref="ICompressionAlgorithm.Decompress(StorageFile,StorageFolder,IReadOnlyList&lt;FileEntry&gt;,ReaderOptions)"/>
        public abstract Task<Stream> Decompress(StorageFile archive, StorageFolder location,
            IReadOnlyList<FileEntry> entries, ReaderOptions options = null);
    }
}
