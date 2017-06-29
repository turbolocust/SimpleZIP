using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Common.Compression;
using SimpleZIP_UI.Common.Model;
using SimpleZIP_UI.Exceptions;

namespace SimpleZIP_UI.UI
{
    /// <summary>
    /// Handles complex operations for the corresponding GUI controller.
    /// </summary>
    internal class ExtractionSummaryPageControl : BaseControl
    {
        internal ExtractionSummaryPageControl(Page parent) : base(parent)
        {
        }

        /// <summary>
        /// Performs an action after the start button has been pressed.
        /// </summary>
        /// <param name="selectedFiles">A list of selected files.</param>
        /// <returns>An object that consists of result parameters.</returns>
        internal async Task<Result> StartButtonAction(IReadOnlyList<StorageFile> selectedFiles)
        {
            string message;
            try
            {
                InitOperation();
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

                            var result = await handler.ExtractFromArchive(file, OutputFolder, token);
                            if (result.StatusCode < 0)
                            {
                                totalDuration = totalDuration.Add(result.ElapsedTime);
                            }
                            else
                            {
                                resultMessage += "\nArchive " + file.DisplayName + " could not be extracted.";
                            }
                        }

                        return new Result
                        {
                            Message = resultMessage,
                            ElapsedTime = totalDuration
                        };
                    }
                    return await handler.ExtractFromArchive(selectedFiles[0], OutputFolder, token);
                }
                catch (OperationCanceledException ex)
                {
                    if (IsCancelRequest)
                    {
                        IsCancelRequest = false; // reset
                    }
                    message = ex.Message;
                }
                catch (InvalidArchiveTypeException ex)
                {
                    message = ex.Message;
                }
            }
            catch (NullReferenceException ex)
            {
                message = ex.Message;
            }

            return new Result
            {
                Message = message,
                StatusCode = Result.Status.Fail
            };
        }
    }
}
