using System.Collections.Generic;

namespace SimpleZIP_UI.Application.Compression.Reader
{
    /// <summary>
    /// Represents a node in the archive's hierarchy.
    /// </summary>
    internal class Node
    {
        /// <summary>
        /// The identifier of this node including file separators.
        /// </summary>
        internal string Id { get; }

        /// <summary>
        /// Friendly name of this node. Can be set differently but should optimally 
        /// consist of the <see cref="Id"/> without path and file separators.
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// The entries of this node. See <see cref="Entry"/>.
        /// </summary>
        internal ICollection<Entry> Entries { get; }

        /// <summary>
        /// Children of this node, which are also nodes.
        /// </summary>
        internal ICollection<Node> Children { get; }

        internal Node(string id)
        {
            Id = id;
            Entries = new LinkedList<Entry>();
            Children = new LinkedList<Node>();
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
