// ==++==
// 
// Copyright (C) 2017 Matthias Fussenegger
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

namespace SimpleZIP_UI.Application.Compression.Streams
{
    /// <inheritdoc />
    /// <summary>
    /// Decorator for System.IO.Stream instances.
    /// </summary>
    internal abstract class DecoratorStream : Stream
    {
        /// <summary>
        /// The aggregated stream to be decorated.
        /// </summary>
        private readonly Stream _decoratedStream;

        protected DecoratorStream(Stream decoratedStream)
        {
            _decoratedStream = decoratedStream ??
                throw new ArgumentNullException(nameof(decoratedStream));
        }

        public override bool CanRead => _decoratedStream.CanRead;

        public override bool CanSeek => _decoratedStream.CanSeek;

        public override bool CanWrite => _decoratedStream.CanWrite;

        public override long Length => _decoratedStream.Length;

        public override long Position
        {
            get => _decoratedStream.Position;
            set => _decoratedStream.Position = value;
        }

        public override void Flush()
        {
            _decoratedStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _decoratedStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _decoratedStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _decoratedStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _decoratedStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _decoratedStream.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
