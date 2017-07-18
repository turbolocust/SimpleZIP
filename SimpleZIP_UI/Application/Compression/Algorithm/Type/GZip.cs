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
using SharpCompress.Compressors.Deflate;
using SimpleZIP_UI.Application.Compression.Model;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Type
{
    public class GZip : CompressorAlgorithm
    {
        protected override async Task<Stream> GetCompressorStream(StorageFile archive, CompressorOptions options)
        {
            var stream = options.IsCompression
                ? new GZipStream(await archive.OpenStreamForWriteAsync(),
                        CompressionMode.Compress, CompressionLevel.BestSpeed)
                : new GZipStream(await archive.OpenStreamForReadAsync(), CompressionMode.Decompress);

            // set file name to stream
            var fileName = options.FileName;
            if (!string.IsNullOrEmpty(fileName))
            {
                stream.FileName = fileName;
            }

            return stream;
        }
    }
}
