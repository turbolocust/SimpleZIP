using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression.Algorithm;
using SimpleZIP_UI.Application.Model;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Exceptions;

namespace SimpleZIP_UI.Application.Compression
{
    internal class ArchivingOperation : IDisposable
    {
        /// <summary>
        /// Source for cancellation token.
        /// </summary>
        private readonly CancellationTokenSource _tokenSource;

        /// <summary>
        /// True if an operation is in progress..
        /// </summary>
        internal bool IsRunning { get; private set; }

        public ArchivingOperation()
        {
            _tokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Performs this operation.
        /// </summary>
        /// <param name="archiveInfo">Consists of information about the archive.</param>
        /// <returns>A result object consisting of further details.</returns>
        public async Task<Result> Perform(ArchiveInfo archiveInfo)
        {
            if (archiveInfo == null) return null;

            IsRunning = true;
            var startTime = DateTime.Now;

            Result result;
            switch (archiveInfo.Mode)
            {
                case OperationMode.Compress:
                    result = await CreateArchive(
                        archiveInfo.SelectedFiles,
                        archiveInfo.ArchiveName,
                        archiveInfo.OutputFolder,
                        archiveInfo.ArchiveType);
                    break;
                case OperationMode.Decompress:
                    result = await ExtractFromArchive(
                        archiveInfo.SelectedFiles[0],
                        archiveInfo.OutputFolder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(archiveInfo.Mode));
            }

            var duration = DateTime.Now - startTime;
            result.ElapsedTime = duration;

            IsRunning = false;
            return result;
        }

        /// <summary>
        /// Cancels this operation.
        /// </summary>
        public void Cancel()
        {
            _tokenSource.Cancel();
        }

        /// <summary>
        /// Creates a new archive with compressed versions of the specified files.
        /// </summary>
        /// <param name="files">The files to be compressed.</param>
        /// <param name="archiveName">The name of the archive to be created.</param>
        /// <param name="location">Where to store the archive.</param>
        /// <param name="value">The archive type to be created.</param>
        /// <returns>An object that consists of result parameters.</returns>
        private async Task<Result> CreateArchive(IReadOnlyList<StorageFile> files,
            string archiveName, StorageFolder location, Archives.ArchiveType value)
        {
            var token = _tokenSource.Token;
            var algorithm = Archives.DetermineAlgorithm(value);

            return await Task.Run(async () => // execute compression asynchronously
            {
                var message = "";
                var isSuccess = false;

                if (files.Count > 0)
                {
                    try
                    {
                        var archive = await location.CreateFileAsync(archiveName,
                            CreationCollisionOption.GenerateUniqueName);

                        algorithm.SetCancellationToken(token);
                        isSuccess = await algorithm.Compress(files, archive, location);

                        if (token.IsCancellationRequested)
                        {
                            FileUtils.Delete(archive);
                        }
                    }
                    catch (Exception ex)
                    {
                        message = ex.Message;
                    }
                }

                return EvaluateResult(message, isSuccess);

            }, token);
        }

        /// <summary>
        /// Extracts files from an archive to the specified location.
        /// </summary>
        /// <param name="archiveFile">The archive to be extracted.</param>
        /// <param name="location">The location where to extract the archive's content.</param>
        /// <returns>An object that consists of result parameters.</returns>
        /// <exception cref="InvalidArchiveTypeException">If the file type of the selected 
        /// file is not supported or unknown.</exception>
        /// <exception cref="UnauthorizedAccessException">If extraction at the archive's 
        /// location is not allowed.</exception>
        private async Task<Result> ExtractFromArchive(StorageFile archiveFile, StorageFolder location)
        {
            var fileType = FileUtils.GetFileNameExtension(archiveFile.Name);
            var token = _tokenSource.Token;
            IArchivingAlgorithm algorithm;

            // try to get enum type by file extension, which is the key
            if (Archives.ArchiveFileTypes.TryGetValue(fileType, out Archives.ArchiveType value)
                || Archives.ArchiveExtendedFileTypes.TryGetValue(fileType, out value))
            {
                algorithm = Archives.DetermineAlgorithm(value);
            }
            else
            {
                throw new InvalidArchiveTypeException("The selected file format is not supported.");
            }

            return await Task.Run(async () => // execute extraction asynchronously
            {
                var message = "";
                var isSuccess = false;

                try
                {
                    algorithm.SetCancellationToken(token);
                    isSuccess = await algorithm.Extract(archiveFile, location);
                }
                catch (IOException ex)
                {
                    message = ex.Message;
                }

                return EvaluateResult(message, isSuccess);

            }, token);
        }

        /// <summary>
        /// Evaluates the operation and returns the result.
        /// </summary>
        /// <param name="message">The message to be evaluated.</param>
        /// <param name="isSuccess">True if operation was successful, false otherwise.</param>
        /// <returns>An object that consists of result parameters.</returns>
        private Result EvaluateResult(string message, bool isSuccess)
        {
            Result.Status status;

            if (_tokenSource.IsCancellationRequested)
            {
                status = Result.Status.Interrupt;
            }
            else
            {
                status = isSuccess
                    ? Result.Status.Success
                    : Result.Status.Fail;
            }

            return new Result
            {
                StatusCode = status,
                Message = message
            };
        }

        public void Dispose()
        {
            IsRunning = false;
            _tokenSource.Dispose();
        }
    }
}
