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

namespace SimpleZIP_UI.Application.Progress
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
}
