// ==++==
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

using SimpleZIP_UI.Application.Compression.Tree.Reader;
using System.Collections.Generic;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Model
{
    /// <summary>
    /// Represents an item that can be extracted.
    /// </summary>
    public class ExtractableItem
    {
        /// <summary>
        /// Friendly name of this item.
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// The archive which can be extracted.
        /// </summary>
        internal StorageFile Archive { get; }

        /// <summary>
        /// The password of the archive.
        /// </summary>
        internal string Password { get; set; }

        /// <summary>
        /// Optional list of entries to be extracted. If this is not 
        /// <code>null</code>, then only these entries will be extracted.
        /// </summary>
        internal IReadOnlyList<IArchiveEntry> Entries { get; }

        internal ExtractableItem(string name, StorageFile archive,
            IReadOnlyList<IArchiveEntry> entries = null)
        {
            Name = name;
            Archive = archive;
            Entries = entries;
        }
    }
}
