using System;
using System.Collections.Generic;
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
    internal class CompressionSummaryPageControl : BaseControl
    {
        public CompressionSummaryPageControl(Page parent) : base(parent)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedFiles"></param>
        /// <param name="archiveName"></param>
        /// <param name="key"></param>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<string[]> StartButtonAction(IReadOnlyList<StorageFile> selectedFiles, string archiveName, Algorithm key)
        {
            try
            {
                InitOperation();
                try
                {
                    var handler = CompressionHandler.Instance;
                    return await handler.CreateArchive(selectedFiles, archiveName, OutputFolder, key, CancellationToken.Token);

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
