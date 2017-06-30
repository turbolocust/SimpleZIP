using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Common.Compression;
using SimpleZIP_UI.Common.Model;

namespace SimpleZIP_UI.UI
{
    /// <summary>
    /// Handles complex operations for the corresponding GUI controller.
    /// </summary>
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

            try
            {
                try
                {
                    var handler = new CompressionFacade();
                    var token = CancellationToken.Token;

                    if (selectedFiles.Count > 1) // multiple files selected
                    {
                        var totalDuration = new TimeSpan();
                        var resultMessage = "";

                        foreach (var file in selectedFiles)
                        {
                            if (token.IsCancellationRequested) break;

                            var subResult = await handler.ExtractFromArchive(file, OutputFolder, token);
                            if (subResult.StatusCode < 0)
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
                        result = await handler.ExtractFromArchive(selectedFiles[0], OutputFolder, token);
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
