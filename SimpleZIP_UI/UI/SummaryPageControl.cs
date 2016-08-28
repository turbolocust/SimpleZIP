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
        private StorageFolder _outputFolder = ApplicationData.Current.LocalFolder;

        public SummaryPageControl(Page parent) : base(parent)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedFiles"></param>
        /// <param name="archiveName"></param>
        /// <param name="key"></param>
        public async Task<int> StartButtonAction(IReadOnlyList<StorageFile> selectedFiles, string archiveName, Algorithm key)
        {
            var archive = new FileInfo(Path.Combine(_outputFolder.Path, archiveName));
            CancellationToken = new CancellationTokenSource();

            try
            {
                var handler = CompressionHandler.Instance;
                return await handler.CreateArchive(selectedFiles, archive, key, CancellationToken.Token);
            }
            catch (OperationCanceledException)
            {
                if (archive.Exists && IsCancelRequest)
                {
                    archive.Delete();
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
