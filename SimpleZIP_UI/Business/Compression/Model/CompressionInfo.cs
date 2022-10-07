// ==++==
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
using System.Collections.Generic;
using Windows.Storage;

namespace SimpleZIP_UI.Business.Compression.Model
{
    internal class CompressionInfo : OperationInfo
    {
        /// <summary>
        /// The type of the archive. See <see cref="Archives.ArchiveType"/>.
        /// </summary>
        internal Archives.ArchiveType ArchiveType { get; }

        /// <summary>
        /// The name of the archive to be created.
        /// </summary>
        internal string ArchiveName { get; set; }

        /// <summary>
        /// List of selected files to be compressed.
        /// </summary>
        internal IReadOnlyList<StorageFile> SelectedFiles { get; set; }

        internal CompressionInfo(Archives.ArchiveType archiveType, ulong size) : base(size)
        {
            ArchiveType = archiveType;
        }
    }
}
