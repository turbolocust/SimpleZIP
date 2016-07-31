using System;
using System.Collections.Generic;
using Windows.Storage.Pickers;
using SimpleZIP_UI.Appl.Compression;
using SimpleZIP_UI.Exceptions;

namespace SimpleZIP_UI.Control
{
    /// <summary>
    /// Handles complex operations for the GUI controller and delegates them to the application layer.
    /// </summary>
    internal class MainPageControl
    {
        /// <summary>
        /// The algorithm that is used for compressing and decompressing operations.
        /// </summary>
        private ICompressionAlgorithm _compressionAlgorithm;

        /// <summary>
        /// Enumeration type to identify the specific algorithm.
        /// </summary>
        public enum Algorithm
        {
            Zip, Gzip, Tarball
        }

        /// <summary>
        /// Stores the file type for each enum type.
        /// </summary>
        private readonly Dictionary<string, Algorithm> _algorithmFileTypes = new Dictionary<string, Algorithm>();

        public MainPageControl()
        {
            _algorithmFileTypes.Add(".zip", Algorithm.Zip);
            _algorithmFileTypes.Add(".gzip", Algorithm.Gzip);
            _algorithmFileTypes.Add(".gz", Algorithm.Tarball);
        }

        /// <summary>
        /// Handles complex logic for the compress button.
        /// </summary>
        public async void CompressButtonAction()
        {
            //TODO

        }

        /// <summary>
        /// Handles complex logic for the decompress button.
        /// </summary>
        public async void DecompressButtonAction()
        {
            var picker = new FileOpenPicker()
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            // add each supported file type to the picker
            foreach (var fileType in _algorithmFileTypes)
            {
                picker.FileTypeFilter.Add(fileType.Key);
            }

            var file = await picker.PickSingleFileAsync();
            if (file != null) // system has now access to file
            {
                Algorithm value;
                // try to get enum type by file type
                if (!_algorithmFileTypes.TryGetValue(file.FileType, out value))
                {
                    SetStrategy(value);
                    if (_compressionAlgorithm != null)
                    {
                        // get the parent folder of the archive
                        var parent = await file.GetParentAsync();
                        if (parent != null)
                        {
                            // create new output folder with display name of archive
                            var outputFolder = await parent.CreateFolderAsync(file.DisplayName);
                            if (outputFolder != null)
                            {
                                // then extract archive to newly created output folder
                                _compressionAlgorithm.Extract(@file.Path, @outputFolder.Path);
                            }
                        }
                    }
                }
                else
                {
                    throw new InvalidFileTypeException();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void SetStrategy(Algorithm value)
        {
            switch (value) // assign correct instance to use the right algorithm
            {
                case Algorithm.Zip:
                    _compressionAlgorithm = Zipper.Instance;
                    break;

                case Algorithm.Gzip:
                    _compressionAlgorithm = GZipper.Instance;
                    break;

                case Algorithm.Tarball:
                    throw new NotImplementedException();

                default:
                    throw new InvalidFileTypeException();
            }
        }
    }
}
