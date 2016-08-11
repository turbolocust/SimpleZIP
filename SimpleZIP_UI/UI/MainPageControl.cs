using System;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Common.Compression;
using SimpleZIP_UI.UI.Factory;

namespace SimpleZIP_UI.UI
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
            var picker = FilePickerFactory.CreateCompressFileOpenPicker();

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
            var picker = FilePickerFactory.CreateDecompressFileOpenPicker();

            var file = await picker.PickSingleFileAsync();
            if (file != null) // system has now access to file
            {
                var compressionHandler = CompressionHandler.Instance;
                // show busy indicator while operation is in progress
                var indicator = BusyIndicator.Start("Operation in progress. Please wait . . .");
                var duration = await compressionHandler.ExtractFromArchive(file);
                // close the busy indicator
                indicator.Close();
                if (duration > 0)
                {
                    MessageDialogFactory.CreateInformationDialog("Success",
                        "The operation succeeded.\n\nFiles have been extracted to a subfolder at the archive's location.\n\nTotal duration: " +
                        duration);
                }
            }
        }
    }
}
