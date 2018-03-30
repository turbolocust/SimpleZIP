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

namespace SimpleZIP_UI.Presentation.View.Model
{
    internal class RecentArchiveModel
    {
        /// <summary>
        /// The last time the archive was used as date/time.
        /// </summary>
        public string WhenUsed { get; }

        /// <summary>
        /// The name of the file to be displayed.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The physical location (full path) of the file.
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// Constructs a new model for the ListBox in <see cref="MainPage"/>.
        /// </summary>
        /// <param name="whenUsed">Date/Time when archive was used.</param>
        /// <param name="fileName">The name of the file to be displayed.</param>
        /// <param name="location">The location of the file to be displayed.</param>
        public RecentArchiveModel(string whenUsed, string fileName, string location)
        {
            WhenUsed = whenUsed;
            FileName = fileName;
            Location = location;
        }
    }
}
