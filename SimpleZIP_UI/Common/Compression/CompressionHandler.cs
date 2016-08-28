using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleZIP_UI.Common.Compression.Algorithm;
using SimpleZIP_UI.Exceptions;
using SimpleZIP_UI.UI;
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
        /// <param name="archive"></param>
        /// <param name="key"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<int> CreateArchive(IReadOnlyList<StorageFile> files, FileInfo archive, Control.Algorithm key, CancellationToken ct)
        {
            var task = new Task<int>(() =>
            {
                var currentTime = DateTime.Now.Millisecond;
                var totalDuration = 0;

                if (files.Count > 0)
                {
                    try
                    {
                        ChooseStrategy(key); // determines the algorithm to be used
                        _compressionAlgorithm.Compress(files, archive.FullName, archive.DirectoryName);
                        totalDuration = DateTime.Now.Millisecond - currentTime;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        totalDuration = -2;
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        GoogleAnalytics.EasyTracker.GetTracker()
                            .SendException(ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace, true);
                    }
                }

                return totalDuration;

            }, ct);

            task.Start();
            return await task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="archiveFile"></param>
        /// <exception cref="InvalidFileTypeException">If the file type of the selected file is not supported or unknown.</exception>
        /// <exception cref="UnauthorizedAccessException">If extraction at the archive's path is not allowed.</exception>
        public async Task<int> ExtractFromArchive(StorageFile archiveFile, CancellationToken ct)
        {
            Control.Algorithm key; // the file type of the archive
            StorageFolder outputFolder = null;

            // try to get enum type by file extension, which is the key
            if (Control.AlgorithmFileTypes.TryGetValue(archiveFile.FileType, out key))
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
                            // try to create the folder for extraction
                            outputFolder = await parent.CreateFolderAsync(archiveFile.DisplayName,
                                        CreationCollisionOption.GenerateUniqueName);
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

            var task = new Task<int>(() => // execute extraction in task
            {
                var currentTime = DateTime.Now.Millisecond;
                var totalDuration = 0;

                if (outputFolder != null)
                {
                    _compressionAlgorithm.Extract(archiveFile.Path, outputFolder.Path);
                    totalDuration = DateTime.Now.Millisecond - currentTime;
                }

                return totalDuration;
            }, ct);

            task.Start();
            return await task;
        }

        /// <summary>
        /// Assigns the correct algorithm instance to be used by the archive's file extension.
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="ArgumentOutOfRangeException">May only be thrown on fatal error.</exception>
        private void ChooseStrategy(Control.Algorithm key)
        {
            switch (key)
            {
                case Control.Algorithm.Zip:
                    _compressionAlgorithm = Zipper.Instance;
                    break;

                case Control.Algorithm.Gzip:
                    _compressionAlgorithm = GZipper.Instance;
                    break;

                case Control.Algorithm.TarGz:
                    _compressionAlgorithm = Tarball.Instance;
                    break;

                case Control.Algorithm.TarBz2:
                    _compressionAlgorithm = Tarball.Instance;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }
        }
    }
}
