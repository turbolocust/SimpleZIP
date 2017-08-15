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
using System.Threading;

namespace SimpleZIP_UI.Presentation
{
    internal sealed class ProgressManager
    {
        /// <summary>
        /// Placeholder value which can be set if current progress value
        /// has been received. Works like a lock and can be used to e.g. 
        /// avoid flooding of the UI thread.
        /// </summary>
        internal const double Sentinel = -1d;

        private double _progressValue;

        internal ProgressManager()
        {
            _progressValue = Sentinel;

        }

        /// <summary>
        /// Returns the current progress value and sets the specified value 
        /// as the current progress value. The specified value is set atomically.
        /// </summary>
        /// <param name="newValue">The value to be exchanged.</param>
        /// <returns>The previous assigned value.</returns>
        internal double Exchange(double newValue)
        {
            return Interlocked.Exchange(ref _progressValue, newValue);
        }
    }
}
