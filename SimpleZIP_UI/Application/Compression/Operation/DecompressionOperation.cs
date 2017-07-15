using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression.Algorithm;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Exceptions;

namespace SimpleZIP_UI.Application.Compression.Operation
{
    internal class DecompressionOperation : ArchivingOperation<DecompressionInfo>
    {
        /// <summary>
        /// Extracts files from an archive to the specified location.
        /// </summary>
        /// <param name="archiveFile">The archive to be extracted.</param>
        /// <param name="location">The location where to extract the archive's content.</param>
        /// <param name="entries">Optional entries to be extracted from the archive.</param>
        /// <returns>An object that consists of result parameters.</returns>
        /// <exception cref="InvalidArchiveTypeException">If the file type of the selected 
        /// file is not supported or unknown.</exception>
        /// <exception cref="UnauthorizedAccessException">If extraction at the archive's 
        /// location is not allowed.</exception>
        private async Task<Result> ExtractFromArchive(StorageFile archiveFile, StorageFolder location,
            IReadOnlyList<FileEntry> entries = null)
        {
            var fileType = FileUtils.GetFileNameExtension(archiveFile.Name);
            var token = TokenSource.Token;
            ICompressionAlgorithm algorithm;

            // try to get enum type by file extension, which is the key
            if (Archives.ArchiveFileTypes.TryGetValue(fileType, out Archives.ArchiveType value)
                || Archives.ArchiveExtendedFileTypes.TryGetValue(fileType, out value))
            {
                algorithm = Archives.DetermineAlgorithm(value);
            }
            else
            {
                throw new InvalidArchiveTypeException(
                    I18N.Resources.GetString("FileFormatNotSupported/Text"));
            }

            return await Task.Run(async () => // execute extraction asynchronously
            {
                var message = string.Empty;
                var isSuccess = false;

                try
                {
                    algorithm.Token = token;
                    isSuccess = entries == null
                        ? await algorithm.Decompress(archiveFile, location)
                        : await algorithm.Decompress(archiveFile, location, entries);
                }
                catch (IOException ex)
                {
                    message = ex.Message;
                }

                return EvaluateResult(message, isSuccess);

            }, token);
        }

        /// <inheritdoc cref="ArchivingOperation{T}.StartOperation"/>
        protected override async Task<Result> StartOperation(DecompressionInfo info)
        {
            var archive = info.Item.Archive;
            var entries = info.Item.Entries;
            return await ExtractFromArchive(archive, info.OutputFolder, entries);
        }
    }
}
