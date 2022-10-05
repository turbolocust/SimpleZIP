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

using System.Collections.Concurrent;
using Serilog;
using SimpleZIP_UI.Application;
using SimpleZIP_UI.Application.Compression.TreeBuilder;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Presentation.Cache
{
    internal sealed class RootNodeCache : ICache<string, ArchiveTreeRoot>
    {
        private readonly ILogger _logger = Log.ForContext<RootNodeCache>();
        private readonly ConcurrentDictionary<string, ArchiveTreeRoot> _nodesCache;

        /// <inheritdoc />
        public void Write(string key, ArchiveTreeRoot node)
        {
            _logger.Debug("Writing node '{NodeName}' with key '{Key}' to cache", node, key);
            _nodesCache.AddOrUpdate(key, node, (k, oldValue) =>
            {
                _logger.Debug("Old value '{NodeName}'", oldValue);
                return node; // always use newer value
            });
        }

        /// <inheritdoc />
        public ArchiveTreeRoot Read(string key)
        {
            _logger.Debug("Trying to read key '{Key}' from cache", key);
            _nodesCache.TryGetValue(key, out var rootNode);
            return rootNode; // can be null
        }

        private void Clear()
        {
            _logger.Debug("Clearing cache");
            _nodesCache.Clear();
        }

        /// <summary>
        /// Performs an initialization of cached files, created and used in some use cases.
        /// </summary>
        /// <param name="force">True to force initialization, false to respect threshold.</param>
        internal static async void InitializeIfNeededOrForced(bool force = false)
        {
            // only clear cache if forced or threshold is exceeded
            if (force || Instance._nodesCache.Count > 10)
            {
                Instance.Clear();

                try
                {
                    Instance._logger.Debug("Purging temporary files in {TempFolder} folder", TempFolder.Archives);
                    var tempFolder = await FileUtils.GetTempFolderAsync(TempFolder.Archives).ConfigureAwait(false);
                    await FileUtils.CleanFolderAsync(tempFolder).ConfigureAwait(false);
                }
                catch
                {
                    // ignore (no task is returned)
                }
            }
        }

        #region Singleton members

        /// <summary>
        /// Lock object which is used for double-checked locking
        /// when retrieving the singleton instance of this class.
        /// </summary>
        private static readonly object LockObject = new object();

        /// <summary>
        /// Singleton instance of this class.
        /// </summary>
        private static volatile RootNodeCache _instance;

        /// <summary>
        /// Returns the singleton instance of this class. This property is thread-safe.
        /// </summary>
        internal static RootNodeCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        if (_instance == null) // double-check
                        {
                            _instance = new RootNodeCache();
                        }
                    }
                }

                return _instance;
            }
        }

        private RootNodeCache()
        {
            _nodesCache = new ConcurrentDictionary<string, ArchiveTreeRoot>();
        }

        #endregion
    }
}
