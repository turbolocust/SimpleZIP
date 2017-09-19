﻿// ==++==
// 
// Copyright (C) 2017 Matthias Fussenegger
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

namespace SimpleZIP_UI.Presentation.View.Model
{
    /// <summary>
    /// Represents a list box item for the <see cref="BrowseArchivePage"/>.
    /// </summary>
    public class ArchiveEntryModel
    {
        /// <summary>
        /// True if this model represents a node.
        /// </summary>
        public bool IsNode { get; }

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
        /// <param name="isNode">True if this model represents a node (folder).</param>
        /// <param name="displayName">Friendly name of the model to be displayed.</param>
        public ArchiveEntryModel(bool isNode, string displayName)
        {
            IsNode = isNode;
            DisplayName = displayName;
        }
    }
}
