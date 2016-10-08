using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.UI.Factory;
using SimpleZIP_UI.UI.View;

namespace SimpleZIP_UI.UI
{
    public abstract class BaseControl
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
            Zip, Gzip, TarGz, TarBz2
        }

        /// <summary>
        /// Stores the file type for each enum type.
        /// </summary>
        public static readonly Dictionary<string, Algorithm> AlgorithmFileTypes = new Dictionary<string, Algorithm>();

        static BaseControl()
        {
            AlgorithmFileTypes.Add(".zip", Algorithm.Zip);
            AlgorithmFileTypes.Add(".z", Algorithm.Gzip);
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
        public async void AbortButtonAction()
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
                    ParentPage.Frame.Navigate(typeof(MainPage));
                }
            }
        }

        /// <summary>
        /// Opens a picker to select a folder and returns it. May be null if the user cancels.
        /// </summary>
        public async Task<StorageFolder> OutputPathPanelAction()
        {
            var picker = PickerFactory.CreateFolderPicker();

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null) // system has now access to folder
            {
                OutputFolder = folder;
            }
            return folder;
        }
    }
}