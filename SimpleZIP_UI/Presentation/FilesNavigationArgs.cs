﻿// ==++==
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
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Storage;

namespace SimpleZIP_UI.Presentation
{
    internal class FilesNavigationArgs
    {
        public IReadOnlyList<StorageFile> StorageFiles { get; }

        public ShareOperation ShareOperation { get; }

        public bool IsArchivesOnly { get; }

        public FilesNavigationArgs(IReadOnlyList<StorageFile> files,
            ShareOperation shareOp = null, bool archivesOnly = false)
        {
            StorageFiles = files;
            ShareOperation = shareOp;
            IsArchivesOnly = archivesOnly;
        }
    }
}
