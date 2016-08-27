using System;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Common.Compression;
using SimpleZIP_UI.Exceptions;
using SimpleZIP_UI.UI.Factory;

namespace SimpleZIP_UI.UI
{
    /// <summary>
    /// Handles complex operations for the corresponding GUI controller.
    /// </summary>
    internal class MainPageControl : Control
    {

        public MainPageControl(Page parent) : base(parent)
        {
        }

        /// <summary>
        /// Handles complex logic for the compress button.
        /// </summary>
        public async void CompressButtonAction()
        {
            var picker = PickerFactory.CreateCompressFileOpenPicker();

            var files = await picker.PickMultipleFilesAsync();
            if (files?.Count > 0) // must not be null and empty
            {
                ParentPage.Frame.Navigate(typeof(SummaryPage), files);
            }
        }

        /// <summary>
        /// Handles complex logic for the decompress button.
        /// </summary>
        public async void DecompressButtonAction()
        {
            var picker = PickerFactory.CreateDecompressFileOpenPicker();

            var file = await picker.PickSingleFileAsync();
            if (file != null) // system has now access to file
            {
                var compressionHandler = CompressionHandler.Instance;
                var busyIndicator = BusyIndicator.Start("Operation in progress. Please wait . . .");
                var duration = 0; // to measure the time of the extraction operation

                try
                {
                    duration = await compressionHandler.ExtractFromArchive(file);
                }
                catch (UnauthorizedAccessException)
                {
                    DialogFactory.CreateInformationDialog("Oops!",
                        "Insufficient rights to extract archive here.\n\n" +
                        "Please move archive to a different location and try again.");
                }
                catch (InvalidFileTypeException)
                {
                    DialogFactory.CreateInformationDialog("Oops!", "The chosen file format is not supported.");
                }
                finally
                {
                    busyIndicator.Close();
                }

                if (duration > 0)
                {
                    DialogFactory.CreateInformationDialog("Success",
                        "The operation succeeded.\n\n" +
                        "Files have been extracted to the specified subfolder at the archive's location.\n\n" +
                        "Total duration: " + duration);
                }
            }
        }
    }
}
