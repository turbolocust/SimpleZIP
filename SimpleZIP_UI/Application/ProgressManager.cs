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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace SimpleZIP_UI.Application
{
    /// <summary>
    /// Offers methods to create new instances of <see cref="ProgressManager{TNumber}"/>.
    /// </summary>
    public static class ProgressManagers
    {
        /// <summary>
        /// Creates a new progress manager for inexact progress calculation.
        /// </summary>
        /// <returns>A new progress manager for inexact progress calculation.</returns>
        public static ProgressManager<int> CreateInexact()
        {
            return ProgressManagerInexact.CreateInstance();
        }

        /// <summary>
        /// Creates a new progress manager for exact progress calculation.
        /// </summary>
        /// <returns>A new progress manager for exact progress calculation.</returns>
        public static ProgressManager<double> CreateExact()
        {
            return ProgressManagerExact.CreateInstance();
        }
    }

    /// <summary>
    /// Represents a manager for progress values.
    /// </summary>
    /// <typeparam name="TNumber">Any numeric type.</typeparam>
    public abstract class ProgressManager<TNumber>
    {
        /// <summary>
        /// Placeholder value which can be set if current progress value
        /// has been received. Works like a lock and is used to e.g. 
        /// avoid flooding of the UI thread.
        /// </summary>
        public abstract TNumber Sentinel { get; }

        /// <summary>
        /// Holds either the total progress or <see cref="Sentinel"/>.
        /// </summary>
        protected TNumber TotalProgress;

        /// <summary>
        /// Maps identifiers (keys) to progression values.
        /// </summary>
        protected readonly ConcurrentDictionary<object, TNumber> ProgressValues;

        /// <summary>
        /// Creates a new ProgressManager instance.
        /// </summary>
        protected ProgressManager()
        {
            ProgressValues = new ConcurrentDictionary<object, TNumber>();
        }

        /// <summary>
        /// Resets this instance and clears all mappings.
        /// </summary>
        public void Reset()
        {
            TotalProgress = Sentinel;
            ProgressValues.Clear();
        }

        /// <summary>
        /// Returns the current progress value and sets the specified value 
        /// as the current progress value. The specified value is set atomically.
        /// </summary>
        /// <param name="newValue">The value to be exchanged.</param>
        /// <returns>The previously assigned value.</returns>
        public abstract TNumber Exchange(TNumber newValue);

        /// <summary>
        /// Updates and returns the total progress value considering all mapped
        /// values. Each time this method is called with an unknown key, the
        /// specified value will be mapped to that key. If the key already exists,
        /// the currently mapped value will be updated.
        /// </summary>
        /// <param name="id">Key to which the value is to be mapped.</param>
        /// <param name="newValue">Updated value to be mapped.</param>
        /// <returns>The total progress value considering all mappings.</returns>
        public TNumber UpdateProgress(object id, TNumber newValue)
        {
            return ProgressValues.Count > 1
                ? CalculateTotalProgress(id, newValue) : newValue;
        }

        /// <summary>
        /// Updates and returns the total progress value considering all mapped
        /// values. Each time this method is called with an unknown key, the
        /// specified value will be mapped to that key. If the key already exists,
        /// the currently mapped value will be updated. Based on the concrete instance
        /// of <see cref="ProgressManager{TNumber}"/> the correct progress value
        /// is being extracted from <see cref="Progress"/>.
        /// </summary>
        /// <param name="id">Key to which the value is to be mapped.</param>
        /// <param name="progress">Holds the progress value to be updated.</param>
        /// <returns>The total progress value considering all mappings.</returns>
        internal abstract TNumber UpdateProgress(object id, Progress progress);

        /// <summary>
        /// Calculates and returns the total progress.
        /// </summary>
        /// <param name="id">The id to which the new value shall be mapped.</param>
        /// <param name="newValue">The updated progress value.</param>
        /// <returns></returns>
        protected abstract TNumber CalculateTotalProgress(object id, TNumber newValue);
    }

    /// <inheritdoc />
    public sealed class ProgressManagerInexact : ProgressManager<int>
    {
        /// <inheritdoc />
        public override int Sentinel { get; } = -1;

        private ProgressManagerInexact()
        {
            TotalProgress = Sentinel;
        }

        internal static ProgressManager<int> CreateInstance()
        {
            return new ProgressManagerInexact();
        }

        /// <inheritdoc />
        public override int Exchange(int newValue)
        {
            return Interlocked.Exchange(ref TotalProgress, newValue);
        }

        /// <inheritdoc />
        internal override int UpdateProgress(object id, Progress progress)
        {
            return UpdateProgress(id, progress.Percentage);
        }

        /// <inheritdoc />
        protected override int CalculateTotalProgress(object id, int newValue)
        {
            ProgressValues.AddOrUpdate(id, newValue, (key, oldValue) => newValue);
            return ProgressValues.Sum(entry => entry.Value) / ProgressValues.Count;
        }
    }

    /// <inheritdoc />
    public sealed class ProgressManagerExact : ProgressManager<double>
    {
        /// <inheritdoc />
        public override double Sentinel { get; } = -1d;

        private ProgressManagerExact()
        {
            TotalProgress = Sentinel;
        }

        internal static ProgressManager<double> CreateInstance()
        {
            return new ProgressManagerExact();
        }

        /// <inheritdoc />
        public override double Exchange(double newValue)
        {
            return Interlocked.Exchange(ref TotalProgress, newValue);
        }

        /// <inheritdoc />
        internal override double UpdateProgress(object id, Progress progress)
        {
            return UpdateProgress(id, progress.PercentageExact);
        }

        /// <inheritdoc />
        protected override double CalculateTotalProgress(object id, double newValue)
        {
            ProgressValues.AddOrUpdate(id, newValue, (key, oldValue) => newValue);
            return ProgressValues.Sum(entry => entry.Value) / ProgressValues.Count;
        }
    }
}
