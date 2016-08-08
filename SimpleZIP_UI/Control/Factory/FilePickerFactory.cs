using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace SimpleZIP_UI.Control.Factory
{
    internal class FilePickerFactory
    {
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
            foreach (var fileType in Control.AlgorithmFileTypes)
            {
                picker.FileTypeFilter.Add(fileType.Key);
            }

            return picker;
        }
    }
}
