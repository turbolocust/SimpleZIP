﻿// ==++==
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
using Windows.Storage;
using Serilog;
using SimpleZIP_UI.Business.Compression.Algorithm.Factory;

namespace SimpleZIP_UI.Business.Compression.Operation
{
    internal class CompressionOperation : ArchivingOperation<CompressionInfo>
    {
        private readonly ILogger _logger = Log.ForContext<CompressionOperation>();

        private async Task<Result> CreateArchive(CompressionInfo info)
        {
            string archiveName = info.ArchiveName;
            var files = info.SelectedFiles;
            var location = info.OutputFolder;
            var token = TokenSource.Token;

            var options = new CompressionOptions(info.Encoding);

            return await Task.Run(async () => // execute compression asynchronously
            {
                string message = string.Empty;
                string verboseMsg = string.Empty;
                string name = archiveName;
                bool isSuccess = false;

                if (files.Count > 0)
                {
                    var archive = await location.CreateFileAsync(archiveName,
                        CreationCollisionOption.GenerateUniqueName);
                    name = archive.Name; // might have changed because of creation collision option

                    try
                    {
                        Algorithm.Token = token;
                        await Algorithm.CompressAsync(files, archive, location, options).ConfigureAwait(false);
                        isSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        if (ex is OperationCanceledException) throw;

                        _logger.Error(ex, "Compressing to {ArchiveName} failed", archive.Name);

                        const ExceptionMessages.OperationType opType = ExceptionMessages.OperationType.Writing;
                        message = await ExceptionMessages.GetStringFor(ex, opType, archive).ConfigureAwait(false);
                        verboseMsg = ex.Message;
                    }
                    finally
                    {
                        if (token.IsCancellationRequested)
                        {
                            FileUtils.DeleteSafely(archive);
                        }
                    }
                }

                return EvaluateResult(name, message, verboseMsg, isSuccess);

            }, token).ConfigureAwait(false);
        }

        /// <inheritdoc cref="ArchivingOperation{T}.GetAlgorithmAsync"/>
        protected override Task<ICompressionAlgorithm> GetAlgorithmAsync(CompressionInfo info, AlgorithmOptions options)
        {
            return Task.FromResult(AlgorithmFactory.DetermineAlgorithm(info.ArchiveType, options));
        }

        /// <inheritdoc cref="ArchivingOperation{T}.StartOperation"/>
        protected override async Task<Result> StartOperation(CompressionInfo info)
        {
            return await CreateArchive(info).ConfigureAwait(false);
        }
    }
}
