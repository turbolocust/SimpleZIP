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

using SimpleZIP_UI.Application.Compression.Algorithm.Event;
using SimpleZIP_UI.Application.Compression.Algorithm.Options;
using SimpleZIP_UI.Application.Compression.Reader;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Algorithm
{
    /// <inheritdoc cref="ICompressionAlgorithm" />
    public abstract class AbstractAlgorithm : ICompressionAlgorithm, IProgressObserver<long>
    {
        /// <summary>
        /// Delay rate to lessen <see cref="TotalBytesProcessed"/> events.
        /// As a result, <c>bufferSize</c> times <c>x</c> (update rate) bytes
        /// are not reported to any observers.
        /// </summary>
        private const uint DefaultUpdateDelayRate = 100;

        /// <summary>
        /// Default buffer size for streams.
        /// </summary>
        protected const int DefaultBufferSize = 8192;

        /// <summary>
        /// See <see cref="DefaultUpdateDelayRate"/> for more details.
        /// </summary>
        internal readonly uint UpdateDelayRate;

        /// <summary>
        /// Counter used to respect <see cref="UpdateDelayRate"/>.
        /// </summary>
        internal uint DelayRateCounter { get; private set; }

        /// <summary>
        /// Constructs a new instance of this class.
        /// </summary>
        /// <param name="updateDelayRate">The default update delay rate.</param>
        /// <param name="initialDelayRateCounter">The initial delay rate counter.
        /// Is set to zero if greater than <paramref name="updateDelayRate"/>.</param>
        protected AbstractAlgorithm(uint updateDelayRate = DefaultUpdateDelayRate, uint initialDelayRateCounter = 0)
        {
            UpdateDelayRate = updateDelayRate;
            DelayRateCounter = initialDelayRateCounter <= updateDelayRate ? initialDelayRateCounter : 0;
        }

        /// <summary>
        /// Fires <see cref="ICompressionAlgorithm.TotalBytesProcessed"/>
        /// using the specified parameter.
        /// </summary>
        /// <param name="processedBytes">The bytes processed so far.</param>
        private void FireTotalBytesProcessed(long processedBytes)
        {
            var evtArgs = new TotalBytesProcessedEventArgs { TotalBytesProcessed = processedBytes };
            TotalBytesProcessed?.Invoke(this, evtArgs);
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

        /// <inheritdoc cref="IProgressObserver{T}.Update"/>
        public void Update(long value)
        {
            if (DelayRateCounter++ == UpdateDelayRate)
            {
                FireTotalBytesProcessed(value);
                DelayRateCounter = 0;
            }
        }

        #region ICompressionAlgorithm

        /// <inheritdoc />
        public event EventHandler<TotalBytesProcessedEventArgs> TotalBytesProcessed;

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