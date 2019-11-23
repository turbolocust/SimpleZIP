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
using System.Collections.Generic;

namespace SimpleZIP_UI.Presentation.Handler
{
    internal interface ICacheHandler<T>
    {
        /// <summary>
        /// The actual cache.
        /// </summary>
        IReadOnlyDictionary<string, T> Cache { get; }

        /// <summary>
        /// Writes a new value to the cache using the specified key.
        /// </summary>
        /// <param name="key">The key to be associated with the value.</param>
        /// <param name="node">The value to be cached.</param>
        void WriteToCache(string key, T node);


        /// <summary>
        /// Reads a value from the cache. If the value with the specified
        /// key is not found, then <c>null</c> is returned.
        /// </summary>
        /// <param name="key">The key of the cached value.</param>
        /// <returns>The cached value or <c>null</c>.</returns>
        T ReadFromCache(string key);

        /// <summary>
        /// Clears the entire cache.
        /// </summary>
        void ClearCache();
    }
}
