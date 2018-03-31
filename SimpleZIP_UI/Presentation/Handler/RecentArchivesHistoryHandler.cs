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
using System.Linq;
using SimpleZIP_UI.Presentation.View.Model;
using static SimpleZIP_UI.Presentation.View.Model.RecentArchiveModel;

namespace SimpleZIP_UI.Presentation.Handler
{
    internal class RecentArchivesHistoryHandler
    {
        public const string DefaultDateFormat = @"dd/MM/yyyy - hh:mm";

        internal static int MaxHistoryItems { get; } = 25;

        internal static RecentArchiveModelCollection GetHistory()
        {
            return Settings.TryGet(Settings.Keys.RecentArchivesKey, out string xml)
                ? RecentArchiveModelCollection.From(xml)
                : new RecentArchiveModelCollection();
        }

        internal static void SaveToHistory(string fileName, string location)
        {
            if (!Settings.TryGet(Settings.Keys.RecentArchivesKey, out string xml))
            {
                xml = string.Empty;
            }

            var collection = RecentArchiveModelCollection.From(xml);
            var whenUsed = DateTime.Now.ToString(DefaultDateFormat);
            var model = new RecentArchiveModel(whenUsed, fileName, location);
            var history = collection.Models.ToList();

            if (history.Count == MaxHistoryItems)
            {
                history.RemoveAt(history.Count - 1); // remove last
            }

            history.Insert(0, model); // insert new element at first position
            collection.Models = history.ToArray(); // update models

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
    }
}
