using System;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.UI.Factory;
using SimpleZIP_UI.UI.View;

namespace SimpleZIP_UI.UI
{
    /// <summary>
    /// Handles complex operations for the corresponding GUI controller.
    /// </summary>
    internal class MainPageControl : BaseControl
    {
        internal MainPageControl(Page parent) : base(parent)
        {
        }

        /// <summary>
        /// Handles complex logic for the compress button.
        /// </summary>
        internal async void CompressButtonAction()
        {
            var picker = PickerFactory.CreateCompressFileOpenPicker();

            var files = await picker.PickMultipleFilesAsync();
            if (files?.Count > 0) // must not be null and empty
            {
                ParentPage.Frame.Navigate(typeof(CompressionSummaryPage), files);
            }
        }

        /// <summary>
        /// Handles complex logic for the decompress button.
        /// </summary>
        internal async void DecompressButtonAction()
        {
            var picker = PickerFactory.CreateDecompressFileOpenPicker();

            var files = await picker.PickMultipleFilesAsync();
            if (files != null) // must not be null
            {
                ParentPage.Frame.Navigate(typeof(ExtractionSummaryPage), files);
            }
        }
    }
}
