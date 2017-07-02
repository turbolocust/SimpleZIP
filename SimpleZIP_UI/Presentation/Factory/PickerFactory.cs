using System.Collections.Generic;
using System.Linq;
using Windows.Storage.Pickers;

namespace SimpleZIP_UI.Presentation.Factory
{
    internal class PickerFactory
    {
        private PickerFactory()
        {
            // holds static members only
        }

        /// <summary>
        /// Creates a new file open picker to select any file(s) that may be compressed.
        /// </summary>
        /// <returns>The newly created file picker.</returns>
        public static FileOpenPicker CreateCompressFilesOpenPicker()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                FileTypeFilter = { "*" }
            };

            return picker;
        }

        /// <summary>
        /// Creates a new file picker to select any archive that may be decompressed.
        /// Only those files can be picked that are supported by the application (see <code>AlgorithmFileTypes</code> enum).
        /// </summary>
        /// <returns>The newly created file picker.</returns>
        public static FileOpenPicker CreateDecompressFileOpenPicker()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            foreach (var fileType in GetListOfSupportedFileTypes())
            {
                picker.FileTypeFilter.Add(fileType);
            }

            return picker;
        }

        /// <summary>
        /// Creates a new folder picker to select any folder.
        /// </summary>
        /// <returns>The newly created folder picker.</returns>
        public static FolderPicker CreateFolderPicker()
        {
            var picker = new FolderPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeFilter = { "*" }
            };

            return picker;
        }

        /// <summary>
        /// Returns a list consisting of supported file types.
        /// </summary>
        /// <returns>A list of supported file types.</returns>
        public static IList<string> GetListOfSupportedFileTypes()
        {
            return BaseControl.AlgorithmFileTypes.Select(fileType => fileType.Key).ToList();
        }

    }
}
