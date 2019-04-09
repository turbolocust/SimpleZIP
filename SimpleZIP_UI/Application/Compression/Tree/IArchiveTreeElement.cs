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
namespace SimpleZIP_UI.Application.Compression.Tree
{
    internal interface IArchiveTreeElement
    {
        /// <summary>
        /// The unique identifier of this entry.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The friendly name of this entry.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// True if this entry is browsable (directory), false otherwise.
        /// </summary>
        bool IsBrowsable { get; }

        /// <summary>
        /// True if this entry is an archive, false otherwise.
        /// </summary>
        bool IsArchive { get; }
    }
}
