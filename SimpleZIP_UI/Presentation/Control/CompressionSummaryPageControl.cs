// ==++==
// 
// Copyright (C) 2017 Matthias Fussenegger
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
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Compression;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Operation;

namespace SimpleZIP_UI.Presentation.Control
{
    internal class CompressionSummaryPageControl : SummaryPageControl<CompressionInfo>
    {
        internal CompressionSummaryPageControl(Page parent) : base(parent)
        {
        }

        /// <inheritdoc cref="SummaryPageControl{T}.GetArchivingOperation"/>
        protected override ArchivingOperation<CompressionInfo> GetArchivingOperation()
        {
            return new CompressionOperation();
        }

        /// <inheritdoc cref="SummaryPageControl{T}.PerformOperation"/>
        protected override async Task<Result> PerformOperation(CompressionInfo[] operationInfos)
        {
            var operationInfo = operationInfos[0]; // since use case does not support multiple operations
            var selectedFiles = operationInfo.SelectedFiles;
            var key = operationInfo.ArchiveType;
            var resultMessage = new StringBuilder();
            Result.Status statusCode;

            CheckFileSizes(selectedFiles);

            try
            {
                var result = key.Equals(Archives.ArchiveType.GZip)
                            || key.Equals(Archives.ArchiveType.BZip2)
                    ? await CompressSeparately(selectedFiles, operationInfo)
                    : await Operation.Perform(operationInfo);
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
                resultMessage.AppendLine(ex.Message);
            }

            return new Result
            {
                StatusCode = statusCode,
                Message = resultMessage.ToString()
            };
        }

        /// <summary>
        /// Compresses each of the specified files separately.
        /// </summary>
        /// <param name="files">The files to be compressed.</param>
        /// <param name="operationInfo">The amount of operations to be performed.</param>
        /// <returns>A result object consisting of further details.</returns>
        private async Task<Result> CompressSeparately(IReadOnlyCollection<StorageFile> files, CompressionInfo operationInfo)
        {
            var subMessage = new StringBuilder();
            var successCount = 0;
            var statusCode = Result.Status.Fail;

            foreach (var file in files) // compress each file separately
            {
                if (IsCancelRequest) break;

                operationInfo.SelectedFiles = new[] { file };
                var subResult = await Operation.Perform(operationInfo);

                if (subResult.StatusCode != Result.Status.Success)
                {
                    statusCode = Result.Status.PartialFail;
                    subMessage.AppendLine(I18N.Resources.GetString("FileNotCompressed/Text", file.DisplayName));
                }
                else
                {
                    ++successCount;
                }
                subMessage.AppendLine(subResult.Message);
            }
            if (successCount == files.Count)
            {
                statusCode = Result.Status.Success;
            }

            return new Result
            {
                StatusCode = statusCode,
                Message = subMessage.ToString()
            };
        }
    }
}
