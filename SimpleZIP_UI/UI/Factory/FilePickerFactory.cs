using Windows.Storage.Pickers;

namespace SimpleZIP_UI.UI.Factory
{
    internal class FilePickerFactory
    {
        private FilePickerFactory()
        {
            // currently holds static members only
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static FileOpenPicker CreateCompressFileOpenPicker()
        {
            var picker = new FileOpenPicker()
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
                FileTypeFilter = { "*" }
            };

            return picker;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static FileOpenPicker CreateDecompressFileOpenPicker()
        {
            var picker = new FileOpenPicker()
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            // add each supported file type to the picker
            foreach (var fileType in UI.Control.AlgorithmFileTypes)
            {
                picker.FileTypeFilter.Add(fileType.Key);
            }

            return picker;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static FolderPicker CreateFolderPicker()
        {
            var picker = new FolderPicker()
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            return picker;
        }
    }
}
