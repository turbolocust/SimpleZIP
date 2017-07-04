using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SharpCompress.Common;
using SharpCompress.Writers;
using SimpleZIP_UI.Application.Compression.Algorithm;
using SimpleZIP_UI.Application.Compression.Algorithm.Type;

namespace SimpleZIP_UI_TEST
{
    [TestClass]
    public class AlgorithmsTest
    {
        private IReadOnlyList<StorageFile> _files;

        private readonly StorageFolder _workingDir = ApplicationData.Current.TemporaryFolder;

        private const string ArchiveName = "simpleZipUiTestArchive";

        private const string FileText = "This is just a test for testing compression and extraction.";

        /// <summary>
        /// Tests the compression and extraction using ZIP archive type.
        /// </summary>
        [TestMethod]
        public async void ZipCompressionExtractionTest()
        {
            var options = new WriterOptions(CompressionType.Deflate);
            await PerformArchiveOperations(new Zip(), ".zip", options);
        }

        /// <summary>
        /// Tests the compression and extraction using TAR (gzip) archive type.
        /// </summary>
        [TestMethod]
        public async void TarGzipCompressionExtractionTest()
        {
            var options = new WriterOptions(CompressionType.GZip);
            await PerformArchiveOperations(new TarGz(), ".tgz", options);
        }

        /// <summary>
        /// Tests the compression and extraction using TAR (bzip2) archive type.
        /// </summary>
        [TestMethod]
        public async void TarBzip2CompressionExtractionTest()
        {
            var options = new WriterOptions(CompressionType.BZip2);
            await PerformArchiveOperations(new TarBzip2(), ".tbz2", options);
        }
        
        /// <summary>
        /// Tests the compression and extraction using TAR (lzip) archive type.
        /// </summary>
        [TestMethod]
        public async void TarLzipCompressionExtractionTest()
        {
            var options = new WriterOptions(CompressionType.BZip2);
            await PerformArchiveOperations(new TarLz(), ".tlz", options);
        }

        private async Task<bool> PerformArchiveOperations(IArchivingAlgorithm compressionAlgorithm, string fileType, WriterOptions options)
        {
            return await Task.Run(async () =>
            {
                var tempFile = await _workingDir.CreateFileAsync("tempFile");

                using (var streamWriter = new StreamWriter(await tempFile.OpenStreamForWriteAsync()))
                {
                    streamWriter.AutoFlush = true;
                    streamWriter.WriteLine(FileText);
                }

                _files = await _workingDir.GetFilesAsync();
                Assert.IsNotNull(_files);
                Assert.AreEqual(_files.Count, 1); // check if file exists

                var archive = await _workingDir.CreateFileAsync(ArchiveName + fileType);
                Assert.IsTrue(await compressionAlgorithm.Compress(_files, archive, _workingDir, options));

                return await ArchiveExtraction(compressionAlgorithm, fileType); // extract archive after creation
            });
        }

        private async Task<bool> ArchiveExtraction(IArchivingAlgorithm compressionAlgorithm, string fileType)
        {
            var archive = await _workingDir.GetFileAsync(ArchiveName + fileType);
            Assert.IsNotNull(archive);

            var outputFolder = await _workingDir.CreateFolderAsync("simpleZipUiTempOutput");
            Assert.IsNotNull(outputFolder);

            // extract archive
            Assert.IsTrue(await compressionAlgorithm.Extract(archive, outputFolder));

            var files = await outputFolder.GetFilesAsync();
            Assert.IsNotNull(files);

            if (files.Count == 1)
            {
                foreach (var file in files)
                {
                    using (var streamReader = new StreamReader(await file.OpenStreamForReadAsync()))
                    {
                        var line = streamReader.ReadLine();
                        Assert.IsNotNull(line);

                        if (!line.Equals(FileText))
                        {
                            Assert.Fail("Files do not match.");
                        }
                    }
                }
            }
            else
            {
                Assert.Fail("Archive not properly created.");
            }

            await outputFolder.DeleteAsync(); // clean up when done
            return true;
        }
    }
}
