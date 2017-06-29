using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Common.Model;
using SimpleZIP_UI.UI.Factory;
using SimpleZIP_UI.UI.View;

namespace SimpleZIP_UI.UI
{
    internal abstract class BaseControl
    {
        /// <summary>
        /// The parent page to whom this control belongs to.
        /// </summary>
        protected Page ParentPage { get; }

        /// <summary>
        /// True if a cancel request has been made.
        /// </summary>
        protected bool IsCancelRequest;

        /// <summary>
        /// Token used to cancel an archiving operation.
        /// </summary>
        protected CancellationTokenSource CancellationToken;

        /// <summary>
        /// Where the archive or its content will be saved to.
        /// </summary>
        protected StorageFolder OutputFolder;

        /// <summary>
        /// Enumeration type to identify algorithms.
        /// </summary>
        public enum Algorithm
        {
            Zip, GZip, SevenZip, Rar, TarGz, TarBz2
        }

        /// <summary>
        /// Stores the file type for each algorithm type.
        /// </summary>
        internal static readonly Dictionary<string, Algorithm> AlgorithmFileTypes = new Dictionary<string, Algorithm>();

        static BaseControl()
        {
            AlgorithmFileTypes.Add(".zip", Algorithm.Zip);
            AlgorithmFileTypes.Add(".7z", Algorithm.SevenZip);
            AlgorithmFileTypes.Add(".rar", Algorithm.Rar);
            AlgorithmFileTypes.Add(".z", Algorithm.GZip);
            AlgorithmFileTypes.Add(".gzip", Algorithm.GZip);
            AlgorithmFileTypes.Add(".gz", Algorithm.TarGz);
            AlgorithmFileTypes.Add(".tgz", Algorithm.TarGz);
            AlgorithmFileTypes.Add(".bz2", Algorithm.TarBz2);
            AlgorithmFileTypes.Add(".tbz2", Algorithm.TarBz2);
        }

        protected BaseControl(Page parent)
        {
            ParentPage = parent;
            CancellationToken = null;
        }

        /// <summary>
        /// Initializes any operation by checking if an output folder was selected
        /// and also creates a new token for cancellation.
        /// </summary>
        protected void InitOperation()
        {
            if (OutputFolder == null)
            {
                throw new NullReferenceException("No valid output folder selected.");
            }
            CancellationToken?.Dispose();
            CancellationToken = new CancellationTokenSource();
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
                    var durationText = BuildDurationText(result.ElapsedTime);
                    dialog = DialogFactory.CreateInformationDialog(
                        "Success", durationText);
                    break;
                case Result.Status.Fail:
                    dialog = DialogFactory.CreateErrorDialog(result.Message);
                    break;
                case Result.Status.Interrupt:
                    dialog = DialogFactory.CreateInformationDialog("Interrupted",
                        "Operation has been canceled.");
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
            return dialog;
        }

        /// <summary>
        /// Performs an action after the abort button has been pressed.
        /// </summary>
        internal void AbortButtonAction()
        {
            try
            {
                CancellationToken?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                IsCancelRequest = true;
            }
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
        /// Builds the text that shows the total duration converted into minutes.
        /// </summary>
        /// <param name="timeSpan">The duration as time span.</param>
        /// <returns>A friendly string that shows the total duration in minutes.
        /// If the duration is less than one second it will not contain a number.</returns>
        private static string BuildDurationText(TimeSpan timeSpan)
        {
            var durationText = new StringBuilder("Total duration: ");

            if (timeSpan.Seconds < 1)
            {
                durationText.Append("Less than one second.");
            }
            else
            {
                durationText.Append(timeSpan.ToString(@"hh\:mm\:ss"));
                if (timeSpan.Minutes < 1)
                {
                    durationText.Append(" seconds.");
                }
                else if (timeSpan.Hours < 1)
                {
                    durationText.Append(" minutes.");
                }
                else
                {
                    durationText.Append(" hours.");
                }
            }
            return durationText.ToString();
        }
    }
}