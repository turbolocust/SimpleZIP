// ==++==
// 
// Copyright (C) 2020 Matthias Fussenegger
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

using SimpleZIP_UI.Business.Compression.Algorithm;
using SimpleZIP_UI.Business.Compression.Algorithm.Options;
using SimpleZIP_UI.Business.Compression.Model;
using SimpleZIP_UI.Business.Util;
using SimpleZIP_UI.I18N;
using System;
using System.Threading.Tasks;
using Serilog;
using SimpleZIP_UI.Business.Compression.Algorithm.Factory;

namespace SimpleZIP_UI.Business.Compression.Operation
{
    internal class DecompressionOperation : ArchivingOperation<DecompressionInfo>
    {
        private readonly ILogger _logger = Log.ForContext<DecompressionOperation>();

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
            bool collect = info.IsCollectFileNames;

            return await Task.Run(async () => // execute extraction asynchronously
            {
                string message = string.Empty;
                string verboseMsg = string.Empty;
                bool isSuccess = false;

                var options = new DecompressionOptions(info.Encoding, password);

                try
                {
                    Algorithm.Token = token;

                    if (entries.IsNullOrEmpty())
                        await Algorithm.DecompressAsync(archiveFile, location, options).ConfigureAwait(false);
                    else
                        await Algorithm.DecompressAsync(archiveFile, location, entries, collect, options).ConfigureAwait(false);

                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    bool passwordSet = password != null;
                    if (ex is OperationCanceledException ||
                        ex is ArchiveEncryptedException && !passwordSet)
                    {
                        throw; // simply re-throw, but only if password not set
                    }

                    _logger.Error(ex, "Decompressing {ArchiveName} failed", archiveFile.Name);

                    var opType = passwordSet
                        ? ExceptionMessages.OperationType.ReadingPasswordSet
                        : ExceptionMessages.OperationType.Reading;

                    message = await ExceptionMessages.GetStringFor(ex, opType, archiveFile).ConfigureAwait(false);
                    verboseMsg = ex.Message;
                }

                return EvaluateResult(archiveFile.Name, message, verboseMsg, isSuccess);

            }, token).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ArchivingOperation{T}.GetAlgorithmAsync"/>
        protected override async Task<ICompressionAlgorithm> GetAlgorithmAsync(DecompressionInfo info, AlgorithmOptions options)
        {
            string fileType = FileUtils.GetFileNameExtension(info.Item.Archive.Name);
            var value = Archives.DetermineArchiveTypeByFileExtension(fileType);

            if (value != Archives.ArchiveType.Unknown)
            {
                return AlgorithmFactory.DetermineAlgorithm(value, options);
            }

            try
            {
                // try to detect archive by reading file headers
                var result = await Archives.DetermineArchiveType(info.Item.Archive).ConfigureAwait(false);

                if (result != Archives.ArchiveType.Unknown)
                {
                    return AlgorithmFactory.DetermineAlgorithm(value, options);
                }

                throw new InvalidArchiveTypeException(Resources.GetString("UnknownArchiveType/Text"));
            }
            catch (InvalidArchiveTypeException ex)
            {
                string friendlyErrMsg = Resources.GetString("FileFormatNotSupported/Text");
                throw new InvalidArchiveTypeException(friendlyErrMsg, ex);
            }
        }

        /// <inheritdoc cref="ArchivingOperation{T}.StartOperation"/>
        protected override async Task<Result> StartOperation(DecompressionInfo info)
        {
            return await ExtractFromArchive(info).ConfigureAwait(false);
        }
    }
}
