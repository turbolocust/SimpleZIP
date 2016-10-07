using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.UI.Factory;

namespace SimpleZIP_UI.UI
{
    public abstract class BaseControl
    {
        /// <summary>
        /// 
        /// </summary>
        protected Page ParentPage { get; }

        /// <summary>
        /// 
        /// </summary>
        protected bool IsCancelRequest = false;

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
            AlgorithmFileTypes.Add(".gz", Algorithm.Gzip);
            AlgorithmFileTypes.Add(".tgz", Algorithm.TarGz);
            AlgorithmFileTypes.Add(".bz2", Algorithm.TarBz2);
            AlgorithmFileTypes.Add(".tbz2", Algorithm.TarBz2);
        }

        protected BaseControl(Page parent)
        {
            ParentPage = parent;
        }

        /// <summary>
        /// 
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
                    CancellationToken?.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    IsCancelRequest = true;
                }
                finally
                {
                    ParentPage.Frame.Navigate(typeof(View.MainPage));
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
                OutputFolder = folder;
            }
            return folder;
        }
    }
}