using System;
using Windows.Storage.Pickers;
using SimpleZIP_UI.Compression;

namespace SimpleZIP_UI.Control
{
    /// <summary>
    ///     Handles more complex operations for the GUI controller and delegates them to the application layer.
    /// </summary>
    internal class MainPageControl
    {
        /// <summary>
        /// 
        /// </summary>
        private ICompressionAlgorithm _compressionAlgorithm;

        /// <summary>
        /// 
        /// </summary>
        public enum Algorithms
        {
            Zip, Gzip, Tarball
        }

        /// <summary>
        /// 
        /// </summary>
        public async void CompressButtonAction(Algorithms algo)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            picker.FileTypeFilter.Add(".zip");
            picker.FileTypeFilter.Add((".gzip"));
            picker.FileTypeFilter.Add(".gz");

            var files = await picker.PickMultipleFilesAsync();
            if (files != null)
            {
                _compressionAlgorithm = Zipper.Instance;

            }
        }

        /// <summary>
        /// 
        /// </summary>
        public async void DecompressButtonAction(Algorithms algo)
        {
            var picker = new FileSavePicker()
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                _compressionAlgorithm = Zipper.Instance;
            }
        }
    }
}
