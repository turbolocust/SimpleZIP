using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Presentation.Factory;
using SimpleZIP_UI.Presentation.View;

namespace SimpleZIP_UI.Presentation
{
    internal class MainPageControl : BaseControl
    {
        internal MainPageControl(Page parent) : base(parent)
        {
        }

        /// <summary>
        /// Performs an action when the compress button has been tapped.
        /// </summary>
        internal async Task<bool> CompressButtonAction()
        {
            var picker = PickerFactory.CreateCompressFilesOpenPicker();
            var files = await picker.PickMultipleFilesAsync();

            if (!(files?.Count > 0)) return false;
            ParentPage.Frame.Navigate(typeof(CompressionSummaryPage), files);
            return true;
        }

        /// <summary>
        /// Performs an action when the decompress button has been tapped.
        /// </summary>
        internal async Task<bool> DecompressButtonAction()
        {
            var picker = PickerFactory.CreateDecompressFileOpenPicker();
            var files = await picker.PickMultipleFilesAsync();

            if (!(files?.Count > 0)) return false;
            ParentPage.Frame.Navigate(typeof(ExtractionSummaryPage), files);
            return true;
        }

        /// <summary>
        /// Performs an action when the button for opening an archive has been tapped.
        /// </summary>
        internal async Task<bool> OpenArchiveButtonAction()
        {
            var picker = PickerFactory.CreateDecompressFileOpenPicker();
            var file = await picker.PickSingleFileAsync();

            if (file == null) return false;
            ParentPage.Frame.Navigate(typeof(BrowseArchivePage), file);
            return true;
        }
    }
}
