using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Application.Compression.Operation
{
    internal class CompressionOperation : ArchivingOperation<CompressionInfo>
    {
        /// <summary>
        /// Creates a new archive with compressed versions of the specified files.
        /// </summary>
        /// <param name="files">The files to be compressed.</param>
        /// <param name="archiveName">The name of the archive to be created.</param>
        /// <param name="location">Where to store the archive.</param>
        /// <param name="value">The archive type to be created.</param>
        /// <returns>An object that consists of result parameters.</returns>
        /// <exception cref="OperationCanceledException">Thrown if operation has been canceled.</exception>
        private async Task<Result> CreateArchive(IReadOnlyList<StorageFile> files,
            string archiveName, StorageFolder location, Archives.ArchiveType value)
        {
            var token = TokenSource.Token;
            var algorithm = Archives.DetermineAlgorithm(value);

            return await Task.Run(async () => // execute compression asynchronously
            {
                var message = string.Empty;
                var isSuccess = false;

                if (files.Count > 0)
                {
                    var archive = await location.CreateFileAsync(archiveName,
                        CreationCollisionOption.GenerateUniqueName);
                    try
                    {
                        algorithm.Token = token;
                        isSuccess = await algorithm.Compress(files, archive, location);

                        if (token.IsCancellationRequested)
                        {
                            FileUtils.Delete(archive);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is TaskCanceledException)
                        {
                            throw new OperationCanceledException();
                        }
                        message = ex.Message;
                    }
                    finally
                    {
                        if (token.IsCancellationRequested)
                        {
                            FileUtils.Delete(archive);
                        }
                    }
                }

                return EvaluateResult(message, isSuccess);

            }, token);
        }

        /// <inheritdoc cref="ArchivingOperation{T}.StartOperation"/>
        protected override async Task<Result> StartOperation(CompressionInfo info)
        {
            return await CreateArchive(
                info.SelectedFiles,
                info.ArchiveName,
                info.OutputFolder,
                info.ArchiveType);
        }
    }
}
