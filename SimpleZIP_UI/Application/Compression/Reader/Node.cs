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
using System.Collections.Generic;

namespace SimpleZIP_UI.Application.Compression.Reader
{
    /// <summary>
    /// Represents a folder in the archive's hierarchy.
    /// </summary>
    internal class Node : IArchiveEntry
    {
        /// <inheritdoc />
        /// <summary>
        /// Friendly name of this node. Can be set differently but should ideally
        /// consist of the <see cref="P:SimpleZIP_UI.Application.Compression.Reader.Node.Id" /> 
        /// without path and file separators.
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Defaults to true as this entry is a node.
        /// </summary>
        public bool IsNode { get; } = true;

        /// <summary>
        /// The identifier of this node, including file separators.
        /// </summary>
        internal string Id { get; }

        /// <summary>
        /// Children of this node, which can be nodes, entries or both.
        /// </summary>
        internal ISet<IArchiveEntry> Children { get; }

        internal Node(string id)
        {
            Id = id;
            Children = new HashSet<IArchiveEntry>();
        }

        protected bool Equals(Node other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Node)obj);
        }

        public override int GetHashCode()
        {
            return Id != null ? Id.GetHashCode() : 0;
        }
    }
}
