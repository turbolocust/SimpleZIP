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

namespace SimpleZIP_UI.Application.Util
{
    internal static class CollectionUtils
    {
        /// <summary>
        /// Checks whether this collection is <code>null</code> or empty.
        /// </summary>
        /// <typeparam name="T">The type of elements in this collection.</typeparam>
        /// <param name="collection">The collection to be checked.</param>
        /// <returns>True if collection is <code>null</code> or empty, false otherwise.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }
    }
}
