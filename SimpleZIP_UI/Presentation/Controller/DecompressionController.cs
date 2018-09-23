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
using System.Text;
using System.Threading.Tasks;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Operation;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Presentation.Controller
{
    internal class DecompressionController : SummaryPageController<DecompressionInfo>
    {
        internal DecompressionController(INavigation navHandler) : base(navHandler)
        {
        }

        /// <inheritdoc cref="SummaryPageController{T}.GetArchivingOperation"/>
        protected override ArchivingOperation<DecompressionInfo> GetArchivingOperation()
        {
            return Operations.ForDecompression();
        }

        /// <inheritdoc cref="SummaryPageController{T}.PerformOperation"/>
        protected override async Task<Result> PerformOperation(DecompressionInfo[] operationInfos)
        {
            var resultMessage = new StringBuilder();
            var verboseMessage = new StringBuilder();
            var statusCode = Result.Status.Fail;
            int successCount = 0;

            foreach (var operationInfo in operationInfos)
            {
                if (IsCancelRequest) break;
                var item = operationInfo.Item;

                try
                {
                    Result subResult;
                    try
                    {
                        subResult = await Operation.Perform(operationInfo, false);
                    }
                    catch (SharpCompress.Common.CryptographicException)
                    {
                        // archive is encrypted, ask for password and try again
                        string password = await RequestPassword(operationInfo.Item.Name);
                        operationInfo.Item.Password = password;
                        subResult = await Operation.Perform(operationInfo, false);
                    }

                    if (subResult.StatusCode != Result.Status.Success)
                    {
                        statusCode = Result.Status.PartialFail;
                        resultMessage.AppendLine(I18N.Resources
                            .GetString("ArchiveNotExtracted/Text", item.Name));
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
                    if (ex is OperationCanceledException && IsCancelRequest)
                    {
                        statusCode = Result.Status.Interrupt;
                        IsCancelRequest = false; // reset
                        break;
                    }
                    statusCode = Result.Status.PartialFail;
                    verboseMessage.AppendLine(ex.Message);
                }
            }

            if (successCount == operationInfos.Length)
            {
                statusCode = Result.Status.Success;
            }

            return new Result
            {
                StatusCode = statusCode,
                Message = resultMessage.ToString(),
                VerboseMessage = verboseMessage.ToString()
            };
        }
    }
}
