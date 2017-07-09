using System;
using System.Text;
using System.Threading.Tasks;
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
            var statusCode = Result.Status.Fail;

            CheckFileSizes(selectedFiles);

            try
            {
                if (key.Equals(Archives.ArchiveType.GZip)) // requires special treatment
                {
                    var subMessage = new StringBuilder();
                    var successCount = 0;
                    // compress each file separately
                    foreach (var file in selectedFiles)
                    {
                        if (IsCancelRequest) break;

                        operationInfo.SelectedFiles = new[] { file };
                        var subResult = await Operation.Perform(operationInfo);

                        if (subResult.StatusCode != Result.Status.Success)
                        {
                            statusCode = Result.Status.PartialFail;
                            subMessage.AppendLine("File ").Append(file.DisplayName).Append(" was not compressed.");
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
                    resultMessage.Append(subMessage);
                }
                else
                {
                    var subResult = await Operation.Perform(operationInfo);
                    resultMessage.Append(subResult.Message);
                    statusCode = subResult.StatusCode;
                }
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
    }
}
