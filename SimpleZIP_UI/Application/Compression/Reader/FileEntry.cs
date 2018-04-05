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
namespace SimpleZIP_UI.Application.Compression.Reader
{
    /// <summary>
    /// Represents a file entry within a node.
    /// </summary>
    public class FileEntry : IArchiveEntry
    {
        internal string Key { get; }

        /// <inheritdoc />
        /// <summary>
        /// The key of the entry without the path.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        /// <summary>
        /// Defaults to false as this entry is not a node.
        /// </summary>
        public bool IsNode { get; } = false;

        /// <summary>
        /// The uncompressed size of this file entry.
        /// </summary>
        public ulong Size { get; }

        internal FileEntry(string key, string name, ulong size)
        {
            Key = key;
            Name = name;
            Size = size;
        }
    }
}
