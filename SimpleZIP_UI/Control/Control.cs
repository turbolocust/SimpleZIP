using System;
using System.Collections.Generic;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Appl.Compression;
using SimpleZIP_UI.Exceptions;

namespace SimpleZIP_UI.Control
{
    public abstract class Control
    {
        protected Frame RootFrame { get; set; }

        /// <summary>
        /// The algorithm that is used for compressing and decompressing operations.
        /// </summary>
        protected ICompressionAlgorithm CompressionAlgorithm;

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
        protected static readonly Dictionary<string, Algorithm> AlgorithmFileTypes = new Dictionary<string, Algorithm>();

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        protected void SetStrategy(Algorithm value)
        {
            switch (value) // assign correct instance to use the right algorithm
            {
                case Algorithm.Zip:
                    CompressionAlgorithm = Zipper.Instance;
                    break;

                case Algorithm.Gzip:
                    CompressionAlgorithm = GZipper.Instance;
                    break;
                case Algorithm.TarGz:
                    break;
                case Algorithm.TarBz2:
                    break;
                default:
                    throw new InvalidFileTypeException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected static FileOpenPicker CreateCompressFileOpenPicker()
        {
            var picker = new FileOpenPicker()
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                FileTypeFilter = { "*" }
            };

            return picker;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected static FileOpenPicker CreateDecompressFileOpenPicker()
        {
            var picker = new FileOpenPicker()
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            // add each supported file type to the picker
            foreach (var fileType in AlgorithmFileTypes)
            {
                picker.FileTypeFilter.Add(fileType.Key);
            }

            return picker;
        }
    }
}