// ==++==
// 
// Copyright (C) 2017 Matthias Fussenegger
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Presentation.Factory;
using SimpleZIP_UI.Presentation.View;

namespace SimpleZIP_UI.Presentation.Controller
{
    internal class MainPageController : BaseController
    {
        internal enum MainPageActionType
        {
            Compress, Decompress, OpenArchive
        }

        internal MainPageController(Page parent) : base(parent)
        {
        }

        /// <summary>
        /// Performs a concrete action based on the specified <see cref="MainPageActionType"/>.
        /// </summary>
        /// <param name="type">The type of action to be performed.</param>
        /// <returns>Task which returns true on success and false otherwise.</returns>
        internal async Task<bool> PerformAction(MainPageActionType type)
        {
            bool success;
            try
            {
                switch (type)
                {
                    case MainPageActionType.Compress:
                        success = await CompressButtonAction();
                        break;
                    case MainPageActionType.Decompress:
                        success = await DecompressButtonAction();
                        break;
                    case MainPageActionType.OpenArchive:
                        success = await OpenArchiveButtonAction();
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
            catch (Exception)
            {
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Performs an action when the compress button has been tapped.
        /// </summary>
        private async Task<bool> CompressButtonAction()
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
        private async Task<bool> DecompressButtonAction()
        {
            var picker = PickerFactory.CreateDecompressFileOpenPicker();
            var files = await picker.PickMultipleFilesAsync();

            if (!(files?.Count > 0)) return false;
            ParentPage.Frame.Navigate(typeof(DecompressionSummaryPage), ConvertFiles(files));
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
            items.AddRange(files.Select(file => new ExtractableItem(file.Name, file)));
            return items;
        }

        /// <summary>
        /// Performs an action when the button for opening an archive has been tapped.
        /// </summary>
        private async Task<bool> OpenArchiveButtonAction()
        {
            var picker = PickerFactory.CreateDecompressFileOpenPicker();
            var file = await picker.PickSingleFileAsync();

            if (file == null) return false;
            ParentPage.Frame.Navigate(typeof(BrowseArchivePage), file);
            return true;
        }
    }
}
