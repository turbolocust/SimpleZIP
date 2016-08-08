using System;
using System.Collections.Generic;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Appl.Compression.Algorithm;
using SimpleZIP_UI.Exceptions;

namespace SimpleZIP_UI.Control
{
    public abstract class Control
    {
        protected Frame RootFrame { get; set; }

        /// <summary>
        /// Enumeration type to identify the specific algorithm.
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
            AlgorithmFileTypes.Add(".gzip", Algorithm.Gzip);
            AlgorithmFileTypes.Add(".gz", Algorithm.TarGz);
            AlgorithmFileTypes.Add(".bz2", Algorithm.TarBz2);
        }

        protected Control(Frame rootFrame)
        {
            RootFrame = rootFrame;
        }
    }
}