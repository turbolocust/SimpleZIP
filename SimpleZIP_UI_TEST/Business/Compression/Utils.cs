// ==++==
// 
// Copyright (C) 2022 Matthias Fussenegger
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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI_TEST.Business.Compression
{
    internal static class Utils
    {
        internal static string GenerateRandomString(int length)
        {
            var sb = new StringBuilder(length);
            var rand = new Random();

            for (int i = 0; i < length; ++i)
            {
                sb.Append((char)rand.Next(65, 122));
            }

            return sb.ToString();
        }

        internal static async Task<(string fileContent, StorageFile file)>
            CreateTestFile(StorageFolder directory, string name)
        {
            string randomString = GenerateRandomString(256);
            var tempFile = await directory.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);

            using (var streamWriter = new StreamWriter(await tempFile
                .OpenStreamForWriteAsync().ConfigureAwait(false)))
            {
                await streamWriter.WriteAsync(randomString).ConfigureAwait(false);
                await streamWriter.FlushAsync().ConfigureAwait(false);
            }

            return (randomString, tempFile);
        }
    }
}
