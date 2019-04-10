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

using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Compression.TreeBuilder;

namespace SimpleZIP_UI.Presentation.View.Model
{
    /// <summary>
    /// Represents a list box item for the <see cref="BrowseArchivePage"/>.
    /// </summary>
    public class ArchiveEntryModel
    {
        /// <summary>
        /// The type of the archive entry this model represents.
        /// </summary>
        public ArchiveEntryModelType EntryType { get; }

        /// <summary>
        /// A friendly name which will be displayed in the ListBox.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The symbol that will be displayed together with <see cref="DisplayName"/>
        /// in the list box.
        /// </summary>
        public Symbol Symbol { get; set; }

        /// <summary>
        /// Constructs a new model for the ListBox in <see cref="BrowseArchivePage"/>.
        /// </summary>
        /// <param name="type">The type of the archive entry this model should represent.</param>
        /// <param name="displayName">Friendly name of the model to be displayed.</param>
        public ArchiveEntryModel(ArchiveEntryModelType type, string displayName)
        {
            EntryType = type;
            DisplayName = displayName;
        }

        /// <summary>
        /// Factory method for creating a new instance of <see cref="ArchiveEntryModel"/>.
        /// </summary>
        /// <param name="entry">Entry of which to extract information for the model.</param>
        /// <returns>A new instance of <see cref="ArchiveEntryModel"/>.</returns>
        internal static ArchiveEntryModel Create(IArchiveTreeElement entry)
        {
            ArchiveEntryModelType type;
            var symbol = Symbol.Preview;

            if (entry.IsArchive)
            {
                type = ArchiveEntryModelType.Archive;
                symbol = Symbol.OpenLocal;
            }
            else if (entry.IsBrowsable)
            {
                type = ArchiveEntryModelType.Node;
                symbol = Symbol.Folder;
            }
            else
            {
                type = ArchiveEntryModelType.File;
            }

            return new ArchiveEntryModel(type, entry.Name)
            {
                Symbol = symbol
            };
        }

        public enum ArchiveEntryModelType
        {
            File, Archive, Node
        }
    }
}
