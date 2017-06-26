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
        private static CompressionFacade _instance;

        public static CompressionFacade Instance => _instance ?? (_instance = new CompressionFacade());

        private CompressionFacade()
        {
            // singleton
        }

        /// <summary>
        /// Stores the current time when initializing a new operation.
        /// </summary>
        private int _startTime;

        /// <summary>
        /// Stores the result of the operation.
        /// </summary>
        private bool _isSuccess;

        /// <summary>
        /// The algorithm that is used for compressing and decompressing.
        /// </summary>
        private IArchivingAlgorithm _compressionAlgorithm;

        /// <summary>
        /// Optional writer options for compression.
        /// </summary>
        private WriterOptions _writerOptions;

        /// <summary>
        /// Creates a new archive with a compressed version of the specified file.
        /// </summary>
        /// <param name="file">The file to compress.</param>
        /// <param name="archiveName">The name of the archive to create.</param>
        /// <param name="location">Where to store the archive.</param>
        /// <param name="key">The key of the algorithm to use.</param>
        /// <param name="ct">The token that is used to cancel the operation.</param>
        /// <returns>An object that contains result parameters.</returns>
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

                    _isSuccess = archive != null
                        && await _compressionAlgorithm.Compress(file, archive, location, _writerOptions);
                }
                catch (IOException ex)
                {
                    message = ex.Message;
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    message = ex.Message;
                }

                return EvaluateResult(message);

            }, ct);
        }

        /// <summary>
        /// Creates a new archive with compressed versions of the specified files.
        /// </summary>
        /// <param name="files">The files to compress.</param>
        /// <param name="archiveName">The name of the archive to create.</param>
        /// <param name="location">Where to store the archive.</param>
        /// <param name="key">The key of the algorithm to use.</param>
        /// <param name="ct">The token that is used to cancel the operation.</param>
        /// <returns>An object that contains result parameters.</returns>
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

                        _isSuccess = archive != null
                            && await _compressionAlgorithm.Compress(files, archive, location, _writerOptions);
                    }
                    catch (IOException ex)
                    {
                        message = ex.Message;
                    }
                    catch (InvalidArchiveTypeException ex)
                    {
                        message = ex.Message;
                    }
                }

                return EvaluateResult(message);

            }, ct);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="archiveFile"></param>
        /// <param name="location"></param>
        /// <param name="ct">The token that is used to cancel the operation.</param>
        /// <returns></returns>
        /// <exception cref="InvalidArchiveTypeException">If the file type of the selected 
        /// file is not supported or unknown.</exception>
        /// <exception cref="UnauthorizedAccessException">If extraction at the archive's 
        /// path is not allowed.</exception>
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

                return EvaluateResult(message);

            }, ct);
        }

        /// <summary>
        /// Initializes the compression or extraction operation
        /// and chooses the strategy by the specified key.
        /// </summary>
        /// <param name="key">The key of the algorithm.</param>
        private void InitOperation(BaseControl.Algorithm key)
        {
            _writerOptions = null;
            _startTime = DateTime.Now.Millisecond;

            ChooseStrategy(key); // determines the right algorithm
        }

        /// <summary>
        /// Evaluates the operation and returns the result.
        /// </summary>
        /// <param name="message">The message to evaluate.</param>
        /// <returns>An object consisting of result parameters.</returns>
        private Result EvaluateResult(string message)
        {
            var statusCode = message.Length > 0 ? (short)-1 : (short)0;
            var duration = 0;

            if (_isSuccess) // calculate time on success
            {
                duration = Calculator.CalculateElapsedTime(_startTime);
            }

            return new Result
            {
                StatusCode = statusCode,
                Message = message,
                ElapsedTime = duration
            };
        }

        /// <summary>
        /// Assigns the correct algorithm instance to be used by checking its key.
        /// </summary>
        /// <param name="key">The key of the algorithm.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when key matches no algorithm.</exception>
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
