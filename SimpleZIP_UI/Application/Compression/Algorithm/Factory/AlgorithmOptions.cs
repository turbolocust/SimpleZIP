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

namespace SimpleZIP_UI.Application.Compression.Algorithm.Factory
{
    public sealed class AlgorithmOptions
    {
        /// <summary>
        /// The default buffer size to be used by archive or compressor streams.
        /// </summary>
        private const int DefaultBufferSize = 8192;

        /// <summary>
        /// The update delay rate after which processed bytes get reported.
        /// Although accuracy suffers, this can greatly increase performance.
        /// </summary>
        public uint UpdateDelayRate { get; }

        /// <summary>
        /// The buffer size to be used by archive or compressor streams.
        /// </summary>
        public int BufferSize { get; }

        #region Constructors

        /// <summary>
        /// Constructs a new instance of this class. If <paramref name="bufferSize"/>
        /// is below or equal zero, then the <see cref="BufferSize"/> is set
        /// to <see cref="DefaultBufferSize"/>.
        /// </summary>
        /// <param name="updateDelayRate">The update-delay-rate to be set.</param>
        /// <param name="bufferSize">The buffer size to be used by algorithms
        /// for archive or compressor streams.</param>
        internal AlgorithmOptions(uint updateDelayRate, int bufferSize)
        {
            UpdateDelayRate = updateDelayRate;
            BufferSize = bufferSize > 0 ? bufferSize : DefaultBufferSize;
        }

        /// <summary>
        /// Constructs a new instance of this class with the <see cref="BufferSize"/>
        /// set to <see cref="DefaultBufferSize"/>.
        /// </summary>
        /// <param name="updateDelayRate">The update delay rate to be set.</param>
        internal AlgorithmOptions(uint updateDelayRate) : this(updateDelayRate, DefaultBufferSize)
        {
        }

        #endregion
    }
}
