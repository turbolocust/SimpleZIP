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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleZIP_UI.Business.Compression;
using SimpleZIP_UI.Business.Compression.Algorithm;
using SimpleZIP_UI.Business.Compression.Algorithm.Factory;
using SimpleZIP_UI.Business.Compression.Algorithm.Options;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;

namespace SimpleZIP_UI_TEST.Business.Compression.Algorithm
{
    [TestClass]
    public sealed class CompressorAlgorithmTests
    {
        private const string ArchiveName = "simpleZipUiTestArchive";
        private readonly StorageFolder _workingDir = ApplicationData.Current.TemporaryFolder;

        [DataRow(Archives.ArchiveType.GZip, ".gz")]
        [DataRow(Archives.ArchiveType.BZip2, ".bz2")]
        [DataRow(Archives.ArchiveType.LZip, ".lz")]
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

        #region Private methods

        private async Task<bool> PerformArchiveOperations(
            ICompressionAlgorithm compressionAlgorithm,
            string fileType, ICompressionOptions options)
        {
            return await Task.Run(async () =>
            {
                var (content, tempFile) = await Utils.CreateTestFile(_workingDir, "SimpleZIP_testFile").ConfigureAwait(false);

                var files = new[] { tempFile };
                string archiveName = ArchiveName + fileType;

                var archive = await _workingDir.CreateFileAsync(archiveName, CreationCollisionOption.GenerateUniqueName);
                await compressionAlgorithm.CompressAsync(files, archive, _workingDir, options).ConfigureAwait(false);

                bool success = await ExtractArchiveAndAssert(compressionAlgorithm, archive.Name, content).ConfigureAwait(false);

                await tempFile.DeleteAsync();

                return success;

            }).ConfigureAwait(false);
        }

        private async Task<bool> ExtractArchiveAndAssert(
            ICompressionAlgorithm compressionAlgorithm,
            string archiveName, string expectedContent)
        {
            var archive = await _workingDir.GetFileAsync(archiveName);
            archive.Should().NotBeNull();

            var outputFolder = await _workingDir.CreateFolderAsync(
                "simpleZipUiTempOutput", CreationCollisionOption.ReplaceExisting);

            await compressionAlgorithm.DecompressAsync(archive, outputFolder).ConfigureAwait(false);
            var extractedFiles = await outputFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.DefaultQuery);
            var extractedFile = extractedFiles.First();

            using (var streamReader = new StreamReader(await extractedFile.OpenStreamForReadAsync().ConfigureAwait(false)))
            {
                var actualContent = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                actualContent.Should().NotBeEmpty();
                actualContent.Should().Be(expectedContent);
            }

            await outputFolder.DeleteAsync();

            return true;
        }

        #endregion
    }
}
