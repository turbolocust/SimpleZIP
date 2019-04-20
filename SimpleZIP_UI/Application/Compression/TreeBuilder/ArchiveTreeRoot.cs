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

using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.TreeBuilder
{
    /// <inheritdoc />
    /// <summary>
    /// Represents the root of the archive tree.
    /// </summary>
    internal class ArchiveTreeRoot : ArchiveTreeNode
    {
        /// <summary>
        /// The associated archive file.
        /// </summary>
        internal StorageFile Archive { get; }

        /// <summary>
        /// The password of the archive. May be <code>null</code>.
        /// </summary>
        internal string Password { get; }

        internal ArchiveTreeRoot(string id, StorageFile archive,
            string password = null) : base(id)
        {
            Archive = archive;
            Password = password;
        }
    }
}
