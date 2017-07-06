﻿using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Compression;
using SimpleZIP_UI.Application.Model;
using SimpleZIP_UI.Presentation.Factory;
using System.Collections.Generic;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Presentation
{
    internal abstract class SummaryPageControl : BaseControl, IDisposable
    {
        /// <summary>
        /// Specifies the threshold for the total file size 
        /// after which a warning message will be displayed.
        /// </summary>
        private const ulong FileSizeWarningThreshold = 20971520; // 20 megabytes

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
        /// Performs an action when the start button has been tapped.
        /// </summary>
        /// <param name="archiveInfo">Consists of information about the archive.</param>
        /// <returns>True on success, false otherwise.</returns>
        internal async Task<Result> StartButtonAction(ArchiveInfo archiveInfo)
        {
            using (Operation = new ArchivingOperation())
            {
                Result result;
                try
                {
                    InitOperation(archiveInfo);
                    result = await PerformOperation(archiveInfo);
                    FinalizeOperation();
                }
                catch (Exception ex)
                {
                    result = new Result
                    {
                        Message = ex.Message,
                        StatusCode = Result.Status.Fail
                    };
                }
                return result;
            }
        }

        /// <summary>
        /// Performs an action when the abort button has been pressed.
        /// </summary>
        internal void AbortButtonAction()
        {
            try
            {
                if (Operation == null || !Operation.IsRunning)
                {
                    NavigateBackHome();
                }
                else
                {
                    IsCancelRequest = true;
                    Operation.Cancel();
                }
            }
            catch (ObjectDisposedException)
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
        /// Checks the total size of all the files in the specified list and
        /// displays a toast notification if a threshold has been passed.
        /// </summary>
        /// <param name="files"></param>
        protected async void CheckFileSizes(IReadOnlyList<StorageFile> files)
        {
            var totalSize = await FileUtils.GetFileSizesAsync(files);
            if (totalSize >= FileSizeWarningThreshold)
            {
                ShowToastNotification("Please be patient", "This might take a while. . .");
            }
        }

        /// <summary>
        /// Performs various tasks before the start of the archiving operation.
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
        }

        /// <summary>
        /// Performs various tasks after the archiving operation has finished.
        /// </summary>
        protected void FinalizeOperation()
        {
            IsCancelRequest = false;
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
