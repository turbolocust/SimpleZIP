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

using System.Text;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Options
{
    /// <summary>
    /// Options for archiving operations. See <see cref="ICompressionAlgorithm"/>.
    /// </summary>
    public interface IOptions
    {
        /// <summary>
        /// True to leave the stream to read or write
        /// the archive open, false otherwise.
        /// </summary>
        bool LeaveStreamOpen { get; }

        /// <summary>
        /// The encoding of the entry names to be written or extracted.
        /// </summary>
        Encoding ArchiveEncoding { get; }
    }
}
