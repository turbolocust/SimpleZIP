using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using GoogleAnalytics;
using SimpleZIP_UI.Common.Compression.Algorithm;
using SimpleZIP_UI.Common.Model;
using SimpleZIP_UI.Exceptions;
using SimpleZIP_UI.UI;

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
        /// <param name="key"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Result> CreateArchive(IReadOnlyList<StorageFile> files, string archiveName, StorageFolder location, BaseControl.Algorithm key, CancellationToken ct)
        {
            return await Task.Run(async () =>
            {
                var currentTime = DateTime.Now.Millisecond;
                var duration = 0;
                var message = "";

                if (files.Count > 0)
                {
                    try
                    {
                        ChooseStrategy(key); // determines the algorithm to be used
                        if (await _compressionAlgorithm.Compress(files, archiveName, location))
                        {
                            duration = DateTime.Now.Millisecond - currentTime;
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        message = ex.Message;
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        EasyTracker.GetTracker()
                            .SendException(ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace, true);
                        message = ex.Message;
                    }
                }

                return new Result {
                    StatusCode = message.Length > 0 ? (short)-1 : (short)0,
                    Message = message,
                    ElapsedTime = duration
                };

            }, ct);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="archiveFile"></param>
        /// <param name="location"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="InvalidFileTypeException">If the file type of the selected file is not supported or unknown.</exception>
        /// <exception cref="UnauthorizedAccessException">If extraction at the archive's path is not allowed.</exception>
        public async Task<Result> ExtractFromArchive(StorageFile archiveFile, StorageFolder location, CancellationToken ct)
        {
            BaseControl.Algorithm key; // the file type of the archive
            var message = "";

            // try to get enum type by file extension, which is the key
            if (BaseControl.AlgorithmFileTypes.TryGetValue(archiveFile.FileType, out key))
            {
                try
                {
                    ChooseStrategy(key); // determines the algorithm to be used
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    EasyTracker.GetTracker()
                        .SendException(ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace, true);
                    message = ex.Message;
                }
            }
            else
            {
                throw new InvalidFileTypeException("The selected file format is not supported.");
            }

            return await Task.Run(async () => // execute extraction asynchronously
            {
                var currentTime = DateTime.Now.Millisecond;
                var duration = -1;

                if (await _compressionAlgorithm.Extract(archiveFile, location))
                {
                    duration = DateTime.Now.Millisecond - currentTime;
                }

                return new Result {
                    StatusCode = message.Length > 0 ? (short)-1 : (short)0,
                    Message = message,
                    ElapsedTime = duration
                };

            }, ct);
        }

        /// <summary>
        /// Assigns the correct algorithm instance to be used by the archive's file extension.
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="ArgumentOutOfRangeException">May only be thrown on fatal error.</exception>
        private void ChooseStrategy(BaseControl.Algorithm key)
        {
            switch (key)
            {
                case BaseControl.Algorithm.Zip:
                    _compressionAlgorithm = Zipper.Instance;
                    break;

                case BaseControl.Algorithm.Gzip:
                    _compressionAlgorithm = GZipper.Instance;
                    break;

                case BaseControl.Algorithm.TarGz:
                    _compressionAlgorithm = Tarball.Instance;
                    break;

                case BaseControl.Algorithm.TarBz2:
                    _compressionAlgorithm = Tarball.Instance;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }
        }
    }
}
