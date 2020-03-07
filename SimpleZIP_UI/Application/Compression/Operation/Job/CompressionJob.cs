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

using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Compression.Operation.Job
{
    internal class CompressionJob : ArchivingJob<CompressionInfo>
    {
        /// <inheritdoc />
        public CompressionJob(ArchivingOperation<CompressionInfo> operation,
            params CompressionInfo[] infos) : base(operation, infos)
        {
        }

        /// <inheritdoc />
        public override async Task<Result> Run(ICancelRequest cancelReq)
        {
            var archiveNames = new List<string>(); // names might change in case of file collision
            var resultMessage = new StringBuilder();
            var verboseMessage = new StringBuilder();
            var statusCode = Result.Status.Fail;
            int successCount = 0;

            foreach (var operationInfo in OperationInfos)
            {
                if (cancelReq.IsCancelRequest) break;

                try
                {
                    var key = operationInfo.ArchiveType;
                    var subResult = key.Equals(Archives.ArchiveType.GZip)
                                 || key.Equals(Archives.ArchiveType.BZip2)
                        ? await CompressSeparately(operationInfo, cancelReq).ConfigureAwait(false)
                        : await Operation.Perform(operationInfo).ConfigureAwait(false);
                    archiveNames.AddRange(subResult.ArchiveNames);

                    if (subResult.StatusCode != Result.Status.Success)
                    {
                        statusCode = Result.Status.PartialFail;
                        resultMessage.AppendLine(I18N.Resources
                            .GetString("ArchiveNotCreated/Text",
                                operationInfo.ArchiveName));
                    }
                    else
                    {
                        ++successCount;
                    }

                    resultMessage.CheckAndAppendLine(subResult.Message);
                    verboseMessage.CheckAndAppendLine(subResult.VerboseMessage);
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException && cancelReq.IsCancelRequest)
                    {
                        statusCode = Result.Status.Interrupt;
                        cancelReq.Reset();
                        break;
                    }

                    statusCode = Result.Status.PartialFail;
                    verboseMessage.AppendLine(ex.Message);
                }
            }

            if (successCount == OperationInfos.Count)
            {
                statusCode = Result.Status.Success;
            }

            return new Result(archiveNames.ToArray())
            {
                StatusCode = statusCode,
                Message = resultMessage.ToString(),
                VerboseMessage = verboseMessage.ToString()
            };
        }

        private async Task<Result> CompressSeparately(CompressionInfo operationInfo, ICancelRequest cancelReq)
        {
            var subMessage = new StringBuilder();
            var successCount = 0;
            var statusCode = Result.Status.Fail;
            var selectedFiles = new List<StorageFile>(operationInfo.SelectedFiles);
            var archiveNames = new List<string>(operationInfo.SelectedFiles.Count);

            try
            {
                foreach (var file in selectedFiles) // compress each file separately
                {
                    if (cancelReq.IsCancelRequest) break;

                    operationInfo.SelectedFiles = new[] { file };
                    var subResult = await Operation.Perform(operationInfo, false).ConfigureAwait(false);
                    archiveNames.AddRange(subResult.ArchiveNames);

                    if (subResult.StatusCode != Result.Status.Success)
                    {
                        statusCode = Result.Status.PartialFail;
                        subMessage.AppendLine(I18N.Resources
                            .GetString("FileNotCompressed/Text", file.DisplayName));
                    }
                    else
                    {
                        ++successCount;
                    }

                    subMessage.CheckAndAppendLine(subResult.Message);
                }
                if (successCount == selectedFiles.Count)
                {
                    statusCode = Result.Status.Success;
                }
            }
            finally
            {
                operationInfo.SelectedFiles = selectedFiles;
            }

            return new Result(archiveNames.ToArray())
            {
                StatusCode = statusCode,
                Message = subMessage.ToString()
            };
        }
    }
}
