using System;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Operation;

namespace SimpleZIP_UI.Presentation.Control
{
    internal class ExtractionSummaryPageControl : SummaryPageControl<DecompressionInfo>
    {
        internal ExtractionSummaryPageControl(Page parent) : base(parent)
        {
        }

        /// <inheritdoc cref="SummaryPageControl{T}.GetArchivingOperation"/>
        protected override ArchivingOperation<DecompressionInfo> GetArchivingOperation()
        {
            return new DecompressionOperation();
        }

        /// <inheritdoc cref="SummaryPageControl{T}.PerformOperation"/>
        protected override async Task<Result> PerformOperation(DecompressionInfo[] operationInfos)
        {
            var subMessage = new StringBuilder();
            var resultMessage = new StringBuilder();
            var statusCode = Result.Status.Fail;
            var successCount = 0;

            foreach (var operationInfo in operationInfos)
            {
                if (IsCancelRequest) break;
                var item = operationInfo.Item;
                CheckFileSizes(item);
                try
                {
                    var subResult = await Operation.Perform(operationInfo);
                    if (subResult.StatusCode != Result.Status.Success)
                    {
                        statusCode = Result.Status.PartialFail;
                        subMessage.AppendLine("Archive ").Append(item.DisplayName).Append(" could not be extracted.");
                    }
                    else
                    {
                        ++successCount;
                    }
                    subMessage.AppendLine(subResult.Message);
                    resultMessage.AppendLine(subMessage.ToString());
                    subMessage.Clear();
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
                    resultMessage.AppendLine(ex.Message);
                }
            }

            if (successCount == operationInfos.Length)
            {
                statusCode = Result.Status.Success;
            }

            return new Result
            {
                StatusCode = statusCode,
                Message = resultMessage.ToString()
            };
        }
    }
}
