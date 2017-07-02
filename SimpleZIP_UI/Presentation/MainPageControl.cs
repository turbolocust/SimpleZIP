using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Presentation.Factory;
using SimpleZIP_UI.Presentation.View;

namespace SimpleZIP_UI.Presentation
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
        internal async Task<bool> CompressButtonAction()
        {
            var picker = PickerFactory.CreateCompressFilesOpenPicker();
            var files = await picker.PickMultipleFilesAsync();

            if (!(files?.Count > 0)) return false;
            NavigateTo(typeof(CompressionSummaryPage), files);
            return true;
        }

        /// <summary>
        /// Handles complex logic for the decompress button.
        /// </summary>
        internal async Task<bool> DecompressButtonAction()
        {
            var picker = PickerFactory.CreateDecompressFileOpenPicker();
            var files = await picker.PickMultipleFilesAsync();

            if (!(files?.Count > 0)) return false;
            NavigateTo(typeof(ExtractionSummaryPage), files);
            return true;
        }

        /// <summary>
        /// Navigates to the specified page type with a list of files as parameter.
        /// </summary>
        /// <param name="page">The source page type.</param>
        /// <param name="files">List consisting of <code>StorageFile</code>.</param>
        private void NavigateTo(Type page, IReadOnlyList<StorageFile> files)
        {
            ParentPage.Frame.Navigate(page, files);
        }
    }
}
