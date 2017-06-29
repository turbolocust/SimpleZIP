using System;
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
using SimpleZIP_UI.Common.Util;
using SimpleZIP_UI.Exceptions;
using SimpleZIP_UI.UI;

namespace SimpleZIP_UI.Common.Compression
{
    internal class CompressionFacade
    {
        /// <summary>
        /// Stores the current time when initializing a new operation.
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// Stores the result of the operation.
        /// </summary>
        private bool _isSuccess;

        /// <summary>
        /// The algorithm that is used for compression or decompression.
        /// </summary>
        private IArchivingAlgorithm _compressionAlgorithm;

        /// <summary>
        /// Optional writer options for compression.
        /// </summary>
        private WriterOptions _writerOptions;

        /// <summary>
        /// Creates a new archive with a compressed version of the specified file.
        /// </summary>
        /// <param name="file">The file to be compressed.</param>
        /// <param name="archiveName">The name of the archive to be created.</param>
        /// <param name="location">Where to store the archive.</param>
        /// <param name="key">The key of the algorithm to be used.</param>
        /// <param name="ct">The token that is used to cancel the operation.</param>
        /// <returns>An object that consists of result parameters.</returns>
        public async Task<Result> CreateArchive(StorageFile file, string archiveName,
            StorageFolder location, BaseControl.Algorithm key, CancellationToken ct)
        {
            InitOperation(key);

            return await Task.Run(async () => // execute compression asynchronously
            {
                var message = "";

                try
                {
                    var archive = await location.CreateFileAsync(archiveName,
                        CreationCollisionOption.GenerateUniqueName);

                    _isSuccess = await _compressionAlgorithm.Compress(file, archive, location, _writerOptions);

                    if (ct.IsCancellationRequested)
                    {
                        FileUtils.Cleanup(archive);
                    }
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                }

                return EvaluateResult(message, ct);

            }, ct);
        }

        /// <summary>
        /// Creates a new archive with compressed versions of the specified files.
        /// </summary>
        /// <param name="files">The files to be compressed.</param>
        /// <param name="archiveName">The name of the archive to be created.</param>
        /// <param name="location">Where to store the archive.</param>
        /// <param name="key">The key of the algorithm to be used.</param>
        /// <param name="ct">The token that is used to cancel the operation.</param>
        /// <returns>An object that consists of result parameters.</returns>
        public async Task<Result> CreateArchive(IReadOnlyList<StorageFile> files, string archiveName,
            StorageFolder location, BaseControl.Algorithm key, CancellationToken ct)
        {
            InitOperation(key);

            return await Task.Run(async () => // execute compression asynchronously
            {
                var message = "";

                if (files.Count > 0)
                {
                    try
                    {
                        var archive = await location.CreateFileAsync(archiveName,
                            CreationCollisionOption.GenerateUniqueName);

                        _isSuccess = await _compressionAlgorithm.Compress(files, archive, location, _writerOptions);

                        if (ct.IsCancellationRequested)
                        {
                            FileUtils.Cleanup(archive);
                        }
                    }
                    catch (Exception ex)
                    {
                        message = ex.Message;
                    }
                }

                return EvaluateResult(message, ct);

            }, ct);
        }

        /// <summary>
        /// Extracts files from an archive to the specified location.
        /// </summary>
        /// <param name="archiveFile">The archive to be extracted.</param>
        /// <param name="location">The location where to extract the archive's content.</param>
        /// <param name="ct">The token that is used to cancel the operation.</param>
        /// <returns>An object that consists of result parameters.</returns>
        /// <exception cref="InvalidArchiveTypeException">If the file type of the selected 
        /// file is not supported or unknown.</exception>
        /// <exception cref="UnauthorizedAccessException">If extraction at the archive's 
        /// location is not allowed.</exception>
        public async Task<Result> ExtractFromArchive(StorageFile archiveFile, StorageFolder location, CancellationToken ct)
        {
            BaseControl.Algorithm key; // the file type of the archive
            var message = "";

            // try to get enum type by file extension, which is the key
            if (BaseControl.AlgorithmFileTypes.TryGetValue(archiveFile.FileType, out key))
            {
                InitOperation(key);
            }
            else
            {
                throw new InvalidArchiveTypeException("The selected file format is not supported.");
            }

            return await Task.Run(async () => // execute extraction asynchronously
            {
                try
                {
                    _isSuccess = await _compressionAlgorithm.Extract(archiveFile, location);
                }
                catch (IOException ex)
                {
                    message = ex.Message;
                }

                return EvaluateResult(message, ct);

            }, ct);
        }

        /// <summary>
        /// Initializes the compression or decompression operation
        /// and chooses the strategy by using the specified key.
        /// </summary>
        /// <param name="key">The key of the algorithm.</param>
        private void InitOperation(BaseControl.Algorithm key)
        {
            _writerOptions = null;
            _startTime = DateTime.Now;
            ChooseStrategy(key); // determines the right algorithm
        }

        /// <summary>
        /// Evaluates the operation and returns the result.
        /// </summary>
        /// <param name="message">The message to be evaluated.</param>
        /// <param name="ct">The token to be considered on evaluation.</param>
        /// <returns>An object that consists of result parameters.</returns>
        private Result EvaluateResult(string message, CancellationToken ct)
        {
            Result.Status status;
            TimeSpan duration;

            if (ct.IsCancellationRequested)
            {
                status = Result.Status.Interrupt;
            }
            else
            {
                status = _isSuccess ? Result.Status.Success : Result.Status.Fail;
                duration = DateTime.Now - _startTime;
            }

            return new Result
            {
                StatusCode = status,
                Message = message,
                ElapsedTime = duration
            };
        }

        /// <summary>
        /// Assigns the correct algorithm instance to be used by evaluating its key.
        /// </summary>
        /// <param name="key">The key of the algorithm.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when key matched no algorithm.</exception>
        private void ChooseStrategy(BaseControl.Algorithm key)
        {
            switch (key)
            {
                case BaseControl.Algorithm.Zip:
                    _compressionAlgorithm = Zip.Instance;
                    break;
                case BaseControl.Algorithm.GZip:
                    _compressionAlgorithm = GZip.Instance;
                    break;
                case BaseControl.Algorithm.Rar:
                    _compressionAlgorithm = Rar.Instance;
                    break;
                case BaseControl.Algorithm.SevenZip:
                    _compressionAlgorithm = SevenZip.Instance;
                    break;
                case BaseControl.Algorithm.TarGz:
                    _compressionAlgorithm = Tar.Instance;
                    _writerOptions = new WriterOptions(CompressionType.GZip)
                    {
                        LeaveStreamOpen = false
                    };
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
