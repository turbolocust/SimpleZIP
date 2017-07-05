using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Application.Compression;
using SimpleZIP_UI.Application.Model;
using SimpleZIP_UI.Application.Util;

namespace SimpleZIP_UI.Presentation.View
{
    public sealed partial class ExtractionSummaryPage : Page, IDisposable
    {
        /// <summary>
        /// The aggregated control instance.
        /// </summary>
        private readonly ExtractionSummaryPageControl _control;

        /// <summary>
        /// A list of selected files for decompression.
        /// </summary>
        private IReadOnlyList<StorageFile> _selectedFiles;

        public ExtractionSummaryPage()
        {
            InitializeComponent();
            _control = new ExtractionSummaryPageControl(this);
        }

        /// <summary>
        /// Invoked when the abort button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that have been passed.</param>
        private void AbortButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            AbortButtonToolTip.IsOpen = true;
            _control.AbortButtonAction();
        }

        /// <summary>
        /// Invoked when the start button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that have been passed.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        private async void StartButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            var result = await InitOperation();
            _control.CreateResultDialog(result).ShowAsync().AsTask().Forget();
            Frame.Navigate(typeof(MainPage));
        }

        /// <summary>
        /// Invoked when the button holding the output path has been tapped.
        /// As a result, the user can pick an output folder for the archive.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that have been passed.</param>
        private void OutputPathButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            SetOutputPath();
        }

        /// <summary>
        /// Sets the output path and enables the start button if output path is valid.
        /// </summary>
        private async void SetOutputPath()
        {
            if (ProgressRing.IsActive) return;
            var text = await _control.PickOutputPath();
            if (!string.IsNullOrEmpty(text))
            {
                OutputPathButton.Content = text;
                StartButton.IsEnabled = true;
            }
            else
            {
                StartButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Initializes the archiving operation and waits for the result.
        /// </summary>
        private async Task<Result> InitOperation()
        {
            SetOperationActive(true);
            var archiveInfo = new ArchiveInfo(OperationMode.Decompress)
            {
                SelectedFiles = _selectedFiles
            };
            return await _control.StartButtonAction(archiveInfo);
        }

        /// <summary>
        /// Sets the archiving operation as active. This results in the UI being busy.
        /// </summary>
        /// <param name="isActive">True to set operation as active, false to set it as inactive.</param>
        private void SetOperationActive(bool isActive)
        {
            if (isActive)
            {
                ProgressRing.IsActive = true;
                ProgressRing.Visibility = Visibility.Visible;
                StartButton.IsEnabled = false;
                OutputPathButton.IsEnabled = false;
            }
            else
            {
                ProgressRing.IsActive = false;
                ProgressRing.Visibility = Visibility.Collapsed;
                StartButton.IsEnabled = true;
                OutputPathButton.IsEnabled = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            var list = args.Parameter as IReadOnlyList<StorageFile>;
            if (list == null) // file opened from file explorer
            {
                var eventArgs = args.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;
                if (eventArgs?.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
                {
                    var fileArgs = eventArgs as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
                    var files = fileArgs?.Files;
                    if (files != null)
                    {
                        list = files.Where(file => file != null).Cast<StorageFile>().ToList();
                    }
                }
            }

            if (list != null)
            {
                _selectedFiles = list;
                foreach (var f in _selectedFiles) // populate list
                {
                    ItemsListBox.Items?.Add(new TextBlock { Text = f.Name });
                }
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            e.Cancel = _control.Operation?.IsRunning ?? false;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            SetOperationActive(false);
            AbortButtonToolTip.IsOpen = false;
        }

        public void Dispose()
        {
            _control?.Dispose();
        }
    }
}
