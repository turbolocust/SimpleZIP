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

using SimpleZIP_UI.Business.Compression.Algorithm.Event;
using SimpleZIP_UI.Business.Compression.Algorithm.Options;
using SimpleZIP_UI.Business.Compression.Reader;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleZIP_UI.Business.Compression.Algorithm.Factory;

namespace SimpleZIP_UI.Business.Compression.Algorithm
{
    /// <inheritdoc cref="ICompressionAlgorithm" />
    public abstract class AbstractAlgorithm : ICompressionAlgorithm, IProgressObserver<long>
    {
        /// <summary>
        /// It is counted up to this delay before progress updates are fired.
        /// </summary>
        internal readonly uint UpdateDelayRate;

        /// <summary>
        /// Counter used to respect <see cref="UpdateDelayRate"/>.
        /// </summary>
        internal uint DelayRateCounter { get; private set; }

        /// <summary>
        /// The buffer size to be used by streams.
        /// </summary>
        protected int BufferSize { get; }

        /// <summary>
        /// Buffers any non-reported bytes between zero and <see cref="UpdateDelayRate"/>.
        /// </summary>
        private long _bytesProcessedBuffer;

        /// <summary>
        /// Constructs a new instance of this class.
        /// </summary>
        /// <param name="options">Options to be considered by this instance.</param>
        protected AbstractAlgorithm(AlgorithmOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            BufferSize = options.BufferSize;
            UpdateDelayRate = options.UpdateDelayRate;
        }

        /// <summary>
        /// Fires <see cref="ICompressionAlgorithm.BytesProcessed"/>.
        /// </summary>
        /// <param name="processedBytes">The bytes processed so far.</param>
        private void FireBytesProcessed(long processedBytes)
        {
            var evtArgs = new BytesProcessedEventArgs(processedBytes);
            BytesProcessed?.Invoke(this, evtArgs);
        }

        /// <summary>
        /// Converts the specified list of <see cref="IArchiveEntry"/> to a dictionary.
        /// </summary>
        /// <param name="entries">List of <see cref="IArchiveEntry"/> to be converted.</param>
        /// <returns>A dictionary consisting of <see cref="IArchiveEntry"/> instances.</returns>
        protected static IDictionary<string, IArchiveEntry> ConvertToMap(IReadOnlyCollection<IArchiveEntry> entries)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));

            int mapSize = entries.Count * 2;
            var map = new Dictionary<string, IArchiveEntry>(mapSize);
            foreach (var entry in entries) map.Add(entry.Key, entry);

            return map;
        }

        /// <summary>
        /// Returns an instance of <see cref="Encoding"/> for UTF-8.
        /// </summary>
        /// <returns>An instance of <see cref="Encoding"/>.</returns>
        protected virtual Encoding GetDefaultEncoding()
        {
            return Encoding.UTF8;
        }

        /// <summary>
        /// Flushes the buffer of processed bytes, which holds buffered
        /// bytes due to <see cref="UpdateDelayRate"/>.
        /// </summary>
        protected void FlushBytesProcessedBuffer()
        {
            FireBytesProcessed(_bytesProcessedBuffer);
            _bytesProcessedBuffer = 0;
        }

        /// <inheritdoc cref="IProgressObserver{T}.Update"/>
        public void Update(long value)
        {
            _bytesProcessedBuffer += value;

            if (DelayRateCounter++ == UpdateDelayRate)
            {
                FireBytesProcessed(_bytesProcessedBuffer);
                DelayRateCounter = 0;
                _bytesProcessedBuffer = 0;
            }
        }

        #region ICompressionAlgorithm

        /// <inheritdoc />
        public event EventHandler<BytesProcessedEventArgs> BytesProcessed;

        /// <inheritdoc cref="ICompressionAlgorithm.Token"/>
        public CancellationToken Token { get; set; }

        /// <inheritdoc />
        public abstract Task CompressAsync(
            IReadOnlyList<StorageFile> files,
            StorageFile archive,
            StorageFolder location,
            ICompressionOptions options = null);

        /// <inheritdoc />
        public abstract Task DecompressAsync(
            StorageFile archive,
            StorageFolder location,
            IDecompressionOptions options = null);

        /// <inheritdoc />
        public abstract Task DecompressAsync(
            StorageFile archive,
            StorageFolder location,
            IReadOnlyList<IArchiveEntry> entries,
            bool collectFileNames,
            IDecompressionOptions options = null);

        #endregion
    }
}