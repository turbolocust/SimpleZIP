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
using System.IO;

namespace SimpleZIP_UI.Application.Streams
{
    internal class ProgressObservableStream : DecoratorStream
    {
        /// <summary>
        /// Observer to be notified when the amount of processed bytes changes.
        /// </summary>
        private readonly IProgressObserver<long> _observer;

        /// <summary>
        /// Total amount of bytes which have already been processed.
        /// </summary>
        private long _totalBytesProcessed;

        public ProgressObservableStream(IProgressObserver<long> observer,
            Stream decoratedStream) : base(decoratedStream)
        {
            _observer = observer;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readBytes = base.Read(buffer, offset, count);

            _totalBytesProcessed += readBytes;
            NotifyObserver();

            return readBytes;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);

            _totalBytesProcessed += count;
            NotifyObserver();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var previousPosition = Position;
            var currentPosition = base.Seek(offset, origin);

            _totalBytesProcessed += currentPosition - previousPosition;
            NotifyObserver();

            return currentPosition;
        }

        /// <summary>
        /// Notifies the aggregated observer.
        /// </summary>
        private void NotifyObserver()
        {
            _observer.Update(_totalBytesProcessed);
        }
    }
}
