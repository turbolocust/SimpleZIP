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

using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SimpleZIP_UI.Application.Compression.Algorithm.Event;
using SimpleZIP_UI.Application.Compression.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Algorithm
{
    /// <inheritdoc cref="ICompressionAlgorithm" />
    public abstract class AbstractAlgorithm : ICompressionAlgorithm, IProgressObserver<long>
    {
        /// <inheritdoc />
        /// <summary>
        /// Event handler for total bytes processed.
        /// </summary>
        public event EventHandler<TotalBytesProcessedEventArgs> TotalBytesProcessed;

        /// <inheritdoc cref="ICompressionAlgorithm.Token"/>
        public CancellationToken Token { get; set; }

        /// <summary>
        /// Default buffer size for streams.
        /// </summary>
        protected const int DefaultBufferSize = 8192;

        /// <summary>
        /// Delay rate to lessen <see cref="TotalBytesProcessed"/> events. As a result,
        /// e.g. <code>bufferSize</code> times <code>x</code> (update rate) bytes are
        /// not reported to any observers.
        /// </summary>
        protected const uint DefaultUpdateDelayRate = 100;

        /// <summary>
        /// See <see cref="DefaultUpdateDelayRate"/> for more details.
        /// </summary>
        private readonly uint _updateDelayRate;

        /// <summary>
        /// Counter used to respect <see cref="_updateDelayRate"/>.
        /// </summary>
        private uint _delayRateCounter;

        /// <inheritdoc cref="ICompressionAlgorithm" />
        protected AbstractAlgorithm(uint updateDelayRate = DefaultUpdateDelayRate)
        {
            _updateDelayRate = updateDelayRate;
        }

        /// <summary>
        /// Returns an instance of <see cref="ArchiveEncoding"/> with the default
        /// encoding set to UTF-8.
        /// </summary>
        /// <returns>An instance of <see cref="ArchiveEncoding"/>.</returns>
        protected ArchiveEncoding GetDefaultEncoding()
        {
            return new ArchiveEncoding { Default = Encoding.UTF8, Password = Encoding.UTF8 };
        }

        /// <summary>
        /// Fires <see cref="ICompressionAlgorithm.TotalBytesProcessed"/>
        /// using the specified parameter.
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
            if (_delayRateCounter++ == _updateDelayRate)
            {
                FireTotalBytesProcessed(value);
                _delayRateCounter = 0;
            }
        }

        /// <inheritdoc />
        public abstract Task<Stream> Compress(IReadOnlyList<StorageFile> files,
            StorageFile archive, StorageFolder location, WriterOptions options = null);

        /// <inheritdoc />
        public abstract Task<Stream> Decompress(StorageFile archive,
            StorageFolder location, ReaderOptions options = null);

        /// <inheritdoc />
        public abstract Task<Stream> Decompress(StorageFile archive, StorageFolder location,
            IReadOnlyList<FileEntry> entries, ReaderOptions options = null);

        /// <inheritdoc />
        public abstract Task<Stream> Decompress(StorageFile archive, StorageFolder location,
            IReadOnlyList<FileEntry> entries, bool collectFileNames, ReaderOptions options = null);
    }
}
