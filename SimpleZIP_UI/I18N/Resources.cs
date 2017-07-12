using Windows.ApplicationModel.Resources;

namespace SimpleZIP_UI.I18N
{
    internal static class Resources
    {
        private static readonly ResourceLoader Loader;

        static Resources()
        {
            Loader = new ResourceLoader();
        }

        /// <summary>
        /// Gets the string with the specified name.
        /// </summary>
        /// <param name="name">The name of the string to get.</param>
        /// <returns>Resource string.</returns>
        internal static string GetString(string name)
        {
            return Loader.GetString(name);
        }

        /// <summary>
        /// Gets the string with the specified name and replaces the format items 
        /// with the string representation of the corresponding object in the specified array.
        /// </summary>
        /// <param name="name">The name of the string to get.</param>
        /// <param name="objects">Objects to be replaced with the format items.</param>
        /// <returns>Resource string.</returns>
        internal static string GetString(string name, params object[] objects)
        {
            var value = Loader.GetString(name);
            return string.Format(value, objects);
        }
    }
}
