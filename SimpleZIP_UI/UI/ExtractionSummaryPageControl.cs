using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Common.Compression;
using SimpleZIP_UI.Exceptions;

namespace SimpleZIP_UI.UI
{
    /// <summary>
    /// Handles complex operations for the corresponding GUI controller.
    /// </summary>
    internal class ExtractionSummaryPageControl : BaseControl
    {
        public ExtractionSummaryPageControl(Page parent) : base(parent)
        {
        }

        public async Task<string[]> StartButtonAction(IReadOnlyList<StorageFile> selectedFiles, Algorithm key)
        {
            try
            {
                InitOperation();
                try
                {
                    var handler = CompressionHandler.Instance;

                    if (selectedFiles.Count > 1) // multiple files selected
                    {
                        var totalDuration = 0d;
                        var message = "";

                        foreach(var file in selectedFiles)
                        {
                            var result = await handler.ExtractFromArchive(file, OutputFolder, CancellationToken.Token);

                            var duration = 0d;
                            if (double.TryParse(result[0], out duration))
                            {
                                totalDuration += duration;
                            }
                            else
                            {
                                message += "\nArchive named " + file.DisplayName + " could not be extracted.";
                            }
                        }

                        return new[] { totalDuration.ToString(CultureInfo.InvariantCulture), message };
                    }

                    return await handler.ExtractFromArchive(selectedFiles[0], OutputFolder, CancellationToken.Token);
                }
                catch (OperationCanceledException ex)
                {
                    if (IsCancelRequest)
                    {
                        IsCancelRequest = false; // reset
                    }

                    return new[] { "-1", ex.Message };
                }
                catch (InvalidFileTypeException ex)
                {
                    return new[] { "-1", ex.Message };
                }
            }
            catch (NullReferenceException ex)
            {
                return new[] { "-1", ex.Message };
            }
        }
    }
}
