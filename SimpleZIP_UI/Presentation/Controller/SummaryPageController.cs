// ==++==
// 
// Copyright (C) 2017 Matthias Fussenegger
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// ==--==
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Operation;
using SimpleZIP_UI.Application.Compression.Operation.Event;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Factory;

namespace SimpleZIP_UI.Presentation.Controller
{
    internal abstract class SummaryPageController<T> : BaseController, IDisposable where T : OperationInfo
    {
        /// <summary>
        /// Specifies the threshold for the total file size 
        /// after which a notification will be displayed.
        /// </summary>
        private const ulong FileSizeWarningThreshold = 1024 * 1024 * 25;

        /// <summary>
        /// Avoids exceptions if file/folder picker is already open.
        /// </summary>
        private bool _pickerTriggered;

        /// <summary>
        /// True if a cancel request has been made.
        /// </summary>
        internal bool IsCancelRequest { get; set; }

        /// <summary>
        /// Where the archive or its content will be saved to.
        /// </summary>
        internal StorageFolder OutputFolder { get; set; }

        /// <summary>
        /// Reference to the currently active archiving operation.
        /// </summary>
        internal ArchivingOperation<T> Operation { get; set; }

        /// <summary>
        /// Manages the current progress of the operation.
        /// </summary>
        internal ProgressManager<int> ProgressManager { get; }

        internal SummaryPageController(Page parent) : base(parent)
        {
            DisplayRequest = new DisplayRequest();
            ProgressManager = ProgressManagers.CreateInexact();
        }

        /// <summary>
        /// Performs an action when the start button has been tapped.
        /// </summary>
        /// <param name="listener">Listener to be attached to the operation's 
        /// <see cref="ArchivingOperation{T}.ProgressUpdate"/> event.</param>
        /// <param name="operationInfos">The amount of operations to be performed.</param>
        /// <returns>True on success, false otherwise.</returns>
        internal async Task<Result> StartButtonAction(
            EventHandler<ProgressUpdateEventArgs> listener, params T[] operationInfos)
        {
            using (Operation = GetArchivingOperation())
            {
                Result result;
                var startTime = DateTime.Now;

                try
                {
                    Operation.ProgressUpdate += listener;
                    InitOperation(operationInfos);
                    result = await PerformOperation(operationInfos);
                }
                catch (Exception ex)
                {
                    result = new Result
                    {
                        Message = ex.Message,
                        StatusCode = Result.Status.Fail
                    };
                }
                finally
                {
                    FinishOperation();
                    Operation.ProgressUpdate -= listener;
                }

                result.ElapsedTime = DateTime.Now - startTime;
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
        /// Opens a picker to select a folder and returns it. May be 
        /// <code>null</code> on cancellation or if picker is already showing.
        /// </summary>
        internal async Task<StorageFolder> OutputPathPanelAction()
        {
            if (_pickerTriggered) return null;
            StorageFolder folder;
            try
            {
                _pickerTriggered = true;
                var picker = PickerFactory.FolderPicker;
                folder = await picker.PickSingleFolderAsync();
            }
            catch (Exception)
            {
                folder = null;
            }
            finally
            {
                _pickerTriggered = false;
            }

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
            return folder?.Name ?? string.Empty;
        }

        /// <summary>
        /// Checks the total size of the specified items and its entries and displays a toast 
        /// notification if a threshold has been passed (<see cref="FileSizeWarningThreshold"/>).
        /// </summary>
        /// <param name="items">The items whose sizes are to be checked.</param>
        /// <returns>Task which returns the total size of the files.</returns>
        internal async Task<ulong> CheckFileSizes(IReadOnlyList<ExtractableItem> items)
        {
            ulong totalSize = 0L;
            foreach (var item in items)
            {
                if (!item.Entries.IsNullOrEmpty())
                {
                    totalSize += item.Entries.Aggregate(totalSize,
                        (current, entry) => current + entry.Size);
                }
                else
                {
                    totalSize += await FileUtils.GetFileSizeAsync(item.Archive);
                }
            } 
            ValidateFileSizes(totalSize);
            return totalSize;
        }

        /// <summary>
        /// Checks the total size of the files in the specified list and displays a 
        /// toast notification if a threshold has been passed (<see cref="FileSizeWarningThreshold"/>).
        /// </summary>
        /// <param name="files">The files whose sizes are to be checked.</param>
        /// <returns>Task which returns the total size of the files.</returns>
        internal async Task<ulong> CheckFileSizes(IReadOnlyList<StorageFile> files)
        {
            var totalSize = await FileUtils.GetFileSizesAsync(files);
            ValidateFileSizes(totalSize);
            return totalSize;
        }

        /// <summary>
        /// Validates the specified size and displays a toast notification 
        /// if a threshold has been passed (<see cref="FileSizeWarningThreshold"/>).
        /// </summary>
        /// <param name="totalSize">The total size to be validated.</param>
        private void ValidateFileSizes(ulong totalSize)
        {
            if (totalSize >= FileSizeWarningThreshold)
            {
                ShowToastNotification(I18N.Resources.GetString("PleaseBePatient/Text"),
                    I18N.Resources.GetString("MightTakeWhile/Text"));
            }
        }

        /// <summary>
        /// Performs various tasks before the start of the archiving operation.
        /// </summary>
        /// <param name="operationInfos">The amount of operations to be performed.</param>
        /// <exception cref="NullReferenceException">Thrown if output folder is <code>null</code>.</exception>
        protected void InitOperation(params T[] operationInfos)
        {
            if (OutputFolder == null)
            {
                throw new NullReferenceException(
                    I18N.Resources.GetString("NoValidFolderSelected/Text"));
            }
            // set output folder to each operation info
            foreach (var operationInfo in operationInfos)
            {
                operationInfo.OutputFolder = OutputFolder;
            }
            // keep display alive while operation is in progress
            DisplayRequest.RequestActive();
        }

        /// <summary>
        /// Performs various tasks after the archiving operation has finished.
        /// </summary>
        protected void FinishOperation()
        {
            IsCancelRequest = false;
            ProgressManager.Reset();
            DisplayRequest.RequestRelease();
        }

        /// <summary>
        /// Returns a concrete instance of the archiving operation.
        /// </summary>
        /// <returns>A concrete instance of the archiving operation.</returns>
        protected abstract ArchivingOperation<T> GetArchivingOperation();

        /// <summary>
        /// Performs the archiving operation.
        /// </summary>
        /// <param name="operationInfos">The amount of operations to be performed.</param>
        /// <returns>True on success, false otherwise.</returns>
        protected abstract Task<Result> PerformOperation(T[] operationInfos);

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
