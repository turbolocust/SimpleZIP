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

namespace SimpleZIP_UI_TEST
{
    internal struct NameIndexPair : IEquatable<NameIndexPair>
    {
        /// <summary>
        /// The index that is associated with <see cref="Name"/>.
        /// </summary>
        internal int Index { get; }

        /// <summary>
        /// The name that is associated with <see cref="Index"/>.
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// Creates a new pair that associates a name with an index and vice versa.
        /// </summary>
        /// <param name="index">The index to be associated with <paramref name="name"/>.</param>
        /// <param name="name">The name to be associated with <paramref name="index"/>.</param>
        internal NameIndexPair(int index, string name)
            => (Index, Name) = (index, name);

        /// <inheritdoc />
        public bool Equals(NameIndexPair other)
        {
            return Index == other.Index && Name == other.Name;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is NameIndexPair other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Index * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public static bool operator ==(NameIndexPair left, NameIndexPair right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NameIndexPair left, NameIndexPair right)
        {
            return !left.Equals(right);
        }
    }
}
