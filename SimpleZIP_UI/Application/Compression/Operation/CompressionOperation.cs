// ==++==
// 
// Copyright (C) 2019 Matthias Fussenegger
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

using SimpleZIP_UI.Application.Compression.Algorithm;
using SimpleZIP_UI.Application.Compression.Algorithm.Options;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Util;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Operation
{
    internal class CompressionOperation : ArchivingOperation<CompressionInfo>
    {
        private async Task<Result> CreateArchive(CompressionInfo info)
        {
            string archiveName = info.ArchiveName;
            var files = info.SelectedFiles;
            var location = info.OutputFolder;
            var token = TokenSource.Token;

            var options = new CompressionOptions(false, info.Encoding);

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
                        var stream = await Algorithm.Compress(files, archive, location, options);
                        isSuccess = stream != Stream.Null;
                    }
                    catch (Exception ex)
                    {
                        if (ex is OperationCanceledException)
                        {
                            throw;
                        }

                        verboseMsg = ex.Message;
                    }
                    finally
                    {
                        if (token.IsCancellationRequested)
                        {
                            FileUtils.Delete(archive);
                        }
                    }
                }

                return EvaluateResult(name, message, verboseMsg, isSuccess);
            }, token);
        }

        /// <inheritdoc cref="ArchivingOperation{T}.GetAlgorithmAsync"/>
        protected override Task<ICompressionAlgorithm> GetAlgorithmAsync(CompressionInfo info)
        {
            return Task.FromResult(Archives.DetermineAlgorithm(info.ArchiveType));
        }

        /// <inheritdoc cref="ArchivingOperation{T}.StartOperation"/>
        protected override async Task<Result> StartOperation(CompressionInfo info)
        {
            return await CreateArchive(info);
        }
    }
}
