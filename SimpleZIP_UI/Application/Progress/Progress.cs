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

namespace SimpleZIP_UI.Application.Progress
{
    /// <summary>
    /// Stores values for progress calculation.
    /// </summary>
    public struct Progress : IEquatable<Progress>
    {
        private readonly long _totalBytesToProcess;
        private readonly long _totalBytesProcessed;

        /// <summary>
        /// Returns the percentage value of the current progress.
        /// </summary>
        internal double PercentageExact => _totalBytesProcessed / (float)_totalBytesToProcess * 100;

        /// <summary>
        /// Returns the rounded percentage value of the current progress.
        /// </summary>
        internal int Percentage => (int)Math.Round(PercentageExact);

        internal Progress(long totalBytesToProcess, long totalBytesProcessed)
        {
            _totalBytesToProcess = totalBytesToProcess;
            _totalBytesProcessed = totalBytesProcessed;
        }

        #region Equality members

        /// <inheritdoc />
        public bool Equals(Progress other)
        {
            return _totalBytesToProcess == other._totalBytesToProcess
                   && _totalBytesProcessed == other._totalBytesProcessed;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Progress other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (_totalBytesToProcess.GetHashCode() * 397) ^
                       _totalBytesProcessed.GetHashCode();
            }
        }

        /// <summary>
        /// Compares the current progress (left side) to another one (right side).
        /// </summary>
        /// <param name="left">Progress that is to be compared to the right one.</param>
        /// <param name="right">Progress that is to be compared to the left one.</param>
        /// <returns>True if the right progress matches the left one, false otherwise.</returns>
        public static bool operator ==(Progress left, Progress right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares the current progress (left side) to another one (right side).
        /// </summary>
        /// <param name="left">Progress that is to be compared to the right one.</param>
        /// <param name="right">Progress that is to be compared to the left one.</param>
        /// <returns>True if the right progress does not match the left one, false otherwise.</returns>
        public static bool operator !=(Progress left, Progress right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}
