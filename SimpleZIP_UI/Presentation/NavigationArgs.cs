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
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression.Model;

namespace SimpleZIP_UI.Presentation
{
    internal sealed class NavigationArgs
    {
        public IReadOnlyList<StorageFile> StorageFiles { get; }

        public IReadOnlyList<ExtractableItem> ExtractableItems { get; private set; }

        public ShareOperation ShareOperation { get; }

        /// <inheritdoc />
        public NavigationArgs(IReadOnlyList<StorageFile> files, ShareOperation shareOp = null)
        {
            StorageFiles = files;
            ShareOperation = shareOp;
        }

        /// <summary>
        /// Converts each file in the specified list to <see cref="ExtractableItem"/>.
        /// This will then set <see cref="ExtractableItems"/> of <see cref="NavigationArgs"/>.
        /// </summary>
        /// <param name="files">The files to be converted.</param>
        /// <param name="shareOp">Optional share operation.</param>
        /// <returns>A new instance of <see cref="NavigationArgs"/>.</returns>
        internal static NavigationArgs ForDecompressionSummaryPage(
            IReadOnlyList<StorageFile> files, ShareOperation shareOp = null)
        {
            // convert to ExtractableItem objects
            var items = new List<ExtractableItem>(files.Count);
            items.AddRange(files.Select(file => new ExtractableItem(file.Name, file)));

            return new NavigationArgs(files, shareOp)
            {
                ExtractableItems = items
            };
        }
    }
}
