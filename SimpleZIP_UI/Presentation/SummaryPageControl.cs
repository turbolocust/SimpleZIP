using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Compression;
using SimpleZIP_UI.Application.Model;
using SimpleZIP_UI.Presentation.Factory;

namespace SimpleZIP_UI.Presentation
{
    internal abstract class SummaryPageControl : BaseControl, IDisposable
    {

        /// <summary>
        /// True if a cancel request has been made.
        /// </summary>
        public bool IsCancelRequest { get; protected set; }

        /// <summary>
        /// Reference to the currently active archiving operation.
        /// </summary>
        public ArchivingOperation Operation;

        /// <summary>
        /// Where the archive or its content will be saved to.
        /// </summary>
        protected StorageFolder OutputFolder;

        internal SummaryPageControl(Page parent) : base(parent)
        {
        }

        /// <summary>
        /// Performs an action after the start button has been tapped.
        /// </summary>
        /// <param name="archiveInfo">Consists of information about the archive.</param>
        /// <returns>True on success, false otherwise.</returns>
        internal async Task<Result> StartButtonAction(ArchiveInfo archiveInfo)
        {
            InitOperation(archiveInfo);
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
                if (!Operation.IsRunning)
                {
                    NavigateBackHome();
                }
                else
                {
                    IsCancelRequest = true;
                    Operation.Cancel();
                }
            }
            catch (Exception)
            {
                NavigateBackHome();
            }
        }

        /// <summary>
        /// Opens a picker to select a folder and returns it. 
        /// May be <code>null</code> on cancellation.
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
        /// Returns the name of a folder picked via folder picker.
        /// </summary>
        /// <returns>The name of the picked folder or 
        /// <code>string.Empty</code> if no folder was picked.</returns>
        internal async Task<string> PickOutputPath()
        {
            var folder = await OutputPathPanelAction();
            return folder?.Name ?? "";
        }

        /// <summary>
        /// Initializes a new operation.
        /// </summary>
        /// <param name="archiveInfo">Consists of information about the archive.</param>
        /// <exception cref="NullReferenceException">Thrown if output folder is <code>null</code>.</exception>
        protected void InitOperation(ArchiveInfo archiveInfo)
        {
            if (OutputFolder == null)
            {
                throw new NullReferenceException("No valid output folder selected.");
            }
            archiveInfo.OutputFolder = OutputFolder;
            Operation = new ArchivingOperation();
        }

        /// <summary>
        /// Finalizes the currently active operation.
        /// </summary>
        protected void FinalizeOperation()
        {
            IsCancelRequest = false;
            Operation.Dispose();
        }

        /// <summary>
        /// Performs the archiving operation.
        /// </summary>
        /// <param name="archiveInfo">Consists of information about the archive.</param>
        /// <returns>True on success, false otherwise.</returns>
        protected abstract Task<Result> PerformOperation(ArchiveInfo archiveInfo);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Operation?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
