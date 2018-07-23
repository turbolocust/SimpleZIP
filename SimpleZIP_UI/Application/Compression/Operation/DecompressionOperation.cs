﻿// ==++==
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
using System.IO;
using System.Threading.Tasks;
using SharpCompress.Common;
using SharpCompress.Readers;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Application.Compression.Operation
{
    internal class DecompressionOperation : ArchivingOperation<DecompressionInfo>
    {
        /// <summary>
        /// Extracts files from an archive to the specified location.
        /// </summary>
        /// <param name="info">Consists of information required for extraction.</param>
        /// <returns>An object that consists of result parameters.</returns>
        /// <exception cref="InvalidArchiveTypeException">Thrown if the file type of the selected 
        /// file is not supported or unknown.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if extraction at the archive's 
        /// location is not allowed.</exception>
        /// <exception cref="OperationCanceledException">Thrown if operation has been canceled.</exception>
        private async Task<Result> ExtractFromArchive(DecompressionInfo info)
        {
            var token = TokenSource.Token;

            var archiveFile = info.Item.Archive;
            var location = info.OutputFolder;
            var entries = info.Item.Entries;
            string password = info.Item.Password;

            return await Task.Run(async () => // execute extraction asynchronously
            {
                var message = string.Empty;
                var isSuccess = false;
                var isVerbose = false;

                var options = new ReaderOptions
                {
                    LeaveStreamOpen = false,
                    Password = password
                };

                try
                {
                    Algorithm.Token = token;
                    var stream = entries == null
                        ? await Algorithm.Decompress(archiveFile, location, options)
                        : await Algorithm.Decompress(archiveFile, location, entries, options);
                    isSuccess = stream != Stream.Null;
                }
                catch (Exception ex)
                {
                    bool passwordSet = password != null;
                    if (ex is OperationCanceledException ||
                        ex is CryptographicException && !passwordSet)
                    {
                        throw; // simply re-throw, but only if password not set
                    }

                    message = await I18N.ExceptionMessageHandler
                                    .GetStringFor(ex, false, passwordSet, archiveFile);

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
            else // try to detect archive by reading headers
            {
                string errMsg = I18N.Resources
                    .GetString("FileFormatNotSupported/Text");
                try
                {
                    var task = Archives.DetermineArchiveType(
                        info.Item.Archive, info.Item.Password);
                    task.RunSynchronously();

                    if (task.Result != Archives.ArchiveType.Unknown)
                    {
                        Algorithm = Archives.DetermineAlgorithm(task.Result);
                    }
                    else
                    {
                        throw new InvalidArchiveTypeException(errMsg);
                    }
                }
                catch (InvalidArchiveTypeException ex)
                {
                    throw new InvalidArchiveTypeException(errMsg, ex);
                }
            }
        }

        /// <inheritdoc cref="ArchivingOperation{T}.StartOperation"/>
        protected override async Task<Result> StartOperation(DecompressionInfo info)
        {
            return await ExtractFromArchive(info);
        }
    }
}
