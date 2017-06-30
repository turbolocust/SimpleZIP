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
    internal class CompressionSummaryPageControl : SummaryPageControl
    {
        internal CompressionSummaryPageControl(Page parent) : base(parent)
        {
        }

        protected override async Task<Result> PerformOperation(ArchiveInfo archiveInfo)
        {
            var selectedFiles = archiveInfo.SelectedFiles;
            var archiveName = archiveInfo.ArchiveName;
            var key = archiveInfo.Key;
            var message = "";

            Result result = null;

            try
            {
                try
                {
                    var handler = new CompressionFacade();
                    var token = CancellationToken.Token;

                    if (key.Equals(Algorithm.GZip)) // requires special treatment
                    {
                        var totalDuration = new TimeSpan(0);
                        var resultMessage = "";

                        foreach (var file in selectedFiles)
                        {
                            if (token.IsCancellationRequested) break;

                            var subResult = await handler.CreateArchive(file, archiveName, OutputFolder, key, token);
                            if (subResult.StatusCode == Result.Status.Success)
                            {
                                totalDuration = totalDuration.Add(subResult.ElapsedTime);
                            }
                            else
                            {
                                resultMessage += "\nFile " + file.DisplayName + " was not compressed.";
                            }
                        }
                        result = new Result() { Message = resultMessage, ElapsedTime = totalDuration };
                    }
                    else
                    {
                        result = await handler.CreateArchive(selectedFiles, archiveName, OutputFolder, key, token);
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
