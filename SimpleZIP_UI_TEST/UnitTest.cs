using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using SimpleZIP_UI.Common.Compression.Algorithm;

namespace SimpleZIP_UI_TEST
{
    [TestClass]
    public class AlgorithmsTest
    {
        private IReadOnlyList<StorageFile> _files;

        private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

        private const string ArchiveName = "testArchive";

        private const string FileText = "This is a testfile, for testing compression and extraction.";

        [TestMethod]
        public void Tbz2Test()
        {
            Task.Run(async () =>
            {
                var compressionAlgorithm = Tar.Instance;

                var tempFile = await _localFolder.CreateFileAsync("tempFile");
                using (var fileStream = await tempFile.OpenAsync(FileAccessMode.ReadWrite))
                using (var streamWriter = new StreamWriter(fileStream.AsStreamForWrite()))
                {
                    streamWriter.AutoFlush = true;
                    streamWriter.WriteLine(FileText);
                }

                _files = await _localFolder.GetFilesAsync();
                Assert.IsNotNull(_files);
                Assert.AreEqual(_files.Count, 1); // check if file exists

                var archive = await _localFolder.CreateFileAsync(ArchiveName + ".tbz2");
                Assert.IsTrue(await compressionAlgorithm.Compress(_files, archive, _localFolder));

                ArchiveExtraction(".tbz2", compressionAlgorithm);
            }).GetAwaiter().GetResult();
        }

        public void ArchiveExtraction(string fileType, ICompressionAlgorithm compressionAlgorithm)
        {
            Task.Run(async () =>
            {
                var archive = await _localFolder.GetFileAsync(ArchiveName + fileType);
                Assert.IsNotNull(archive);

                var outputFolder = await _localFolder.CreateFolderAsync("Output");
                Assert.IsNotNull(outputFolder);

                // extract archive
                Assert.IsTrue(await compressionAlgorithm.Extract(archive, outputFolder));

                var files = await outputFolder.GetFilesAsync();
                Assert.IsNotNull(files);

                if (files.Count == 1)
                {
                    foreach (var file in files)
                    {
                        using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
                        using (var streamReader = new StreamReader(fileStream.AsStreamForRead()))
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

                await outputFolder.DeleteAsync();

            }).GetAwaiter().GetResult();
        }
    }
}
