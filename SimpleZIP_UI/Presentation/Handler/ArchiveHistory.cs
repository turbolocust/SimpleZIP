﻿// ==++==
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

using SimpleZIP_UI.Business.Compression.Compressor;
using SimpleZIP_UI.Business.Hashing;
using SimpleZIP_UI.Business.Util;
using SimpleZIP_UI.Presentation.View.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using static SimpleZIP_UI.Presentation.View.Model.RecentArchiveModel;

namespace SimpleZIP_UI.Presentation.Handler
{
    /// <summary>
    /// Singleton class which manages the history of created archives.
    /// </summary>
    internal sealed class ArchiveHistory
    {
        /// <summary>
        /// The default date format that is being used by history items.
        /// </summary>
        public const string DefaultDateFormat = @"dd/MM/yyyy - hh:mm tt";

        /// <summary>
        /// The maximum allowed items in the archive history. This value
        /// is retrieved from the <see cref="StorageApplicationPermissions.MostRecentlyUsedList"/>.
        /// </summary>
        internal static uint MaxHistoryItems => MruList.MaximumItemsAllowed;

        /// <summary>
        /// Holds the most recently used items.
        /// </summary>
        private static StorageItemMostRecentlyUsedList MruList
            => StorageApplicationPermissions.MostRecentlyUsedList;

        /// <summary>
        /// Instance used for hashing the MRU token.
        /// </summary>
        private readonly IMessageDigestProvider _msgDigestProvider;

        /// <summary>
        /// Instance used for compressing/decompressing serialized history data.
        /// </summary>
        private readonly ICompressor<string, string> _compressor;

        #region Singleton members

        /// <summary>
        /// Lock object which is used for double-checked locking
        /// when retrieving the singleton instance of this class.
        /// </summary>
        private static readonly object LockObj = new object();

        /// <summary>
        /// Singleton instance of this class.
        /// </summary>
        private static volatile ArchiveHistory _instance;

