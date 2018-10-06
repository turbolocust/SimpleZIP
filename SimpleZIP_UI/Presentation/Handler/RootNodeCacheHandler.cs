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

using SimpleZIP_UI.Application.Compression.Reader;
using System.Collections.Generic;

namespace SimpleZIP_UI.Presentation.Handler
{
    internal sealed class RootNodeCacheHandler : ICacheHandler<RootNode>
    {
        private readonly Dictionary<string, RootNode> _nodesCache;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, RootNode> Cache => _nodesCache;

        /// <inheritdoc />
        public void WriteToCache(string key, RootNode node)
        {
            _nodesCache.Add(key, node);
        }

        /// <inheritdoc />
        public RootNode ReadFromCache(string key)
        {
            _nodesCache.TryGetValue(key, out var rootNode);
            return rootNode; // can be null
        }

        /// <inheritdoc />
        public void ClearCache()
        {
            _nodesCache.Clear();
        }

        #region Singleton members
        private static readonly object LockObject = new object();

        private static RootNodeCacheHandler _instance;
        public static RootNodeCacheHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        _instance = new RootNodeCacheHandler();
                    }
                }

                return _instance;
            }
        }

        private RootNodeCacheHandler()
        {
            _nodesCache = new Dictionary<string, RootNode>();
        }
        #endregion
    }
}
