using System.Collections.Generic;
using System.Linq;

namespace SimpleZIP_UI.Application.Util
{
    internal static class CollectionUtils
    {
        /// <summary>
        /// Checks whether this collection is <code>null</code> or empty.
        /// </summary>
        /// <typeparam name="T">The type of elements in this collection.</typeparam>
        /// <param name="collection">The collection to be checked.</param>
        /// <returns>True if collection is <code>null</code> or empty, false otherwise.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }
    }
}
