// ==++==
// 
// Copyright (C) 2018 Matthias Fussenegger
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// ==--==
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Reader;
using SimpleZIP_UI.Application.Util;

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
        /// <exception cref="InvalidArchiveTypeException">Thrown if the file type of the selected 
        /// file is not supported or unknown.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if extraction at the archive's 
        /// location is not allowed.</exception>
        /// <exception cref="OperationCanceledException">Thrown if operation has been canceled.</exception>
        private async Task<Result> ExtractFromArchive(StorageFile archiveFile, StorageFolder location,
            IReadOnlyList<FileEntry> entries = null)
        {
            var token = TokenSource.Token;

            return await Task.Run(async () => // execute extraction asynchronously
            {
                var message = string.Empty;
                var isSuccess = false;
                var isVerbose = false;

                try
                {
                    Algorithm.Token = token;
                    var stream = entries == null
                        ? await Algorithm.Decompress(archiveFile, location)
                        : await Algorithm.Decompress(archiveFile, location, entries);
                    isSuccess = stream != Stream.Null;
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                    {
                        throw; // simply re-throw
                    }

                    message = await I18N.ExceptionMessageHandler
                                    .GetStringFor(ex, false, archiveFile);

                    if (message.Length > 0)
                    {
                        isVerbose = true;
                    }
                    else
                    {
                        message = ex.Message; // default message not accepted
                    }
                }
                return EvaluateResult(archiveFile.Name, message, isSuccess, isVerbose);
            }, token);
        }

        /// <inheritdoc cref="ArchivingOperation{T}.SetAlgorithm"/>
        protected override void SetAlgorithm(DecompressionInfo info)
        {
            var fileType = FileUtils.GetFileNameExtension(info.Item.Archive.Name);
            // try to get enum type by filename extension, which is the key
            if (Archives.ArchiveFileTypes.TryGetValue(fileType, out var value)
                || Archives.ArchiveExtendedFileTypes.TryGetValue(fileType, out value))
            {
                Algorithm = Archives.DetermineAlgorithm(value);
            }
            else
            {
                throw new InvalidArchiveTypeException(
                    I18N.Resources.GetString("FileFormatNotSupported/Text"));
            }
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
