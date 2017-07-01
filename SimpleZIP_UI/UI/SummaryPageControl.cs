using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Common.Model;
using SimpleZIP_UI.UI.Factory;

namespace SimpleZIP_UI.UI
{
    internal abstract class SummaryPageControl : BaseControl
    {

        /// <summary>
        /// True if a cancel request has been made.
        /// </summary>
        public bool IsCancelRequest { get; protected set; }

        /// <summary>
        /// True if an operation is running.
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Token used to cancel an archiving operation.
        /// </summary>
        protected CancellationTokenSource CancellationToken;

        /// <summary>
        /// Where the archive or its content will be saved to.
        /// </summary>
        protected StorageFolder OutputFolder;

        internal SummaryPageControl(Page parent) : base(parent)
        {
            CancellationToken = null;
        }

        /// <summary>
        /// Performs an action after the start button has been tapped.
        /// </summary>
        /// <param name="archiveInfo">Consists of information about the archive.</param>
        /// <returns>True on success, false otherwise.</returns>
        internal async Task<Result> StartButtonAction(ArchiveInfo archiveInfo)
        {
            InitOperation();
            var result = await PerformOperation(archiveInfo);
            FinalizeOperation();
            return result;
        }

        /// <summary>
        /// Performs an action after the abort button has been pressed.
        /// </summary>
        internal void AbortButtonAction()
        {
            try
            {
                CancellationToken?.Cancel();
                if (!IsRunning)
                {
                    NavigateBackHome();
                }
            }
            catch (ObjectDisposedException)
            {
                IsCancelRequest = true;
            }
        }

        /// <summary>
        /// Enables the user to cancel the operation via the Back button.
        /// </summary>
        /// <returns>True if cancel request has been confirmed, false otherwise.</returns>
        internal async Task<bool> ConfirmCancellationRequest()
        {
            if (IsRunning && !IsCancelRequest)
            {
                var dialog = DialogFactory.CreateConfirmationDialog("Please confirm",
                    "\nAn operation is running.\nDo you really want to cancel it?");

                var result = await dialog.ShowAsync();
                if (result.Id.Equals(0))
                {
                    AbortButtonAction();
                    IsCancelRequest = true;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Opens a picker to select a folder and returns it. May be <code>null</code> on cancellation.
        /// </summary>
        internal async Task<StorageFolder> OutputPathPanelAction()
        {
            var picker = PickerFactory.CreateFolderPicker();

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null) // system has now access to folder
            {
                OutputFolder = folder;
            }
            return folder;
        }

        /// <summary>
        /// Initializes any operation by checking if an output folder was selected
        /// and also creates a new token for cancellation.
        /// </summary>
        /// <exception cref="NullReferenceException">Thrown if output folder is <code>null</code>.</exception>
        protected void InitOperation()
        {
            if (OutputFolder == null)
            {
                throw new NullReferenceException("No valid output folder selected.");
            }
            CancellationToken = new CancellationTokenSource();
            IsRunning = true;
        }

        /// <summary>
        /// Finalizes the currently active operation.
        /// </summary>
        protected void FinalizeOperation()
        {
            CancellationToken?.Dispose();
            IsRunning = false;
            IsCancelRequest = false;
        }

        /// <summary>
        /// Performs the archiving operation.
        /// </summary>
        /// <param name="archiveInfo">Consists of information about the archive.</param>
        /// <returns>True on success, false otherwise.</returns>
        protected abstract Task<Result> PerformOperation(ArchiveInfo archiveInfo);
    }
}
