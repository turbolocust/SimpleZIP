﻿// ==++==
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

namespace SimpleZIP_UI.Application.Compression.Algorithm.Event
{
    /// <inheritdoc />
    public sealed class BytesProcessedEventArgs : EventArgs
    {
        /// <summary>
        /// The number of bytes that have been processed.
        /// </summary>
        public long BytesProcessed { get; }

        /// <inheritdoc />
        public BytesProcessedEventArgs(long bytesProcessed)
        {
            BytesProcessed = bytesProcessed;
        }
    }
}
