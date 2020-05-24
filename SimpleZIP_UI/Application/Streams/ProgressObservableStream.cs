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
using System.IO;

namespace SimpleZIP_UI.Application.Streams
{
    internal class ProgressObservableStream : DecoratorStream
    {
        /// <summary>
        /// Observer to be notified when the amount of processed bytes changes.
        /// </summary>
        private readonly IProgressObserver<long> _observer;

        public ProgressObservableStream(IProgressObserver<long> observer,
            Stream decoratedStream) : base(decoratedStream)
        {
            _observer = observer;
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            int readBytes = base.Read(buffer, offset, count);
            NotifyObserver(readBytes);
            return readBytes;
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);
            NotifyObserver(count);
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            long previousPosition = Position;
            long currentPosition = base.Seek(offset, origin);
            long processed = currentPosition - previousPosition;

            NotifyObserver(processed);

            return currentPosition;
        }

        private void NotifyObserver(long readBytes)
        {
            _observer.Update(readBytes);
        }
    }
}
