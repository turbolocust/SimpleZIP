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
    internal class CompressionSummaryPageControl : BaseControl
    {
        internal CompressionSummaryPageControl(Page parent) : base(parent)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedFiles"></param>
        /// <param name="archiveName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        internal async Task<Result> StartButtonAction(IReadOnlyList<StorageFile> selectedFiles, string archiveName, Algorithm key)
        {
            string message;
            try
            {
                InitOperation();

                try
                {
                    var handler = CompressionFacade.Instance;
                    var token = CancellationToken.Token;

                    if (key.Equals(Algorithm.GZip)) // requires special treatment
                    {
                        var totalDuration = 0d;
                        var resultMessage = "";

                        foreach (var file in selectedFiles)
                        {
                            if (token.IsCancellationRequested) break;

                            var result = await handler.CreateArchive(file, archiveName, OutputFolder, key, token);
                            if (result.StatusCode < 0)
                            {
                                totalDuration += result.ElapsedTime;
                            }
                            else
                            {
                                resultMessage += "\nFile " + file.DisplayName + " could not be compressed.";
                            }
                        }

                        return new Result()
                        {
                            Message = resultMessage,
                            ElapsedTime = totalDuration
                        };
                    }
                    return await handler.CreateArchive(selectedFiles, archiveName, OutputFolder, key, token);
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
