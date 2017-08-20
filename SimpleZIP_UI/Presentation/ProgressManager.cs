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

using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SimpleZIP_UI.Presentation
{
    internal sealed class ProgressManager
    {
        /// <summary>
        /// Placeholder value which can be set if current progress value
        /// has been received. Works like a lock and is used to e.g. 
        /// avoid flooding of the UI thread.
        /// </summary>
        internal const double Sentinel = -1d;

        /// <summary>
        /// Holds either the total progress or <see cref="Sentinel"/>.
        /// </summary>
        private double _totalProgress;

        /// <summary>
        /// Maps keys to their progress values. The values are held by 
        /// <see cref="ProgressHolder"/>.
        /// </summary>
        private readonly Dictionary<object, ProgressHolder> _progressValues;

        internal ProgressManager()
        {
            _totalProgress = Sentinel;
            _progressValues = new Dictionary<object, ProgressHolder>();
        }

        /// <summary>
        /// Resets this instance and clears all mappings.
        /// </summary>
        internal void Reset()
        {
            _totalProgress = Sentinel;
            _progressValues.Clear();
        }

        /// <summary>
        /// Returns the current progress value and sets the specified value 
        /// as the current progress value. The specified value is set atomically.
        /// </summary>
        /// <param name="newValue">The value to be exchanged.</param>
        /// <returns>The previously assigned value.</returns>
        internal double Exchange(double newValue)
        {
            return Interlocked.Exchange(ref _totalProgress, newValue);
        }

        /// <summary>
        /// Updates and returns the total progress value considering all mapped
        /// values. Each time this method is called with an unknown key, the
        /// specified value will be mapped to that key. If the key already exists,
        /// the currently mapped value will be updated.
        /// </summary>
        /// <param name="key">Key to which the value is to be mapped.</param>
        /// <param name="value">Updated value to be mapped.</param>
        /// <returns>The total progress value considering all mappings.</returns>
        internal double UpdateProgress(object key, double value)
        {
            return _progressValues.Count > 1
                ? CalculateTotalProgress(key, value)
                : value / 100d;
        }

        private double CalculateTotalProgress(object key, double value)
        {
            var exists = _progressValues.TryGetValue(key, out ProgressHolder holder);
            if (!exists)
            {
                holder = new ProgressHolder();
                _progressValues.Add(key, holder);
            }
            holder.Progress = value;

            var progress = _progressValues.Sum(entry => entry.Value.Progress);
            progress /= _progressValues.Count;
            return progress / 100d;
        }

        private class ProgressHolder
        {
            internal double Progress;
        }
    }
}
