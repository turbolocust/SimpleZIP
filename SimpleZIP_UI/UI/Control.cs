using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace SimpleZIP_UI.UI
{
    public abstract class Control
    {
        protected Page ParentPage { get; }

        /// <summary>
        /// Enumeration type to identify algorithms.
        /// </summary>
        public enum Algorithm
        {
            Zip, Gzip, TarGz, TarBz2
        }

        /// <summary>
        /// Stores the file type for each enum type.
        /// </summary>
        public static readonly Dictionary<string, Algorithm> AlgorithmFileTypes = new Dictionary<string, Algorithm>();

        static Control()
        {
            AlgorithmFileTypes.Add(".zip", Algorithm.Zip);
            AlgorithmFileTypes.Add(".z", Algorithm.Gzip);
            AlgorithmFileTypes.Add(".gz", Algorithm.Gzip);
            AlgorithmFileTypes.Add(".tgz", Algorithm.TarGz);
            AlgorithmFileTypes.Add(".bz2", Algorithm.TarBz2);
            AlgorithmFileTypes.Add(".tbz2", Algorithm.TarBz2);
        }

        protected Control(Page parent)
        {
            ParentPage = parent;
        }
    }
}