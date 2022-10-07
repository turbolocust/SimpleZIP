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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleZIP_UI.Business.Compression.Reader
{
    public interface IArchiveReader : IDisposable
    {
        /// <summary>
        /// Opens the archive which is associated with this reader.
        /// </summary>
        /// <param name="password">The optional password of the archive.</param>
        /// <returns>An awaitable task.</returns>
        Task OpenArchiveAsync(string password = null);

        /// <summary>
        /// Reads each entry of the archive.
        /// </summary>
        /// <returns>An enumerable of <see cref="IArchiveEntry"/>.</returns>
        IEnumerable<IArchiveEntry> ReadArchive();
    }
}
