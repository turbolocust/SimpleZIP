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

namespace SimpleZIP_UI.Application.Compression.Tree.Reader
{
    internal class ArchiveEntry : IArchiveEntry
    {
        /// <inheritdoc />
        public string Key { get; }

        /// <inheritdoc />
        public bool IsDirectory { get; }

        /// <inheritdoc />
        public ulong Size { get; }

        /// <inheritdoc />
        public string FileName { get; set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="key">The key of this entry.</param>
        /// <param name="isDirectory">True if entry is
        /// a directory, false otherwise.</param>
        /// <param name="size">The size of this entry in bytes.</param>
        public ArchiveEntry(string key, bool isDirectory, ulong size)
        {
            Key = key;
            IsDirectory = isDirectory;
            Size = size;
        }
    }
}
