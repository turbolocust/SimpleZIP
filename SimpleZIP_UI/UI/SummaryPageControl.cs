using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Common.Compression;
using SimpleZIP_UI.UI.Factory;

namespace SimpleZIP_UI.UI
{
    /// <summary>
    /// 
    /// </summary>
    internal class SummaryPageControl : UI.Control
    {
        /// <summary>
        /// 
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
        public async void StartButtonAction(IReadOnlyList<StorageFile> selectedFiles, string archiveName)
        {
            var resultText = "";
            var archive = new FileInfo(archiveName);

            _cancellationToken = new CancellationTokenSource();

            try
            {
                var duration =
                    await
                        CompressionHandler.Instance.CreateArchive(selectedFiles, archive.FullName,
                            _cancellationToken.Token);
                resultText += "Operation succeeded.\n\nTotal duration: " + duration;
            }
            catch (OperationCanceledException)
            {
                if (archive.Exists)
                {
                    archive.Delete();
                }
                resultText += "Operation cancelled.";
            }
            finally
            {
                await MessageDialogFactory.CreateInformationDialog("", resultText).ShowAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public async void AbortButtonAction()
        {
            var dialog = MessageDialogFactory.CreateConfirmationDialog("Are you sure?",
                "This will cancel the operation.");

            var result = await dialog.ShowAsync();
            if (result.Id.Equals(0)) // cancel operation
            {
                _cancellationToken?.Cancel();
                ParentPage.Frame.Navigate(typeof(MainPage));
            }
        }
    }
}
