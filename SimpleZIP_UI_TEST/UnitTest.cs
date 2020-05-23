﻿// ==++==
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

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleZIP_UI.Application.Compression;
using SimpleZIP_UI.Application.Compression.Algorithm;
using SimpleZIP_UI.Application.Compression.Algorithm.Options;
using SimpleZIP_UI.Application.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleZIP_UI.Application.Hashing;

namespace SimpleZIP_UI_TEST
{
    [TestClass]
    public class AlgorithmsTest
    {
        private IReadOnlyList<StorageFile> _files;

        private readonly StorageFolder _workingDir = ApplicationData.Current.TemporaryFolder;

        private const string ArchiveName = "simpleZipUiTestArchive";

        private static readonly string FileText = GenerateRandomString(256);

        private static string GenerateRandomString(int length)
        {
            var sb = new StringBuilder(length);
            var rand = new Random();

            for (int i = 0; i < length; ++i)
            {
                sb.Append((char)rand.Next(65, 122));
            }

            return sb.ToString();
        }

        [DataRow(Archives.ArchiveType.Zip, ".zip")]
        [DataRow(Archives.ArchiveType.Tar, ".tar")]
        [DataRow(Archives.ArchiveType.TarGz, ".tgz")]
        [DataRow(Archives.ArchiveType.TarBz2, ".tbz2")]
        [DataRow(Archives.ArchiveType.TarLz, ".tlz")]
        [DataTestMethod]
        public async Task CompressionExtractionTest(Archives.ArchiveType archiveType, string fileNameExtension)
        {
            try
            {
                var options = new CompressionOptions(Encoding.UTF8);
                var algorithm = Archives.DetermineAlgorithm(archiveType);
                Assert.IsTrue(await PerformArchiveOperations(algorithm, fileNameExtension, options));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
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
            var buffer = new byte[1 << 12];
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

        [TestMethod]
        public async Task MessageDigestAlgorithmsTest()
        {
            // create an array of values to be tested (hashed and compared)
            var values = new[] { @"34t5rj3490f80e9fj2", @"lk9t99fk3k4nmrkfsö" };
            // create a dictionary which maps each index of a value in the
            // array, together with the algorithm name, to its hashed value
            var nameIndexPairToHashedValue = new Dictionary<NameIndexPair, string>
            {
                [new NameIndexPair(0, "MD5")] = @"7270AAAD8E027D6931C90C8F79C0E1C3",
                [new NameIndexPair(0, "SHA1")] = @"478D07EB9B7D2CAE1D709246B362353655D7A724",
                [new NameIndexPair(0, "SHA256")] = @"20C5A29457A08511D61049D91FB9890E33F0DF1E8C8F1D1F88A74C1B93319EF4",
                [new NameIndexPair(0, "SHA384")] = @"F441A649992566C51B516E21635ADC2DE8A1F74A8D425EE595B99EB66B4ACB7347E4BE09B2DC0D6214212D35F32B2481",
                [new NameIndexPair(0, "SHA512")] = @"1D665267A2DBE5F36B4B6887B08FCF840956B8FF9BFE389D15D0E368456210629F809A3E03EB8BE7F8494C6F1DA07FD890C8C45C7DE475251C0E65F76F680446",
                [new NameIndexPair(1, "MD5")] = @"3E8C98C0FD4889D0542E2304ED700A16",
                [new NameIndexPair(1, "SHA1")] = @"1A16311414871804BBBA12AACEE462E2BC4D5B29",
                [new NameIndexPair(1, "SHA256")] = @"6EE331FADB2765834D8FDDE01EBAC1FE150C4D8F4A308BE7912A68848366C07A",
                [new NameIndexPair(1, "SHA384")] = @"C8A70F67CC08D0F16FA8BC59D6D9FB3192B1F7A55F8BAD508C265D7B964EF330BDEC9DF034DC3D3318C7947F858E46B0",
                [new NameIndexPair(1, "SHA512")] = @"1BCF04308F524F8C028F3667D168F7979C264C922D4136CB693435ED419B3087D0A421F20E99E5C308ACCF529E4EB51C88786246D0DA41F1796215F5AE4FDEB4"
            };

            IMessageDigestProvider messageDigestProvider = new MessageDigestProvider();
            var supportedAlgorithms = messageDigestProvider.SupportedAlgorithms;

            foreach (var (nameIndexPair, expectedHashedValue) in nameIndexPairToHashedValue)
            {
                var algorithmName = nameIndexPair.Name;
                string value = values[nameIndexPair.Index];
                // check if algorithm is actually supported (exists)
                Assert.IsTrue(supportedAlgorithms.Contains(algorithmName));
                (_, string hashedValue) = await messageDigestProvider.ComputeAsync(value, algorithmName);
                // check if computed hash value equals expected hash value
                Assert.AreEqual(expectedHashedValue, hashedValue);
            }
        }

        private async Task<StorageFile> CreateTestFile(string name)
        {
            var tempFile = await _workingDir.CreateFileAsync(
                name, CreationCollisionOption.GenerateUniqueName);

            using (var streamWriter = new StreamWriter(await tempFile.OpenStreamForWriteAsync()))
            {
                await streamWriter.WriteAsync(FileText);
                await streamWriter.FlushAsync();
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

                await compressionAlgorithm.CompressAsync(_files, archive, _workingDir, options);

                // extract archive after creation
                return await ExtractArchive(compressionAlgorithm, archive.Name);
            });
        }

        private async Task<bool> ExtractArchive(ICompressionAlgorithm compressionAlgorithm, string archiveName)
        {
            var archive = await _workingDir.GetFileAsync(archiveName);
            Assert.IsNotNull(archive);

            var outputFolder = await _workingDir.CreateFolderAsync(
                "simpleZipUiTempOutput", CreationCollisionOption.OpenIfExists);

            // extract archive
            await compressionAlgorithm.DecompressAsync(archive, outputFolder);

            var file = _files[0];
            using (var streamReader = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                string line = await streamReader.ReadLineAsync();
                if (line != null && !line.Equals(FileText))
                {
                    Assert.Fail("Files do not match.");
                }
            }

            // clean up once done
            await outputFolder.DeleteAsync();
            await file.DeleteAsync();
            return true;
        }
    }
}
