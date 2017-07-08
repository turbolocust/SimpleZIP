using System.Collections.Generic;

namespace SimpleZIP_UI.Application.Compression.Reader
{
    /// <summary>
    /// Represents a node in the archive's hierarchy.
    /// </summary>
    internal class Node : IArchiveEntry
    {
        /// <summary>
        /// Friendly name of this node. Can be set differently but should optimally 
        /// consist of the <see cref="Id"/> without path and file separators.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Defaults to true as this entry is a node.
        /// </summary>
        public bool IsNode { get; } = true;

        /// <summary>
        /// The identifier of this node including file separators.
        /// </summary>
        internal string Id { get; }

        /// <summary>
        /// Children of this node, which are nodes or entries.
        /// </summary>
        internal ICollection<IArchiveEntry> Children { get; }

        internal Node(string id)
        {
            Id = id;
            Children = new LinkedList<IArchiveEntry>();
        }

        protected bool Equals(Node other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Node)obj);
        }

        public override int GetHashCode()
        {
            return Id != null ? Id.GetHashCode() : 0;
        }
    }
}
