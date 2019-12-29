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

using System.Linq;

namespace SimpleZIP_UI.Application.Progress
{
    /// <inheritdoc />
    public sealed class ProgressManagerInexact : ProgressManager<int>
    {
        private volatile int _totalProgress;

        /// <inheritdoc />
        public override int Sentinel { get; } = -1;

        /// <inheritdoc />
        protected override int TotalProgress
        {
            get => _totalProgress;
            set => _totalProgress = value;
        }

        private ProgressManagerInexact()
        {
            TotalProgress = Sentinel;
        }

        internal static ProgressManager<int> CreateInstance()
        {
            return new ProgressManagerInexact();
        }

        /// <inheritdoc />
        internal override int UpdateProgress(object id, Progress progress)
        {
            return UpdateProgress(id, progress.PercentageInexact);
        }

        /// <inheritdoc />
        protected override int CalculateTotalProgress(object id, int newValue)
        {
            ProgressValues.AddOrUpdate(id, newValue, (key, oldValue) => newValue);
            return ProgressValues.Sum(entry => entry.Value) / ProgressValues.Count;
        }
    }
}
