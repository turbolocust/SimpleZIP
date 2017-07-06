using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Model;

namespace SimpleZIP_UI.Presentation
{
    internal class ExtractionSummaryPageControl : SummaryPageControl
    {
        internal ExtractionSummaryPageControl(Page parent) : base(parent)
        {
        }

        protected override async Task<Result> PerformOperation(ArchiveInfo archiveInfo)
        {
            var selectedFiles = archiveInfo.SelectedFiles;
            var message = "";

            Result result = null;
            CheckFileSizes(selectedFiles);

            try
            {
                try
                {
                    if (selectedFiles.Count > 1) // multiple files selected
                    {
                        var totalDuration = new TimeSpan();
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
                                resultMessage += "\nArchive " + file.DisplayName + " could not be extracted.";
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
