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
    public struct Progress
    {
        private readonly long _totalBytesToProcess;

        private readonly long _totalBytesProcessed;

        internal double PercentageExact => _totalBytesProcessed / (float)_totalBytesToProcess * 100;

        internal int Percentage => (int)Math.Round(PercentageExact);

        internal Progress(long totalBytesToProcess, long totalBytesProcessed)
        {
            _totalBytesToProcess = totalBytesToProcess;
            _totalBytesProcessed = totalBytesProcessed;
        }
    }
}
