using System.Collections.Generic;

namespace SimpleZIP_UI.Application.Compression.Reader
{
    /// <summary>
    /// Represents a node in the archive's hierarchy.
    /// </summary>
    internal class Node
    {
        internal string Id { get; }

        internal ICollection<Entry> Entries { get; }

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
