﻿// ==++==
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

using SimpleZIP_UI.Application;
using SimpleZIP_UI.Application.Compression.TreeBuilder;
using SimpleZIP_UI.Application.Util;
using System.Collections.Generic;

namespace SimpleZIP_UI.Presentation.Handler
{
    internal sealed class RootNodeCacheHandler : ICacheHandler<ArchiveTreeRoot>
    {
        private readonly Dictionary<string, ArchiveTreeRoot> _nodesCache;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, ArchiveTreeRoot> Cache => _nodesCache;

        /// <inheritdoc />
        public void WriteToCache(string key, ArchiveTreeRoot node)
        {
            _nodesCache.Add(key, node);
        }

        /// <inheritdoc />
        public ArchiveTreeRoot ReadFromCache(string key)
        {
            _nodesCache.TryGetValue(key, out var rootNode);
            return rootNode; // can be null
        }

        /// <inheritdoc />
        public void ClearCache()
        {
            _nodesCache.Clear();
        }

        /// <summary>
        /// Performs initialization of e.g. cached files created in some use cases.
        /// </summary>
        /// <param name="force">True to force initialization, false to respect threshold.</param>
        internal static async void CheckInitialize(bool force = false)
        {
            // only clear cache if forced or threshold is exceeded
            if (force || Instance.Cache.Count > 10)
            {
                Instance.ClearCache();
                try
                {
                    var tempFolder = await FileUtils.GetTempFolderAsync(TempFolder.Archives);
                    await FileUtils.CleanFolderAsync(tempFolder);
                }
                catch
                {
                    // ignore (execution cannot be awaited)
                }
            }
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
            _nodesCache = new Dictionary<string, ArchiveTreeRoot>();
        }
        #endregion
    }
}
