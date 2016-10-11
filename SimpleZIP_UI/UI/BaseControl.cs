using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Common.Util;
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
        /// Token used to cancel the packing task.
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
            Zip, SevenZip, GZip, TarGz, TarBz2
        }

        /// <summary>
        /// Stores the file type for each enum type.
        /// </summary>
        internal static readonly Dictionary<string, Algorithm> AlgorithmFileTypes = new Dictionary<string, Algorithm>();

        static BaseControl()
        {
            AlgorithmFileTypes.Add(".zip", Algorithm.Zip);
            AlgorithmFileTypes.Add(".7z", Algorithm.SevenZip);
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
            CancellationToken = new CancellationTokenSource();
        }

        /// <summary>
        /// Handles the action that needs to be performed after the abort button has been pressed.
        /// </summary>
        internal async void AbortButtonAction()
        {
            var dialog = DialogFactory.CreateConfirmationDialog("Are you sure?",
                "This will cancel the operation.");

            var result = await dialog.ShowAsync();
            if (result.Id.Equals(0)) // cancel operation
            {
                try
                {
                    CancellationToken?.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    IsCancelRequest = true;
                }
                finally
                {
                    NavigateBackToHome();
                }
            }
        }

        /// <summary>
        /// Opens a picker to select a folder and returns it. May be null if the user cancels.
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
        /// Builds the text that shows the total duration converted in seconds.
        /// </summary>
        /// <param name="durationMillis">The duration in milliseconds.</param>
        /// <returns>A friendly string that shows the total duration in seconds.
        /// If the duration is less than one second it will not contain a number.</returns>
        internal string BuildDurationText(double durationMillis)
        {
            var durationSecs = Converter.ConvertMillisToSeconds(durationMillis, 3);
            var durationText = "Total duration: ";

            if (durationSecs < 1)
            {
                durationText += "Less than one second.";
            }
            else
            {
                durationText += durationSecs + " seconds.";
            }
            return durationText;
        }

        /// <summary>
        /// Navigates back to the main page.
        /// </summary>
        protected void NavigateBackToHome()
        {
            ParentPage.Frame.Navigate(typeof(MainPage));
        }
    }
}