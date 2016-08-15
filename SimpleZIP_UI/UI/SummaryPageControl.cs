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

        /// <summary>
        /// 
        /// </summary>
        private bool _isCancelRequest = false;

        /// <summary>
        /// Token used to cancel the packing task.
        /// </summary>
        private CancellationTokenSource _cancellationToken;

        public SummaryPageControl(Page parent) : base(parent)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedFiles"></param>
        /// <param name="archiveName"></param>
        /// <param name="key"></param>
        public async void StartButtonAction(IReadOnlyList<StorageFile> selectedFiles, string archiveName, Algorithm key)
        {
            var duration = 0; // holds the time of the operation
            var archive = new FileInfo(_outputFolder.Path + "\\" + archiveName);
            _cancellationToken = new CancellationTokenSource();

            try
            {
                var handler = CompressionHandler.Instance;
                duration = await handler.CreateArchive(selectedFiles, archive, key, _cancellationToken.Token);
            }
            catch (OperationCanceledException)
            {
                if (archive.Exists)
                {
                    archive.Delete();
                }
            }
            finally
            {
                if (duration > 0 && !_isCancelRequest)
                {
                    await DialogFactory.
                        CreateInformationDialog("Success", "Total duration: " + duration).ShowAsync();
                }

                _isCancelRequest = false;
                ParentPage.Frame.Navigate(typeof(MainPage));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public async void AbortButtonAction()
        {
            var dialog = DialogFactory.CreateConfirmationDialog("Are you sure?",
                "This will cancel the operation.");

            var result = await dialog.ShowAsync();
            if (result.Id.Equals(0)) // cancel operation
            {
                try
                {
                    _cancellationToken?.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    _isCancelRequest = true;
                }
                finally
                {
                    ParentPage.Frame.Navigate(typeof(MainPage));
                }
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
