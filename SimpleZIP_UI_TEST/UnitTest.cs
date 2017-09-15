using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            Assert.IsTrue(await PerformArchiveOperations(new Zip(), ".zip", options));
        }

        /// <summary>
        /// Tests the compression and extraction using TAR (gzip) archive type.
        /// </summary>
        [TestMethod]
        public async Task TarGzipCompressionExtractionTest()
        {
            var options = new WriterOptions(CompressionType.GZip) { LeaveStreamOpen = false };
            Assert.IsTrue(await PerformArchiveOperations(new TarGzip(), ".tgz", options));
        }

        /// <summary>
        /// Tests the compression and extraction using TAR (bzip2) archive type.
        /// </summary>
        [TestMethod]
        public async Task TarBzip2CompressionExtractionTest()
        {
            var options = new WriterOptions(CompressionType.BZip2) { LeaveStreamOpen = false };
            Assert.IsTrue(await PerformArchiveOperations(new TarBzip2(), ".tbz2", options));
        }

        /// <summary>
        /// Tests the compression and extraction using TAR (lzip) archive type.
        /// </summary>
        [TestMethod]
        public async Task TarLzipCompressionExtractionTest()
        {
            var options = new WriterOptions(CompressionType.LZip) { LeaveStreamOpen = false };
            Assert.IsTrue(await PerformArchiveOperations(new TarLzip(), ".tlz", options));
        }

        private async Task<bool> PerformArchiveOperations(ICompressionAlgorithm compressionAlgorithm, string fileType, WriterOptions options)
        {
            return await Task.Run(async () =>
            {
                var tempFile = await _workingDir.CreateFileAsync("tempFile");
                Assert.IsNotNull(tempFile);

                using (var streamWriter = new StreamWriter(await tempFile.OpenStreamForWriteAsync()))
                {
                    streamWriter.AutoFlush = true;
                    streamWriter.WriteLine(FileText);
                }

                _files = new[] { tempFile };
                var archive = await _workingDir.CreateFileAsync(ArchiveName + fileType);
                Assert.AreNotEqual(await compressionAlgorithm.Compress(_files, archive, _workingDir, options), Stream.Null);

                return await ArchiveExtraction(compressionAlgorithm, fileType); // extract archive after creation
            });
        }

        private async Task<bool> ArchiveExtraction(ICompressionAlgorithm compressionAlgorithm, string fileType)
        {
            var archive = await _workingDir.GetFileAsync(ArchiveName + fileType);
            Assert.IsNotNull(archive);

            var outputFolder = await _workingDir.CreateFolderAsync("simpleZipUiTempOutput");
            Assert.IsNotNull(outputFolder);

            // extract archive
            Assert.AreNotEqual(await compressionAlgorithm.Decompress(archive, outputFolder), Stream.Null);

            var file = _files[0];
            using (var streamReader = new StreamReader(await file.OpenStreamForReadAsync()))
            {
                var line = streamReader.ReadLine();
                if (!line.Equals(FileText))
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
