﻿// ==++==
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

using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Util;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SimpleZIP_UI.Application.Compression.Operation.Job
{
    internal class DecompressionJob : ArchivingJob<DecompressionInfo>
    {
        /// <summary>
        /// Used to request password if archive is protected.
        /// </summary>
        public IPasswordRequest PasswordRequest { get; set; }

        /// <inheritdoc />
        public DecompressionJob(ArchivingOperation<DecompressionInfo> operation,
            params DecompressionInfo[] infos) : base(operation, infos)
        {
        }

        /// <inheritdoc />
        public override async Task<Result> Run(ICancelRequest cancelReq)
        {
            var resultMessage = new StringBuilder();
            var verboseMessage = new StringBuilder();
            var statusCode = Result.Status.Fail;
            int successCount = 0;

            foreach (var operationInfo in OperationInfos)
            {
                if (cancelReq.IsCancelRequest) break;
                Result subResult = null;

                try
                {
                    try
                    {
                        subResult = await Operation.Perform(operationInfo, false);
                    }
                    catch (SharpCompress.Common.CryptographicException)
                    {
                        // archive is encrypted, will request password and try again
                    }
                    catch (ICSharpCode.SharpZipLib.SharpZipBaseException ex)
                    {
                        if (!ex.Message.Equals("No password set.")) throw;
                        // only continue and request password if none is set
                    }

                    if (subResult == null)
                    {
                        operationInfo.Item.Password = await PasswordRequest
                            .RequestPassword(operationInfo.Item.Name);
                        subResult = await Operation.Perform(operationInfo, false);
                    }

                    if (subResult.StatusCode != Result.Status.Success)
                    {
                        statusCode = Result.Status.PartialFail;
                        resultMessage.AppendLine(I18N.Resources
                            .GetString("ArchiveNotExtracted/Text",
                                operationInfo.Item.Name));
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

            return new Result
            {
                StatusCode = statusCode,
                Message = resultMessage.ToString(),
                VerboseMessage = verboseMessage.ToString()
            };
        }
    }
}
