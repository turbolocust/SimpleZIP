using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SimpleZIP_UI.Common.Compression.Algorithm;
using SimpleZIP_UI.Common.Compression.Algorithm.Type;

namespace SimpleZIP_UI_TEST
{
    [TestClass]
    public class AlgorithmsTest
    {
        private IReadOnlyList<StorageFile> _files;

        private readonly StorageFolder _workingDir = ApplicationData.Current.LocalFolder;

        private const string ArchiveName = "testArchive";

        private const string FileText = "This is a testfile, for testing compression and extraction.";

        /// <summary>
        /// Tests the compression and extraction of each algorithm.
        /// </summary>
        [TestMethod]
        public void CompressionExtractionTest()
        {
            ArchiveCompression(Zip.Instance, ".zip");
            ArchiveCompression(Tar.Instance, ".tbz2");
        }

        private void ArchiveCompression(ICompressionAlgorithm compressionAlgorithm, string fileType)
        {
            Task.Run(async () =>
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
                Assert.IsTrue(await compressionAlgorithm.Compress(_files, archive, _workingDir));

                ArchiveExtraction(compressionAlgorithm, fileType); // extract archive after creation

            }).GetAwaiter().GetResult();
        }

        private void ArchiveExtraction(ICompressionAlgorithm compressionAlgorithm, string fileType)
        {
            Task.Run(async () =>
            {
                var archive = await _workingDir.GetFileAsync(ArchiveName + fileType);
                Assert.IsNotNull(archive);

                var outputFolder = await _workingDir.CreateFolderAsync("Output");
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

                // clean up when done
                await outputFolder.DeleteAsync();
                await PurgeWorkingDir();

            }).GetAwaiter().GetResult();
        }

        private async Task<bool> PurgeWorkingDir()
        {
            var files = await _workingDir.GetFilesAsync();

            foreach (var file in files)
            {
                await file.DeleteAsync();
            }

            return true;
        }
    }
}
