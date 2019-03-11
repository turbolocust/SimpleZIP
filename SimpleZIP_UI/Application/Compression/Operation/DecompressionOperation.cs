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

using SharpCompress.Common;
using SharpCompress.Readers;
using SimpleZIP_UI.Application.Compression.Algorithm;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.I18N;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
            bool collect = info.IsCollectFileNames;

            return await Task.Run(async () => // execute extraction asynchronously
            {
                string message = string.Empty;
                string verboseMsg = string.Empty;
                bool isSuccess = false;

                var options = new ReaderOptions
                {
                    LeaveStreamOpen = false,
                    Password = password,
                    ArchiveEncoding = new ArchiveEncoding
                    {
                        Default = info.Encoding ?? Encoding.UTF8,
                        Password = info.Encoding ?? Encoding.UTF8
                    }
                };

                try
                {
                    Algorithm.Token = token;
                    var stream = entries == null
                        ? await Algorithm.Decompress(archiveFile, location, options)
                        : await Algorithm.Decompress(archiveFile, location, entries, collect, options);
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

                    message = await ExceptionMessages.GetStringFor(ex, false, passwordSet, archiveFile);
                    verboseMsg = ex.Message;
                }

                return EvaluateResult(archiveFile.Name, message, verboseMsg, isSuccess);
            }, token);
        }

        /// <inheritdoc cref="ArchivingOperation{T}.GetAlgorithmAsync"/>
        protected override async Task<ICompressionAlgorithm> GetAlgorithmAsync(DecompressionInfo info)
        {
            var fileType = FileUtils.GetFileNameExtension(info.Item.Archive.Name);
            var value = Archives.DetermineArchiveTypeByFileExtension(fileType);

            if (value != Archives.ArchiveType.Unknown)
            {
                return Archives.DetermineAlgorithm(value);
            }

            try
            {
                // try to detect archive by reading file headers
                var result = await Archives.DetermineArchiveType(info.Item.Archive);

                if (result != Archives.ArchiveType.Unknown)
                {
                    return Archives.DetermineAlgorithm(result);
                }

                throw new InvalidArchiveTypeException("Archive type is unknown.");
            }
            catch (InvalidArchiveTypeException ex)
            {
                string friendlyErrMsg = I18N.Resources
                    .GetString("FileFormatNotSupported/Text");
                throw new InvalidArchiveTypeException(friendlyErrMsg, ex);
            }
        }

        /// <inheritdoc cref="ArchivingOperation{T}.StartOperation"/>
        protected override async Task<Result> StartOperation(DecompressionInfo info)
        {
            return await ExtractFromArchive(info);
        }
    }
}
