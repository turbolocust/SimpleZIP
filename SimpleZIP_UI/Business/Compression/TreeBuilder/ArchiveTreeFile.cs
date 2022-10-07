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

using SimpleZIP_UI.Business.Compression.Reader;
using SimpleZIP_UI.Business.Util;
using System;

namespace SimpleZIP_UI.Business.Compression.TreeBuilder
{
    /// <summary>
    /// Represents a file entry within a node.
    /// </summary>
    public class ArchiveTreeFile : IArchiveTreeElement
    {
        #region Properties

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        /// <summary>
        /// The Id (key) of this entry without the path.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        /// <summary>
        /// Defaults to false as this entry is not a node.
        /// </summary>
        public bool IsBrowsable { get; } = false;

        /// <inheritdoc />
        public bool IsArchive { get; }

        /// <summary>
        /// The uncompressed size of this file entry.
        /// </summary>
        public ulong Size { get; }

        /// <summary>
        /// The last modification time of the entry. Can be <c>null</c>.
        /// </summary>
        public DateTime? LastModified { get; }

        /// <summary>
        /// Name of the file if already extracted. This may
        /// be <c>null</c> if not yet extracted.
        /// </summary>
        internal string FileName { get; set; }

        #endregion

        #region Constructors

        internal ArchiveTreeFile(
            string id,
            string name,
            ulong size,
            bool isArchive = false)
        {
            Id = id;
            Name = name;
            Size = size;
            IsArchive = isArchive;
        }

        internal ArchiveTreeFile(
            string id,
            string name,
            ulong size,
            DateTime? lastModified,
            bool isArchive = false)
            : this(id, name, size, isArchive)
        {
            LastModified = lastModified;
        }

        #endregion

        #region Equals and HashCode

        protected bool Equals(ArchiveTreeFile other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return string.Equals(Id, other.Id, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ArchiveTreeFile)obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id != null ? Id.GetHashCode(StringComparison.Ordinal) : 0;
        }

        #endregion

        /// <summary>
        /// Converts this object to an instance of <see cref="IArchiveEntry"/>.
        /// </summary>
        /// <returns>An instance of <see cref="IArchiveEntry"/>.</returns>
        internal IArchiveEntry ToArchiveEntry()
        {
            return new ArchiveEntry(Id, IsBrowsable, Size, LastModified);
        }

        /// <summary>
        /// Factory method which creates a new instance of <see cref="ArchiveTreeFile"/>.
        /// This method also checks if the specified name consists of a filename
        /// extension which indicates that it is an archive. In this case,
        /// <see cref="IsArchive"/> is set to true.
        /// </summary>
        /// <param name="id">The identifier of the file entry.</param>
        /// <param name="name">The name of the entry.</param>
        /// <param name="size">The size of the entry.</param>
        /// <param name="lastModified">The time of the last modification of the entry.</param>
        /// <returns>A new instance of <see cref="ArchiveTreeFile"/>.</returns>
        public static ArchiveTreeFile CreateFileEntry(string id, string name, ulong size, DateTime? lastModified)
        {
            string ext = FileUtils.GetFileNameExtension(name);
            var archiveType = Archives.DetermineArchiveTypeByFileExtension(ext);
            bool isArchive = archiveType != Archives.ArchiveType.Unknown;
            return new ArchiveTreeFile(id, name, size, lastModified, isArchive);
        }
    }
}
