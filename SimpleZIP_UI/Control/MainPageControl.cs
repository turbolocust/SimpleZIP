using System;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Appl.Compression;
using SimpleZIP_UI.Control.Factory;

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
                compressionHandler.ExtractFromArchive(file);
            }
        }
    }
}
