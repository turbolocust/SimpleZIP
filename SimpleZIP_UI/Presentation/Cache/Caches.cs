// ==++==
// 
// Copyright (C) 2020 Matthias Fussenegger
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

using SimpleZIP_UI.Business.Compression.TreeBuilder;

namespace SimpleZIP_UI.Presentation.Cache
{
    internal static class Caches
    {
        /// <summary>
        /// Returns the cache instance used to store <see cref="ArchiveTreeRoot"/>
        /// instances, of which each is identified by a string.
        /// </summary>
        /// <returns>Cache instance used to store <see cref="ArchiveTreeRoot"/>.</returns>
        public static ICache<string, ArchiveTreeRoot> ForRootNode()
        {
            return RootNodeCache.Instance;
        }
    }
}
