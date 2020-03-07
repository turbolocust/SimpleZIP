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

using System.Collections.Generic;
using SimpleZIP_UI.Application;
using SimpleZIP_UI.Application.Compression.TreeBuilder;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Presentation.Cache
{
    internal sealed class RootNodeCache : ICache<string, ArchiveTreeRoot>
    {
        private readonly Dictionary<string, ArchiveTreeRoot> _nodesCache;

        /// <inheritdoc />
        public void WriteTo(string key, ArchiveTreeRoot node)
        {
            _nodesCache.Add(key, node);
        }

        /// <inheritdoc />
        public ArchiveTreeRoot ReadFrom(string key)
        {
            _nodesCache.TryGetValue(key, out var rootNode);
            return rootNode; // can be null
        }

        /// <inheritdoc />
        public void ClearCache()
        {
            _nodesCache.Clear();
        }

        #region Overloaded operators

        public ArchiveTreeRoot this[string key]
        {
            get => ReadFrom(key);
            set => WriteTo(key, value);
        }

        #endregion

        /// <summary>
        /// Performs an initialization of e.g. cached files created in some use cases.
        /// </summary>
        /// <param name="force">True to force initialization, false to respect threshold.</param>
        internal static async void CheckInitialize(bool force = false)
        {
            // only clear cache if forced or threshold is exceeded
            if (force || Instance._nodesCache.Count > 10)
            {
                Instance.ClearCache();
                try
                {
                    var tempFolder = await FileUtils.GetTempFolderAsync(
                        TempFolder.Archives).ConfigureAwait(false);
                    await FileUtils.CleanFolderAsync(tempFolder).ConfigureAwait(false);
                }
                catch
                {
                    // ignore (no task is returned)
                }
            }
        }

        #region Singleton members

        private static readonly object LockObject = new object();
        private static RootNodeCache _instance;

        public static RootNodeCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        _instance = new RootNodeCache();
                    }
                }

                return _instance;
            }
        }

        private RootNodeCache()
        {
            _nodesCache = new Dictionary<string, ArchiveTreeRoot>();
        }

        #endregion
    }
}
