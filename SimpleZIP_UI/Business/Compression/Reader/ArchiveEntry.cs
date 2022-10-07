// ==++==
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

using System;

namespace SimpleZIP_UI.Business.Compression.Reader
{
    /// <summary>
    /// Represents an entry of an archive that can either be a file
    /// or directory, which is indicated by <see cref="IsDirectory"/>.
    /// </summary>
    internal class ArchiveEntry : IArchiveEntry
    {
        /// <inheritdoc />
        public string Key { get; }

        /// <inheritdoc />
        public bool IsDirectory { get; }

        /// <inheritdoc />
        public ulong Size { get; }

        /// <inheritdoc />
        public DateTime? Modified { get; }

        /// <inheritdoc />
        public string FileName { get; set; }

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="key">The key of this entry.</param>
        /// <param name="isDirectory">True if entry is
        /// a directory, false otherwise.</param>
        /// <param name="size">The size of this entry in bytes.</param>
        /// <param name="modified">The last modification time of the entry.</param>
        public ArchiveEntry(string key, bool isDirectory, ulong size, DateTime? modified)
        {
            Key = key;
            IsDirectory = isDirectory;
            Size = size;
            Modified = modified;
        }
    }
}
