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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.View.Model;
using static SimpleZIP_UI.Presentation.View.Model.RecentArchiveModel;

namespace SimpleZIP_UI.Presentation.Handler
{
    internal class ArchiveHistoryHandler
    {
        public const string DefaultDateFormat = @"dd/MM/yyyy - hh:mm";

        internal static int MaxHistoryItems { get; } = 64;

        internal static RecentArchiveModelCollection GetHistory()
        {
            const string key = Settings.Keys.RecentArchivesKey;
            return Settings.TryGet(key, out string xml)
                ? RecentArchiveModelCollection.From(xml)
                : new RecentArchiveModelCollection();
        }

        internal static async Task<RecentArchiveModelCollection> GetHistoryAsync()
        {
            const string key = Settings.Keys.RecentArchivesKey;
            return await Task.Run(() => Settings.TryGet(key, out string xml)
                ? RecentArchiveModelCollection.From(xml)
                : new RecentArchiveModelCollection());
        }

        internal static void SaveToHistory(string location, params string[] fileNames)
        {
            if (fileNames.IsNullOrEmpty()) return;

            if (!Settings.TryGet(Settings.Keys.RecentArchivesKey, out string xml))
            {
                xml = string.Empty;
            }

            var collection = RecentArchiveModelCollection.From(xml);
            var history = collection.Models.ToList();
            var whenUsed = DateTime.Now.ToString(DefaultDateFormat);
            var models = new List<RecentArchiveModel>(fileNames.Length);
            models.AddRange(fileNames.Select(name => new RecentArchiveModel(whenUsed, name, location)));

            // get maximum history size specified by user
            if (!Settings.TryGet(Settings.Keys.ArchiveHistorySize, out int size) || size > MaxHistoryItems)
            {
                size = MaxHistoryItems;
            }

            collection.Models = UpdateHistory(history, models, size);

            if (!string.IsNullOrEmpty(xml = collection.Serialize()))
            {
                Settings.PushOrUpdate(Settings.Keys.RecentArchivesKey, xml);
            }
        }

        internal static void RemoveFromHistory(RecentArchiveModel model)
        {
            if (Settings.TryGet(Settings.Keys.RecentArchivesKey, out string xml))
            {
                var collection = RecentArchiveModelCollection.From(xml);
                var models = collection.Models.ToList();
                models.Remove(model); // ignore return value
                collection.Models = models.ToArray();
                // store away updated history
                if (!string.IsNullOrEmpty(xml = collection.Serialize()))
                {
                    Settings.PushOrUpdate(Settings.Keys.RecentArchivesKey, xml);
                }
            }
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
                }
                if (numHistory > maxItems) // history got smaller
                {
                    int diff = numHistory - maxItems;
                    history.RemoveRange(maxItems - 1, diff);
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
                history.InsertRange(0, models); // insert from start
            }

            return history.ToArray();
        }
    }
}
