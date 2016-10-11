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
        /// Handles the action that needs to be performed when the start button has been pressed.
        /// </summary>
        /// <param name="selectedFiles"></param>
        /// <returns></returns>
        internal async Task<Result> StartButtonAction(IReadOnlyList<StorageFile> selectedFiles)
        {
            string message;
            try
            {
                InitOperation();
                try
                {
                    var handler = CompressionHandler.Instance;

                    if (selectedFiles.Count > 1) // multiple files selected
                    {
                        var totalDuration = 0d;
                        var messageResult = "";

                        foreach (var file in selectedFiles)
                        {
                            var result = await handler.ExtractFromArchive(file, OutputFolder, CancellationToken.Token);

                            if (result.StatusCode < 0)
                            {
                                totalDuration += result.ElapsedTime;
                            }
                            else
                            {
                                messageResult += "\nArchive " + file.DisplayName + " could not be extracted.";
                            }
                        }

                        return new Result
                        {
                            Message = messageResult,
                            ElapsedTime = totalDuration
                        };
                    }

                    return await handler.ExtractFromArchive(selectedFiles[0], OutputFolder, CancellationToken.Token);
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
                StatusCode = -1 // exception was thrown
            };
        }
    }
}
