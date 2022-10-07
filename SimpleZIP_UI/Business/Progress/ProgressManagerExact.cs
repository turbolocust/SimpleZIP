// ==++==
// 
// Copyright (C) 2019 Matthias Fussenegger
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
using System.Linq;
using System.Threading;

namespace SimpleZIP_UI.Business.Progress
{
    /// <inheritdoc />
    public sealed class ProgressManagerExact : ProgressManager<double>
    {
        private VolatileDouble _totalProgress;

        /// <inheritdoc />
        public override double Sentinel { get; } = -1d;

        /// <inheritdoc />
        protected override double TotalProgress
        {
            get => _totalProgress.Value;
            set => _totalProgress.Value = value;
        }

        private ProgressManagerExact()
        {
            TotalProgress = Sentinel;
        }

        internal static ProgressManager<double> CreateInstance()
        {
            return new ProgressManagerExact();
        }

        /// <inheritdoc />
        internal override double UpdateProgress(object id, Progress progress)
        {
            return UpdateProgress(id, progress.Percentage);
        }

        /// <inheritdoc />
        protected override double CalculateTotalProgress(object id, double newValue)
        {
            ProgressValues.AddOrUpdate(id, newValue, (key, oldValue) => newValue);
            return ProgressValues.Sum(entry => entry.Value) / ProgressValues.Count;
        }

        private struct VolatileDouble
        {
            private long _value;

            /// <summary>
            /// Gets or sets a double value. This operation is thread-safe.
            /// </summary>
            public double Value
            {
                get
                {
                    long valueBits = Interlocked.Read(ref _value);
                    return BitConverter.Int64BitsToDouble(valueBits);
                }
                set
                {
                    long valueBits = BitConverter.DoubleToInt64Bits(value);
                    Interlocked.Exchange(ref _value, valueBits);
                }
            }
        }
    }
}
