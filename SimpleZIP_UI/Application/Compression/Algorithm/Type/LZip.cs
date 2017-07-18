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
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Compressors;
using SharpCompress.Compressors.LZMA;
using SimpleZIP_UI.Application.Compression.Model;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type
{
    internal class LZip : CompressorAlgorithm
    {
        protected override async Task<Stream> GetCompressorStream(StorageFile archive, CompressorOptions options)
        {
            return options.IsCompression
                ? new LZipStream(await archive.OpenStreamForWriteAsync(), CompressionMode.Compress)
                : new LZipStream(await archive.OpenStreamForReadAsync(), CompressionMode.Decompress);
        }
    }
}
