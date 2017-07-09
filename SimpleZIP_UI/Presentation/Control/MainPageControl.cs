using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Presentation.Factory;
using SimpleZIP_UI.Presentation.View;

namespace SimpleZIP_UI.Presentation.Control
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
            ParentPage.Frame.Navigate(typeof(ExtractionSummaryPage), ConvertFiles(files));
            return true;
        }

        /// <summary>
        /// Converts the specified collection to objects of type <see cref="ExtractableItem"/>.
        /// </summary>
        /// <param name="files">The files to be converted.</param>
        /// <returns>A list which consists of <see cref="ExtractableItem"/> objects.</returns>
        private static IReadOnlyList<ExtractableItem> ConvertFiles(IReadOnlyCollection<StorageFile> files)
        {
            var items = new List<ExtractableItem>(files.Count);
            items.AddRange(files.Select(file => new ExtractableItem(file.DisplayName, file)));
            return items;
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