        /// <summary>
        /// Returns the singleton instance of this class. This property is thread-safe.
        /// </summary>
        public static ArchiveHistory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObj)
                    {
                        if (_instance == null) // double-check
                        {
                            _instance = new ArchiveHistory();
                        }
                    }
                }

                return _instance;
            }
        }

        private ArchiveHistory()
        {
            _msgDigestProvider = new MessageDigestProvider();
            // Unicode encoding is important here, see the generated
            // XML file produced by System.Xml.Serialization
            _compressor = new StringGzipCompressor(Encoding.Unicode);
        }

        #endregion

        /// <summary>
        /// Checks whether the specified token exists in the history or not.
        /// </summary>
        /// <param name="mruToken">The token to be checked.</param>
        /// <returns>True if the specified token exists, false otherwise.</returns>
        internal bool ContainsItem(string mruToken)
        {
            return MruList.ContainsItem(mruToken);
        }

        /// <summary>
        /// Returns the specified folder from the history.
        /// </summary>
        /// <param name="mruToken">The folder that is associated with the specified token.</param>
        /// <returns>A task which returns a <see cref="StorageFolder"/>.</returns>
        internal async Task<StorageFolder> GetFolderAsync(string mruToken)
        {
            return await MruList.GetFolderAsync(mruToken);
        }

        /// <summary>
        /// Reads the history of recently created archives synchronously.
        /// </summary>
        /// <returns>Collection of <see cref="RecentArchiveModel"/>.</returns>
        internal RecentArchiveModelCollection GetHistory()
        {
            return GetSerializedHistory(out string xml)
                ? RecentArchiveModelCollection.From(xml)
                : new RecentArchiveModelCollection();
        }

        /// <summary>
        /// Reads the history of recently created archives asynchronously.
        /// </summary>
        /// <returns>A task which returns <see cref="RecentArchiveModelCollection"/>.</returns>
        internal async Task<RecentArchiveModelCollection> GetHistoryAsync()
        {
            return await Task.Run(GetHistory).ConfigureAwait(false);
        }

        /// <summary>
        /// Stores a new entry to the history for each item in the specified array
        /// consisting of file names. Each entry holds the specified <c>location</c>
        /// as well as the current datetime with the format as specified in <see cref="DefaultDateFormat"/>.
        /// </summary>
        /// <param name="folder">The folder to be stored with each entry.</param>
        /// <param name="fileNames">File names to be stored in history.</param>
        internal async Task<bool> SaveToHistoryAsync(StorageFolder folder, params string[] fileNames)
        {
            if (fileNames.IsNullOrEmpty()) return false;

            if (!GetSerializedHistory(out string xml))
            {
                xml = string.Empty;
            }

            var collection = RecentArchiveModelCollection.From(xml);
            var history = collection.Models.ToList();
            var whenUsed = DateTime.Now.ToString(DefaultDateFormat, CultureInfo.InvariantCulture);
            var models = new List<RecentArchiveModel>(fileNames.Length);

            foreach (string name in fileNames)
            {
                var mruToken = await CreateTokenAsync(folder.Path, name).ConfigureAwait(false);
                var model = new RecentArchiveModel(whenUsed, name, folder.Path, mruToken);
                models.Add(model);
            }

            // get maximum history size specified by user
            if (!Settings.TryGet(Settings.Keys.ArchiveHistorySize, out int size) || size > MaxHistoryItems)
            {
                size = (int)MaxHistoryItems;
            }

            collection.Models = UpdateHistory(history, models, size);
            MruList.Clear(); // always clear first

            // save folder (location) to Most Recently Used list
            foreach (var model in collection.Models)
            {
                MruList.AddOrReplace(model.MruToken, folder);
            }
            // also serialize separately to an XML file to be able to store
            // any information which StorageFile's do not have
            if (!string.IsNullOrEmpty(xml = collection.Serialize()))
            {
                StoreAwayCompressed(xml);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the specified model from the history (if exists).
        /// </summary>
        /// <param name="model">The model to be removed.</param>
        internal void RemoveFromHistory(RecentArchiveModel model)
        {
            if (GetSerializedHistory(out string xml))
            {
                var collection = RecentArchiveModelCollection.From(xml);
                var models = collection.Models.ToList();
                try
                {
                    var modelRemove = models.SingleOrDefault(m =>
                        m.MruToken.Equals(model.MruToken, StringComparison.Ordinal));
                    models.Remove(modelRemove);
                    // check if contains first to avoid exception
                    if (MruList.ContainsItem(model.MruToken))
                    {
                        MruList.Remove(model.MruToken);
                    }
                }
                catch
                {
                    // already removed, hence ignore
                }
                collection.Models = models.ToArray();
                // store away updated history
                if (!string.IsNullOrEmpty(xml = collection.Serialize()))
                {
                    StoreAwayCompressed(xml);
                }
            }
        }

        /// <summary>
        /// Clears the entire history.
        /// </summary>
        internal void ClearHistory()
        {
            MruList.Clear();
            Settings.PushOrUpdate(Settings.Keys.RecentArchivesKey, string.Empty);
        }

        private void StoreAwayCompressed(string xml)
        {
            string compressed = _compressor.Compress(xml);
            Settings.PushOrUpdate(Settings.Keys.RecentArchivesKey, compressed);
        }

        private bool GetSerializedHistory(out string xml)
        {
            bool found = Settings.TryGet(Settings.Keys
                .RecentArchivesKey, out string value);
            xml = string.Empty;

            try
            {
                if (found && !string.IsNullOrEmpty(value))
                {
                    xml = _compressor.Decompress(value);
                }
            }
            catch
            {
                found = false;
            }

            return found;
        }

        private async Task<string> CreateTokenAsync(string location, string name)
        {
            // build proper path (FullName) first
            string loc = location.Replace('/', '\\');
            var sb = new StringBuilder(loc);

            if (!loc.EndsWith("\\", StringComparison.Ordinal))
            {
                sb.Append('\\');
            }

            sb.Append(name);

            // project path to fixed length string to avoid
            // length limitation of MRU token
            var (_, hash) = await _msgDigestProvider.ComputeAsync(
                sb.ToString(), "SHA256").ConfigureAwait(false);

            return hash;
        }

        private static RecentArchiveModel[] UpdateHistory(List<RecentArchiveModel> history,
            List<RecentArchiveModel> models, int maxItems)
        {
            if (maxItems == 0)
            {
                history.Clear();
            }
            else
            {
                int numHistory = history.Count;
                int numModels = models.Count, newSize;

                if (numModels > maxItems) // strip away models
                {
                    int diff = numModels - maxItems;
                    models.RemoveRange(numModels - diff, diff);
                    numModels = models.Count;
                }
                if (numHistory > maxItems) // history got smaller
                {
                    int diff = numHistory - maxItems;
                    history.RemoveRange(maxItems, diff);
                    numHistory = history.Count;
                }
                // remove elements if new size exceeds specified one
                if ((newSize = numHistory + numModels) > maxItems)
                {
                    int overflow = newSize - maxItems;
                    for (int i = 0; i < overflow; ++i)
                    {
                        history.RemoveAt(history.Count - 1);
                    }
                }
                history.InsertRange(0, models); // insert from start to keep order
            }

            return history.ToArray();
        }
    }
}
