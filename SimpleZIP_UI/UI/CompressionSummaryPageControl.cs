using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Common.Compression;

namespace SimpleZIP_UI.UI
{
    /// <summary>
    /// Handles complex operations for the corresponding GUI controller.
    /// </summary>
    internal class CompressionSummaryPageControl : Control
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
        public async Task<int> StartButtonAction(IReadOnlyList<StorageFile> selectedFiles, string archiveName, Algorithm key)
        {
            CancellationToken = new CancellationTokenSource();

            if (OutputFolder == null)
            {
                throw new NullReferenceException("No valid output folder selected.");
            }

            try
            {
                var handler = CompressionHandler.Instance;
                return await handler.CreateArchive(selectedFiles, archiveName, OutputFolder, key, CancellationToken.Token);
            }
            catch (OperationCanceledException)
            {
                if (IsCancelRequest)
                {
                    IsCancelRequest = false;
                }

                return -1;
            }
        }
    }
}
