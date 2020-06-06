// ==++==
// 
// Copyright (C) 2018 Matthias Fussenegger
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

using SimpleZIP_UI.Application;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Operation;
using SimpleZIP_UI.Application.Compression.Operation.Event;
using SimpleZIP_UI.Application.Progress;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Factory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Popups;
using Serilog;

namespace SimpleZIP_UI.Presentation.Controller
{
    internal abstract class SummaryPageController<T> : BaseController,
        ICancelRequest, IDisposable where T : OperationInfo
    {
        private readonly ILogger _logger = Log.ForContext<SummaryPageController<T>>();

        /// <summary>
        /// Specifies the threshold for the total file size 
        /// after which a notification will be displayed.
        /// </summary>
        private const ulong FileSizeWarningThreshold = 1024 * 1024 * 100;

        /// <summary>
        /// Avoids exceptions if file/folder picker is already open.
        /// </summary>
        private bool _pickerTriggered;

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

        /// <inheritdoc />
        public bool IsCancelRequest { get; protected set; }

        /// <inheritdoc />
        internal SummaryPageController(INavigation navHandler,
            IPasswordRequest pwRequest) : base(navHandler, pwRequest)
        {
            DisplayRequest = new DisplayRequest();
            ProgressManager = ProgressManagers.CreateInexact();
        }

        /// <inheritdoc />
        public void Reset()
        {
            IsCancelRequest = false;
        }

        #region Private methos

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
        /// Builds the text that shows the total duration converted into minutes.
        /// </summary>
        /// <param name="timeSpan">The duration as time span.</param>
        /// <returns>A friendly string that shows the total duration in minutes.
        /// If the duration is less than one second it will not contain a number.</returns>
        private static string BuildDurationText(TimeSpan timeSpan)
        {
            var durationText = new StringBuilder(I18N.Resources.GetString("TotalDuration/Text"));
            durationText.Append(": ");

            if (timeSpan.Seconds < 1)
            {
                durationText.Append(I18N.Resources.GetString("LessThanSecond/Text"));
            }
            else
            {
                const string format = @"hh\:mm\:ss";
                durationText.Append(timeSpan.ToString(format,
                    CultureInfo.CurrentCulture)).Append(" ");

                if (timeSpan.Minutes < 1)
                {
                    durationText.Append(I18N.Resources.GetString("seconds/Text"));
                }
                else if (timeSpan.Hours < 1)
                {
                    durationText.Append(I18N.Resources.GetString("minutes/Text"));
                }
                else
                {
                    durationText.Append(I18N.Resources.GetString("hours/Text"));
                }
                durationText.Append(".");
            }

            return durationText.ToString();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Performs an action when e.g. the start button has been tapped.
        /// </summary>
        /// <param name="listener">Listener to be attached to the operation's 
        /// <see cref="ArchivingOperation{T}.ProgressUpdate"/> event.</param>
        /// <param name="operationInfos">The amount of operations to be performed.</param>
        /// <returns>True on success, false otherwise.</returns>
        internal async Task<Result> StartAction(EventHandler<ProgressUpdateEventArgs> listener, params T[] operationInfos)
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
                    _logger.Error(ex, "Operation failed.");

                    result = new Result
                    {
                        Message = ex.Message,
                        VerboseMessage = ex.ToString(),
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
        /// Performs an action when e.g. the abort button has been pressed.
        /// </summary>
        internal void AbortAction()
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
                _logger.Warning("Operation already disposed.");
                NavigateBackHome();
            }
        }

        /// <summary>
        /// Opens a picker to select a folder and returns it. It may be 
        /// <c>null</c> on cancellation or if picker is already showing.
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
                _logger.Warning("No folder selected.");
                folder = null;
            }
            finally
            {
                _pickerTriggered = false;
            }

            if (folder != null)
                OutputFolder = folder;

            return folder;
        }

        /// <summary>
        /// Returns the name of a folder picked via a folder picker.
        /// </summary>
        /// <returns>The name of the picked folder or <see cref="string.Empty"/> if none was picked.</returns>
        internal async Task<string> PickOutputPath()
        {
            var folder = await OutputPathPanelAction();
            return folder?.Name ?? string.Empty;
        }

        /// <summary>
        /// Evaluates the specified result and shows a dialog depending on the status.
        /// </summary>
        /// <param name="result">The result to be evaluated.</param>
        /// <returns>True on successful evaluation, false otherwise.</returns>
        internal MessageDialog CreateResultDialog(Result result)
        {
            MessageDialog dialog;

            switch (result.StatusCode)
            {
                case Result.Status.Success:
                    {
                        string title = I18N.Resources.GetString("Success/Text");
                        string durationText = BuildDurationText(result.ElapsedTime);
                        dialog = DialogFactory.CreateInformationDialog(title, durationText);
                        break;
                    }
                case Result.Status.Fail:
                    {
                        string message = !string.IsNullOrEmpty(result.Message)
                            ? result.Message
                            : I18N.Resources.GetString("SomethingWentWrong/Text");
                        dialog = DialogFactory.CreateErrorDialog(message);
                        break;
                    }
                case Result.Status.Interrupt:
                    {
                        string title = I18N.Resources.GetString("Interrupted/Text");
                        string message = I18N.Resources.GetString("OperationCancelled/Text");
                        dialog = DialogFactory.CreateInformationDialog(title, message);
                        break;
                    }
                case Result.Status.PartialFail:
                    {
                        string resultMessage = result.Message;
                        if (string.IsNullOrEmpty(resultMessage))
                        {
                            resultMessage = I18N.Resources.GetString("NotAllProcessed/Text");
                        }

                        dialog = DialogFactory.CreateErrorDialog(resultMessage);
                        break;
                    }
                // default case should never be the case (assertion error)
                default: throw new ArgumentOutOfRangeException(nameof(result));
            }

            return dialog;
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
        /// Checks the output folder and optionally shows an information dialog
        /// if the output folder is not set and thus <c>null</c>.
        /// </summary>
        /// <param name="showDialogIfInvalid">True to show an information dialog
        /// if the output folder is not set.</param>
        /// <returns>True if output folder is set.</returns>
        internal async Task<bool> CheckOutputFolder(bool showDialogIfInvalid = true)
        {
            if (OutputFolder != null)
            {
                return true;
            }
            if (showDialogIfInvalid)
            {
                var dialog = DialogFactory.CreateInformationDialog(
                    I18N.Resources.GetString("OutputFolderMissing/Text"),
                    I18N.Resources.GetString("SelectOutputFolder/Text"));
                await dialog.ShowAsync();
            }
            return false;
        }

        #endregion

        #region Protected methods

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

        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
