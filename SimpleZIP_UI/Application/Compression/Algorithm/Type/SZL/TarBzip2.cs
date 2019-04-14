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

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type.SZL
{
    /// <inheritdoc />
    /// <summary>
    /// Implements <see cref="ICompressionAlgorithm"/> and offers
    /// compression and extraction of TAR+BZIP2 files. This class was
    /// introduced because it's more reliable than SharpCompress.
    /// </summary>
    internal class TarBzip2 : Tar
    {
        /// <inheritdoc />
        protected override Stream GetCompressorInputStream(Stream stream)
        {
            return new BZip2InputStream(stream);
        }

        /// <inheritdoc />
        protected override Stream GetCompressorOutputStream(Stream stream)
        {
            return new BZip2OutputStream(stream);
        }
    }
}
