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
        /// Performs an action after the start button has been tapped.
        /// </summary>
        /// <param name="selectedFiles">A list of selected files.</param>
        /// <param name="archiveName">The name of the archive to be created.</param>
        /// <param name="key">The key of the algorithm to be used.</param>
        /// <returns>An object that consists of result parameters.</returns>
        internal async Task<Result> StartButtonAction(IReadOnlyList<StorageFile> selectedFiles, string archiveName, Algorithm key)
        {
            string message;
            try
            {
                InitOperation();

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

                            var result = await handler.CreateArchive(file, archiveName, OutputFolder, key, token);
                            if (result.StatusCode == Result.Status.Success)
                            {
                                totalDuration = totalDuration.Add(result.ElapsedTime);
                            }
                            else
                            {
                                resultMessage += "\nFile " + file.DisplayName + " was not compressed.";
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
                StatusCode = Result.Status.Fail,
                Message = message
            };
        }
    }
}
