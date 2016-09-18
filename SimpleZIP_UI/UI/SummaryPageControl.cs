using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Common.Compression;
using SimpleZIP_UI.UI.Factory;

namespace SimpleZIP_UI.UI
{
    /// <summary>
    /// Handles complex operations for the corresponding GUI controller.
    /// </summary>
    internal class SummaryPageControl : UI.Control
    {
        /// <summary>
        /// Where the archive will be saved to.
        /// </summary>
        private StorageFolder _outputFolder;

        public SummaryPageControl(Page parent) : base(parent)
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

            if (_outputFolder == null)
            {
                throw new NullReferenceException("No valid output folder selected.");
            }

            try
            {
                var handler = CompressionHandler.Instance;
                return await handler.CreateArchive(selectedFiles, archiveName, _outputFolder, key, CancellationToken.Token);
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

        /// <summary>
        /// Opens a picker to select a folder and returns it.
        /// </summary>
        public async Task<StorageFolder> OutputPathPanelAction()
        {
            var picker = PickerFactory.CreateFolderPicker();

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null) // system has now access to folder
            {
                _outputFolder = folder;
            }
            return folder;
        }
    }
}
