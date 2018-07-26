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
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Operation;

namespace SimpleZIP_UI.Presentation.Controller
{
    internal class CompressionPageController : SummaryPageController<CompressionInfo>
    {
        internal CompressionPageController(INavigation navHandler) : base(navHandler)
        {
        }

        /// <inheritdoc cref="SummaryPageController{T}.GetArchivingOperation"/>
        protected override ArchivingOperation<CompressionInfo> GetArchivingOperation()
        {
            return new CompressionOperation();
        }

        /// <inheritdoc cref="SummaryPageController{T}.PerformOperation"/>
        protected override async Task<Result> PerformOperation(CompressionInfo[] operationInfos)
        {
            var operationInfo = operationInfos[0]; // since use case doesn't support multiple operations
            string[] archiveNames; // names might change in case of file collision
            var resultMessage = new StringBuilder();
            Result.Status statusCode;

            try
            {
                var key = operationInfo.ArchiveType;
                var result = key.Equals(Archives.ArchiveType.GZip)
                            || key.Equals(Archives.ArchiveType.BZip2)
                    ? await CompressSeparately(operationInfo)
                    : await Operation.Perform(operationInfo);
                archiveNames = result.ArchiveNames;
                resultMessage.Append(result.Message);
                statusCode = result.StatusCode;
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException && IsCancelRequest)
                {
                    statusCode = Result.Status.Interrupt;
                    IsCancelRequest = false; // reset
                }
                else
                {
                    statusCode = Result.Status.Fail;
                }
                archiveNames = new string[0];
                resultMessage.AppendLine(ex.Message);
            }

            return new Result(archiveNames)
            {
                StatusCode = statusCode,
                Message = resultMessage.ToString()
            };
        }

        private async Task<Result> CompressSeparately(CompressionInfo operationInfo)
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
                    if (IsCancelRequest) break;

                    operationInfo.SelectedFiles = new[] { file };
                    var subResult = await Operation.Perform(operationInfo, false);
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
                    subMessage.AppendLine(subResult.Message);
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
