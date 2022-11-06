// ==++==
// 
// Copyright (C) 2020 Matthias Fussenegger
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

using SimpleZIP_UI.Business;
using SimpleZIP_UI.Business.Compression;
using SimpleZIP_UI.Business.Util;
using SimpleZIP_UI.Presentation.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Serilog;

namespace SimpleZIP_UI.Presentation.Handler
{
    internal static class ShareTargetHandler
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(ShareTargetHandler));

        /// <summary>
        /// Handles the specified <see cref="ShareOperation"/>.
        /// </summary>
        /// <param name="shareOp">The share operation to be handled.</param>
        /// <returns>An awaitable task.</returns>
        internal static async Task Handle(ShareOperation shareOp)
        {
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

                var args = await ConsistsOfArchivesOnly(files)
                    ? new FilesNavigationArgs(files, shareOp, true)
                    : new FilesNavigationArgs(files, shareOp);

                rootFrame.Navigate(dest, args);
                Window.Current.Content = rootFrame;
                Window.Current.Activate();
            }
            else
            {
                var errMsg = I18N.Resources.GetString("ErrorNoFilesProvided/Text");
                throw new IOException(errMsg);
            }
        }

        private static async Task<bool> ConsistsOfArchivesOnly(IList<StorageFile> files)
        {
            if (files.IsNullOrEmpty()) return false;

            foreach (var file in files)
            {
                bool isArchive = await IsArchiveFile(file).ConfigureAwait(false);
                if (!isArchive) return false;
            }

            return true;
        }

        private static async Task<bool> IsArchiveFile(StorageFile file)
        {
            var type = Archives.ArchiveType.Unknown;

            try
            {
                type = await Archives.DetermineArchiveType(file).ConfigureAwait(false);
            }
            catch (InvalidArchiveTypeException)
            {
                Logger.Error("Archive type of {FileName} is unknown", file.Name);
                // type is already set to ArchiveType.Unknown
            }

            return type != Archives.ArchiveType.Unknown;
        }
    }
}
