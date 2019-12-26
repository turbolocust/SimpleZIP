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
using SimpleZIP_UI.Application.Compression;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage.Pickers;

namespace SimpleZIP_UI.Presentation.Factory
{
    internal static class PickerFactory
    {
        /// <summary>
        /// Folder picker which allows to select folders only.
        /// </summary>
        internal static FolderPicker FolderPicker => new FolderPicker
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            FileTypeFilter = { "*" }
        };

        /// <summary>
        /// Creates a new file open picker to select any file(s).
        /// </summary>
        /// <returns>The newly created file picker.</returns>
        internal static FileOpenPicker FileOpenPickerForAnyFile => new FileOpenPicker
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            FileTypeFilter = { "*" }
        };

        /// <summary>
        /// File picker to select any archive that may be decompressed.
        /// Only those files can be picked that are supported by the application 
        /// (see <see cref="Archives.ArchiveFileTypes"/>).
        /// </summary>
        internal static FileOpenPicker FileOpenPickerForArchives
        {
            get
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
        }

        /// <summary>
        /// Returns a list consisting of supported file types.
        /// </summary>
        /// <returns>A list of supported file types.</returns>
        internal static IList<string> GetListOfSupportedFileTypes()
        {
            return Archives.ArchiveFileTypes.Select(fileType => fileType.Key).ToList();
        }
    }
}
