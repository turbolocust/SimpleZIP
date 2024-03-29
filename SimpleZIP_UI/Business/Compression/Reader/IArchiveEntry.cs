﻿// ==++==
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
    /// An archive entry which is used by <see cref="IArchiveReader"/>.
    /// </summary>
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
        /// The last modification time of this entry. Can be <c>null</c>.
        /// </summary>
        DateTime? Modified { get; }

        /// <summary>
        /// Name of the file if already extracted. This may
        /// be <c>null</c> if not yet extracted.
        /// </summary>
        string FileName { get; set; }
    }
}
