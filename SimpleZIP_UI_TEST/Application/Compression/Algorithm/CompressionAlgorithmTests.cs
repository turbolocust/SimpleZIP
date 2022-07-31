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

using FluentAssertions;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleZIP_UI.Application.Compression;
using SimpleZIP_UI.Application.Compression.Algorithm;
using SimpleZIP_UI.Application.Compression.Algorithm.Factory;
using SimpleZIP_UI.Application.Compression.Algorithm.Options;
using SimpleZIP_UI.Application.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI_TEST.Application.Compression.Algorithm
{
    [TestClass]
    public sealed class CompressionAlgorithmTests
    {
        #region Static members

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

        #endregion

        private readonly StorageFolder _workingDir = ApplicationData.Current.TemporaryFolder;
        private IReadOnlyList<StorageFile> _files;

        [DataRow(Archives.ArchiveType.Zip, ".zip")]
        [DataRow(Archives.ArchiveType.Tar, ".tar")]
        [DataRow(Archives.ArchiveType.TarGz, ".tgz")]
        [DataRow(Archives.ArchiveType.TarBz2, ".tbz2")]
        [DataRow(Archives.ArchiveType.TarLz, ".tlz")]
        [DataTestMethod]
        public async Task CompressAsync_ShouldCreateProperArchive(Archives.ArchiveType archiveType, string fileNameExtension)
        {
            const int updateDelayRate = 100;

            var compressionOptions = new CompressionOptions(Encoding.UTF8);
            var algorithmOptions = new AlgorithmOptions(updateDelayRate);
            var algorithm = AlgorithmFactory.DetermineAlgorithm(archiveType, algorithmOptions);

            bool isSuccess = await PerformArchiveOperations(
                    algorithm, fileNameExtension, compressionOptions)
                .ConfigureAwait(false);

            isSuccess.Should().BeTrue();
        }

        /// <summary>
        /// Tests if the exception message generated by SharpZipLib
        /// equals the one indicating that no password is set.
        /// </summary>
        [TestMethod]
        public async Task ZipFile_GetInputStream_ShouldThrowExceptionWithSpecificMessage_WhenPasswordIsMissing()
        {
            const string archiveName = ArchiveName + "_enc.zip";
            // create test file to be archived first...
            var tempFile = await CreateTestFile("SimpleZIP_testFile_enc").ConfigureAwait(false);
            // ...then create test archive file
            var archive = await _workingDir.CreateFileAsync(
                archiveName, CreationCollisionOption.GenerateUniqueName);

            var zipStream = new ZipOutputStream(await archive
                .OpenStreamForWriteAsync().ConfigureAwait(false))
            {
                Password = "test"
            };

            zipStream.SetLevel(0); // no compression needed

            string entryName = ZipEntry.CleanName(tempFile.Name);
            var entry = new ZipEntry(entryName)
            {
                DateTime = DateTime.Now,
                Size = (long)await FileUtils
                    .GetFileSizeAsync(tempFile)
                    .ConfigureAwait(false)
            };

            zipStream.PutNextEntry(entry);
            var buffer = new byte[1 << 12];

            using (var srcStream = await tempFile
                .OpenStreamForReadAsync().ConfigureAwait(false))
            {
                StreamUtils.Copy(srcStream, zipStream, buffer);
            }

            zipStream.CloseEntry();
            zipStream.IsStreamOwner = true;
            zipStream.Close();

            using (var zipFile = new ZipFile(await archive
                .OpenStreamForReadAsync().ConfigureAwait(false)))
            {
                zipFile.Invoking(f => f.GetInputStream(0)).Should()
                    .Throw<Exception>().WithMessage("No password available*");
            }
        }

        #region Private methods

        private async Task<StorageFile> CreateTestFile(string name)
        {
            var tempFile = await _workingDir.CreateFileAsync(
                name, CreationCollisionOption.GenerateUniqueName);

            using (var streamWriter = new StreamWriter(await tempFile
                .OpenStreamForWriteAsync().ConfigureAwait(false)))
            {
                await streamWriter.WriteAsync(FileText).ConfigureAwait(false);
                await streamWriter.FlushAsync().ConfigureAwait(false);
            }

            return tempFile;
        }

        private async Task<bool> PerformArchiveOperations(
            ICompressionAlgorithm compressionAlgorithm,
            string fileType, ICompressionOptions options)
        {
            return await Task.Run(async () =>
            {
                var tempFile = await CreateTestFile("SimpleZIP_testFile").ConfigureAwait(false);

                _files = new[] { tempFile };
                string archiveName = ArchiveName + fileType;
                var archive = await _workingDir.CreateFileAsync(
                    archiveName, CreationCollisionOption.GenerateUniqueName);

                await compressionAlgorithm.CompressAsync(_files, archive, _workingDir, options).ConfigureAwait(false);

                // extract archive after creation
                return await ExtractArchive(compressionAlgorithm, archive.Name).ConfigureAwait(false);

            }).ConfigureAwait(false);
        }

        private async Task<bool> ExtractArchive(ICompressionAlgorithm compressionAlgorithm, string archiveName)
        {
            var archive = await _workingDir.GetFileAsync(archiveName);
            archive.Should().NotBeNull();

            var outputFolder = await _workingDir.CreateFolderAsync(
                "simpleZipUiTempOutput",
                CreationCollisionOption.OpenIfExists);

            // extract archive
            await compressionAlgorithm.DecompressAsync(archive, outputFolder).ConfigureAwait(false);

            var file = _files[0];
            using (var streamReader = new StreamReader(
                await file.OpenStreamForReadAsync().ConfigureAwait(false)))
            {
                string line = await streamReader.ReadLineAsync().ConfigureAwait(false);
                line.Should().NotBeNullOrEmpty();
                line.Should().Be(FileText);
            }

            // clean up once done
            await outputFolder.DeleteAsync();
            await file.DeleteAsync();

            return true;
        }

        #endregion
    }
}