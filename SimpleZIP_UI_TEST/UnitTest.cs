﻿// ==++==
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

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SimpleZIP_UI.Application.Compression;
using SimpleZIP_UI.Application.Compression.Algorithm;
using SimpleZIP_UI.Application.Compression.Algorithm.Options;
using SimpleZIP_UI.Application.Util;
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
            try
            {
                var options = new CompressionOptions(false, Encoding.UTF8);
                var algorithm = Archives.DetermineAlgorithm(Archives.ArchiveType.Zip);
                Assert.IsTrue(await PerformArchiveOperations(algorithm, ".zip", options));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Assert.Fail();
            }
        }

        /// <summary>
        /// Tests the compression and extraction using TAR (uncompressed) archive type.
        /// </summary>
        [TestMethod]
        public async Task TarCompressionExtractionTest()
        {
            try
            {
                var options = new CompressionOptions(false, Encoding.UTF8);
                var algorithm = Archives.DetermineAlgorithm(Archives.ArchiveType.Tar);
                Assert.IsTrue(await PerformArchiveOperations(algorithm, ".tar", options));
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
            try
            {
                var options = new CompressionOptions(false, Encoding.UTF8);
                var algorithm = Archives.DetermineAlgorithm(Archives.ArchiveType.TarGz);
                Assert.IsTrue(await PerformArchiveOperations(algorithm, ".tgz", options));
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
            try
            {
                var options = new CompressionOptions(false, Encoding.UTF8);
                var algorithm = Archives.DetermineAlgorithm(Archives.ArchiveType.TarBz2);
                Assert.IsTrue(await PerformArchiveOperations(algorithm, ".tbz2", options));
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
            try
            {
                var options = new CompressionOptions(false, Encoding.UTF8);
                var algorithm = Archives.DetermineAlgorithm(Archives.ArchiveType.TarLz);
                Assert.IsTrue(await PerformArchiveOperations(algorithm, ".tlz", options));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Assert.Fail();
            }
        }

        /// <summary>
        /// Tests if the exception message generated by SharpZipLib
        /// equals the one indicating that no password is set.
        /// </summary>
        [TestMethod]
        public async Task EncryptedZipArchiveExceptionTest()
        {
            const string archiveName = ArchiveName + "_enc.zip";
            // create test file to be archived first...
            var tempFile = await CreateTestFile("SimpleZIP_testFile_enc");
            // ...then create test archive file
            var archive = await _workingDir.CreateFileAsync(
                archiveName, CreationCollisionOption.GenerateUniqueName);

            var zipStream = new ZipOutputStream(await archive.OpenStreamForWriteAsync())
            {
                Password = "test"
            };

            zipStream.SetLevel(0); // no compression needed

            string entryName = ZipEntry.CleanName(tempFile.Name);
            var entry = new ZipEntry(entryName)
            {
                DateTime = DateTime.Now,
                Size = (long)await FileUtils.GetFileSizeAsync(tempFile)
            };

            zipStream.PutNextEntry(entry);
            var buffer = new byte[4096];
            using (var srcStream = await tempFile.OpenStreamForReadAsync())
            {
                StreamUtils.Copy(srcStream, zipStream, buffer);
            }

            zipStream.CloseEntry();
            zipStream.IsStreamOwner = true;
            zipStream.Close();

            try
            {
                using (var zipFile = new ZipFile(await archive.OpenStreamForReadAsync()))
                {
                    zipFile.GetInputStream(0); // password required here
                    Assert.Fail("Correct password provided. " +
                                "This is not the purpose of this test.");
                }
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("No password available"));
            }
        }

        private async Task<StorageFile> CreateTestFile(string name)
        {
            var tempFile = await _workingDir.CreateFileAsync(
                name, CreationCollisionOption.GenerateUniqueName);

            using (var streamWriter = new StreamWriter(
                await tempFile.OpenStreamForWriteAsync()))
            {
                streamWriter.Write(FileText);
                streamWriter.Flush();
            }

            return tempFile;
        }

        private async Task<bool> PerformArchiveOperations(
            ICompressionAlgorithm compressionAlgorithm,
            string fileType, ICompressionOptions options)
        {
            return await Task.Run(async () =>
            {
                var tempFile = await CreateTestFile("SimpleZIP_testFile");

                _files = new[] { tempFile };
                string archiveName = ArchiveName + fileType;
                var archive = await _workingDir.CreateFileAsync(
                    archiveName, CreationCollisionOption.GenerateUniqueName);
                Assert.AreNotEqual(await compressionAlgorithm.Compress(
                    _files, archive, _workingDir, options), Stream.Null);

                // extract archive after creation
                return await ArchiveExtraction(compressionAlgorithm, archive.Name);
            });
        }

        private async Task<bool> ArchiveExtraction(
            ICompressionAlgorithm compressionAlgorithm, string archiveName)
        {
            var archive = await _workingDir.GetFileAsync(archiveName);
            Assert.IsNotNull(archive);

            var outputFolder = await _workingDir.CreateFolderAsync(
                "simpleZipUiTempOutput", CreationCollisionOption.OpenIfExists);

            // extract archive
            Assert.AreNotEqual(await compressionAlgorithm
                .Decompress(archive, outputFolder), Stream.Null);

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
