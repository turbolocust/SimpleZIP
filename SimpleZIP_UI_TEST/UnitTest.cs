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

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SharpCompress.Common;
using SharpCompress.Writers;
using SimpleZIP_UI.Application.Compression.Algorithm;
using SimpleZIP_UI.Application.Compression.Algorithm.Type;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI_TEST
{
    [TestClass]
    public class AlgorithmsTest
    {
        private IReadOnlyList<StorageFile> _files;

        private readonly StorageFolder _workingDir = ApplicationData.Current.TemporaryFolder;

        private const string ArchiveName = "simpleZipUiTestArchive";

        private static readonly string FileText = GenerateRandomString(255);

        private static string GenerateRandomString(int length)
        {
            var sb = new StringBuilder(length);
            var rand = new Random();
            for (var i = 0; i < length; ++i)
            {
                sb.Append((char)rand.Next(65, 122));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Tests the compression and extraction using ZIP archive type.
        /// </summary>
        [TestMethod]
        public async Task ZipCompressionExtractionTest()
        {
            var options = new WriterOptions(CompressionType.Deflate) { LeaveStreamOpen = false };
            try
            {
                Assert.IsTrue(await PerformArchiveOperations(new Zip(), ".zip", options));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Assert.Fail();
            }
        }

        /// <summary>
        /// Tests the compression and extraction using TAR (gzip) archive type.
        /// </summary>
        [TestMethod]
        public async Task TarGzipCompressionExtractionTest()
        {
            var options = new WriterOptions(CompressionType.GZip) { LeaveStreamOpen = false };
            try
            {
                Assert.IsTrue(await PerformArchiveOperations(new TarGzip(), ".tgz", options));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Assert.Fail();
            }
        }

        /// <summary>
        /// Tests the compression and extraction using TAR (bzip2) archive type.
        /// </summary>
        [TestMethod]
        public async Task TarBzip2CompressionExtractionTest()
        {
            var options = new WriterOptions(CompressionType.BZip2) { LeaveStreamOpen = false };
            try
            {
                Assert.IsTrue(await PerformArchiveOperations(new TarBzip2(), ".tbz2", options));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Assert.Fail();
            }
        }

        /// <summary>
        /// Tests the compression and extraction using TAR (lzip) archive type.
        /// </summary>
        [TestMethod]
        public async Task TarLzipCompressionExtractionTest()
        {
            var options = new WriterOptions(CompressionType.LZip) { LeaveStreamOpen = false };
            try
            {
                Assert.IsTrue(await PerformArchiveOperations(new TarLzip(), ".tlz", options));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Assert.Fail();
            }
        }

        private async Task<bool> PerformArchiveOperations(
            ICompressionAlgorithm compressionAlgorithm, string fileType, WriterOptions options)
        {
            return await Task.Run(async () =>
            {
                var tempFile = await _workingDir.CreateFileAsync(
                    "SimpleZIP_testFile", CreationCollisionOption.GenerateUniqueName);
                using (var streamWriter = new StreamWriter(await tempFile.OpenStreamForWriteAsync()))
                {
                    streamWriter.Write(FileText);
                    streamWriter.Flush();
                }

                _files = new[] { tempFile };
                string archiveName = ArchiveName + fileType;
                var archive = await _workingDir.CreateFileAsync(
                    archiveName, CreationCollisionOption.GenerateUniqueName);
                Assert.AreNotEqual(await compressionAlgorithm.Compress(_files, archive, _workingDir, options), Stream.Null);

                // extract archive after creation
                return await ArchiveExtraction(compressionAlgorithm, archive.Name);
            });
        }

        private async Task<bool> ArchiveExtraction(ICompressionAlgorithm compressionAlgorithm, string archiveName)
        {
            var archive = await _workingDir.GetFileAsync(archiveName);
            Assert.IsNotNull(archive);

            var outputFolder = await _workingDir.CreateFolderAsync(
                "simpleZipUiTempOutput", CreationCollisionOption.OpenIfExists);

            // extract archive
            Assert.AreNotEqual(await compressionAlgorithm.Decompress(archive, outputFolder), Stream.Null);

            var file = _files[0];
            using (var streamReader = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                var line = streamReader.ReadLine();
                if (line != null && !line.Equals(FileText))
                {
                    Assert.Fail("Files do not match.");
                }
            }

            // clean up when done
            await outputFolder.DeleteAsync();
            await file.DeleteAsync();
            return true;
        }
    }
}
