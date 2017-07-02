using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Model;

namespace SimpleZIP_UI.Presentation
{
    internal class CompressionSummaryPageControl : SummaryPageControl
    {
        internal CompressionSummaryPageControl(Page parent) : base(parent)
        {
        }

        protected override async Task<Result> PerformOperation(ArchiveInfo archiveInfo)
        {
            var selectedFiles = archiveInfo.SelectedFiles;
            var key = archiveInfo.Algorithm;
            var message = "";

            Result result = null;

            try
            {
                try
                {
                    if (key.Equals(Algorithm.GZip)) // requires special treatment
                    {
                        var totalDuration = new TimeSpan(0);
                        var resultMessage = "";

                        foreach (var file in selectedFiles)
                        {
                            if (IsCancelRequest) break;

                            archiveInfo.SelectedFiles = new[] { file };
                            var subResult = await Operation.Perform(archiveInfo);
                            if (subResult.StatusCode == Result.Status.Success)
                            {
                                totalDuration = totalDuration.Add(subResult.ElapsedTime);
                            }
                            else
                            {
                                resultMessage += "\nFile " + file.DisplayName + " was not compressed.";
                            }
                        }
                        result = new Result { Message = resultMessage, ElapsedTime = totalDuration };
                    }
                    else
                    {
                        result = await Operation.Perform(archiveInfo);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException && IsCancelRequest)
                    {
                        IsCancelRequest = false; // reset
                    }
                    message = ex.Message;
                }
            }
            catch (NullReferenceException ex)
            {
                message = ex.Message;
            }

            return result ?? new Result
            {
                StatusCode = Result.Status.Fail,
                Message = message
            };
        }
    }
}
