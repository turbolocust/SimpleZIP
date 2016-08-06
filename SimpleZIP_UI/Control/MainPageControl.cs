using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Appl.Compression;
using SimpleZIP_UI.Exceptions;

namespace SimpleZIP_UI.Control
{
    /// <summary>
    /// Handles complex operations for the GUI controller and delegates them to the application layer.
    /// </summary>
    internal class MainPageControl : Control
    {

        public MainPageControl(Frame rootFrame) : base(rootFrame)
        {
        }

        /// <summary>
        /// Handles complex logic for the compress button.
        /// </summary>
        public async void CompressButtonAction()
        {
            var picker = CreateCompressFileOpenPicker();

            var files = await picker.PickMultipleFilesAsync();
            if (files?.Count > 0) // must not be null and empty
            {
                RootFrame.Navigate(typeof(SummaryPage), files);
            }
        }

        /// <summary>
        /// Handles complex logic for the decompress button.
        /// </summary>
        public async void DecompressButtonAction()
        {
            var picker = CreateDecompressFileOpenPicker();

            var file = await picker.PickSingleFileAsync();
            if (file != null) // system has now access to file
            {
                Algorithm value;
                // try to get enum type by file type
                if (!AlgorithmFileTypes.TryGetValue(file.FileType, out value))
                {
                    SetStrategy(value);
                    if (CompressionAlgorithm != null)
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
                                CompressionAlgorithm.Extract(@file.Path, @outputFolder.Path);

                            }
                        }
                    }
                }
                else
                {
                    throw new InvalidFileTypeException("The selected file format is not supported.");
                }
            }
        }
    }
}
