﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SharpCompress.Common;
using SharpCompress.Writers;
using SimpleZIP_UI.Common.Compression.Algorithm;
using SimpleZIP_UI.Common.Compression.Algorithm.Type;
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
        /// The algorithm that is used for compressing and decompressing.
        /// </summary>
        private ICompressionAlgorithm _compressionAlgorithm;

        /// <summary>
        /// Optional writer options for compression.
        /// </summary>
        private WriterOptions _writerOptions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="archiveName"></param>
        /// <param name="location"></param>
        /// <param name="key"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Result> CreateArchive(IReadOnlyList<StorageFile> files, string archiveName,
            StorageFolder location, BaseControl.Algorithm key, CancellationToken ct)
        {
            _writerOptions = null; // reset writer options

            return await Task.Run(async () =>
            {
                var currentTime = DateTime.Now.Millisecond;
                var duration = 0;
                var message = "";

                if (files.Count > 0)
                {
                    try
                    {
                        var archive = await location.CreateFileAsync(archiveName,
                            CreationCollisionOption.GenerateUniqueName);
                        if (archive != null)
                        {
                            ChooseStrategy(key); // determines the algorithm to be used

                            if (await _compressionAlgorithm.Compress(files, archive, location, _writerOptions))
                            {
                                duration = DateTime.Now.Millisecond - currentTime;
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        message = ex.Message;
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        message = ex.Message;
                    }
                }

                return new Result
                {
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
        /// <exception cref="InvalidArchiveTypeException">If the file type of the selected file is not supported or unknown.</exception>
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
                    message = ex.Message;
                }
            }
            else
            {
                throw new InvalidArchiveTypeException("The selected file format is not supported.");
            }

            return await Task.Run(async () => // execute extraction asynchronously
            {
                var currentTime = DateTime.Now.Millisecond;
                var duration = -1;

                if (await _compressionAlgorithm.Extract(archiveFile, location))
                {
                    duration = DateTime.Now.Millisecond - currentTime;
                }

                return new Result
                {
                    StatusCode = message.Length > 0 ? (short)-1 : (short)0,
                    Message = message,
                    ElapsedTime = duration
                };

            }, ct);
        }

        /// <summary>
        /// Assigns the correct algorithm instance to be used by checking its key.
        /// </summary>
        /// <param name="key">The key of the algorithm.</param>
        /// <exception cref="ArgumentOutOfRangeException">May only be thrown on fatal error.</exception>
        private void ChooseStrategy(BaseControl.Algorithm key)
        {
            switch (key)
            {
                case BaseControl.Algorithm.Zip:
                    _compressionAlgorithm = Zip.Instance;
                    break;

                case BaseControl.Algorithm.SevenZip:
                    _compressionAlgorithm = SevenZip.Instance;
                    break;

                case BaseControl.Algorithm.GZip:
                    _compressionAlgorithm = GZip.Instance;
                    break;

                case BaseControl.Algorithm.TarGz:
                    _compressionAlgorithm = Tar.Instance;
                    _writerOptions = new WriterOptions(CompressionType.GZip);
                    break;

                case BaseControl.Algorithm.TarBz2:
                    _compressionAlgorithm = Tar.Instance;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }
        }
    }
}
