// ==++==
// 
// Copyright (C) 2019 Matthias Fussenegger
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

using ICSharpCode.SharpZipLib.BZip2;
using System.IO;
using System.Threading;
using Windows.Storage;

namespace SimpleZIP_UI.Business.Compression.Reader.SZL
{
    internal sealed class ArchiveReaderTarBz2 : ArchiveReaderTar
    {
        /// <inheritdoc />
        public ArchiveReaderTarBz2(IStorageFile archive,
            CancellationToken cancellationToken) : base(archive, cancellationToken)
        {
        }

        /// <inheritdoc />
        protected override Stream GetCompressorStream(Stream stream)
        {
            return new BZip2InputStream(stream)
            {
                IsStreamOwner = true
            };
        }
    }
}
