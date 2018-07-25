// ==++==
// 
// Copyright (C) 2018 Matthias Fussenegger
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
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Compression;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.View;

namespace SimpleZIP_UI.Presentation.Handler
{
    internal sealed class ShareTargetHandler
    {
        /// <summary>
        /// Handles the specified <see cref="ShareOperation"/>.
        /// </summary>
        /// <param name="shareOp">The share operation to be handled.</param>
        /// <returns>A task which returns an error message as string.</returns>
        internal async Task<string> Handle(ShareOperation shareOp)
        {
            string message = string.Empty;

            shareOp.ReportStarted();

            var storageItems = await shareOp.Data.GetStorageItemsAsync();
            shareOp.ReportDataRetrieved();

            if (!storageItems.IsNullOrEmpty())
            {
                var files = (from item in storageItems
                             where item.IsOfType(StorageItemTypes.File)
                             select item as StorageFile).ToList().AsReadOnly();

                var rootFrame = new Frame();
                var dest = typeof(ShareTargetOptionsPage);

                var args = ConsistsOfArchivesOnly(files)
                    ? NavigationArgs.ForDecompressionSummaryPage(files, shareOp)
                    : new NavigationArgs(files, shareOp);

                rootFrame.Navigate(dest, args);
                Window.Current.Content = rootFrame;
                Window.Current.Activate();
            }
            else
            {
                message = I18N.Resources.GetString("ErrorNoFilesProvided/Text");
            }

            return message;
        }

        private static bool ConsistsOfArchivesOnly(IEnumerable<StorageFile> files)
        {
            return files.All(IsArchiveFile);
        }

        private static bool IsArchiveFile(StorageFile file)
        {
            string ext = FileUtils.GetFileNameExtension(file.Path);
            var type = Archives.DetermineArchiveTypeByFileExtension(ext);
            return type != Archives.ArchiveType.Unknown;
        }
    }
}
