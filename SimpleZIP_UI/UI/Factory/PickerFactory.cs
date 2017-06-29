using Windows.Storage.Pickers;

namespace SimpleZIP_UI.UI.Factory
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

            // add each supported file type to the picker
            foreach (var fileType in BaseControl.AlgorithmFileTypes)
            {
                picker.FileTypeFilter.Add(fileType.Key);
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
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            return picker;
        }
    }
}
