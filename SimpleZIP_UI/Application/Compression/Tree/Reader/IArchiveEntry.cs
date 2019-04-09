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
    public interface IArchiveEntry
    {
        /// <summary>
        /// The key of the archive entry.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// True if this entry is a directory, false otherwise.
        /// </summary>
        bool IsDirectory { get; }

        /// <summary>
        /// The size of the entry in bytes.
        /// </summary>
        ulong Size { get; }

        /// <summary>
        /// Name of the file if already extracted. This may
        /// be <code>null</code> if not yet extracted.
        /// </summary>
        string FileName { get; set; }
    }
}
