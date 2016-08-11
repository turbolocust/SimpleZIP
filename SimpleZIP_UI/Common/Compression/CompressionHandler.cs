using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleZIP_UI.Common.Compression.Algorithm;
using SimpleZIP_UI.Exceptions;
using SimpleZIP_UI.UI.Factory;

namespace SimpleZIP_UI.Common.Compression
{
    internal class CompressionHandler
    {
        private static CompressionHandler _instance;

        public static CompressionHandler Instance => _instance ?? (_instance = new CompressionHandler());

        private CompressionHandler()
        {
            // singleton
        }

        /// <summary>
        /// The algorithm that is used for compressing and decompressing operations.
        /// </summary>
        private ICompressionAlgorithm _compressionAlgorithm;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="archiveName"></param>
        /// <param name="location"></param>
        public async Task<int> CreateArchive(IReadOnlyList<StorageFile> files, string archiveName, string location)
        {
            var task = new Task<int>(() =>
            {
                var currentTime = DateTime.Now.Millisecond;
                if (files.Count > 0)
                {
                    _compressionAlgorithm.Compress(files, archiveName, location);
                }
                return DateTime.Now.Millisecond - currentTime;
            });
            task.Start();
            return await task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="archiveFile"></param>
        /// <exception cref="InvalidFileTypeException"></exception>
        public async Task<int> ExtractFromArchive(StorageFile archiveFile)
        {
            var currentTime = DateTime.Now.Millisecond;
            UI.Control.Algorithm key;

            // try to get enum type by file extension, which is the key
            if (!UI.Control.AlgorithmFileTypes.TryGetValue(archiveFile.FileType, out key))
            {
                try
                {
                    ChooseStrategy(key); // determines the algorithm to be used

                    if (_compressionAlgorithm != null)
                    {
                        // get the parent folder of the archive
                        var parent = await archiveFile.GetParentAsync();
                        if (parent != null)
                        {
                            try
                            {
                                // try to create the folder for extraction
                                var outputFolder =
                                    await
                                        parent.CreateFolderAsync(archiveFile.DisplayName,
                                            CreationCollisionOption.GenerateUniqueName);
                                // then extract archive content to newly created folder
                                _compressionAlgorithm.Extract(archiveFile.Path, outputFolder.Path);
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                MessageDialogFactory.CreateInformationDialog("Error",
                                    "Insufficient rights to extract archive here.\nPlease move archive to a different location.");
                                GoogleAnalytics.EasyTracker.GetTracker()
                                    .SendException(ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace, false);
                            }
                        }
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    GoogleAnalytics.EasyTracker.GetTracker()
                        .SendException(ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace, true);
                }
            }
            else
            {
                throw new InvalidFileTypeException("The selected file format is not supported.");
            }

            return DateTime.Now.Millisecond - currentTime;
        }

        /// <summary>
        /// Assigns the correct algorithm instance to be used by the archive's file extension.
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void ChooseStrategy(UI.Control.Algorithm key)
        {
            switch (key)
            {
                case UI.Control.Algorithm.Zip:
                    _compressionAlgorithm = Zipper.Instance;
                    break;

                case UI.Control.Algorithm.Gzip:
                    _compressionAlgorithm = GZipper.Instance;
                    break;

                case UI.Control.Algorithm.TarGz:
                    _compressionAlgorithm = Tarball.Instance;
                    break;

                case UI.Control.Algorithm.TarBz2:
                    _compressionAlgorithm = Tarball.Instance;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }
        }
    }
}
