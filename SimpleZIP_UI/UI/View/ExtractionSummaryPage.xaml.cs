using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Common.Model;

namespace SimpleZIP_UI.UI.View
{
    public sealed partial class ExtractionSummaryPage
    {
        private readonly ExtractionSummaryPageControl _control;

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
        /// <param name="args">Arguments that may have been passed.</param>
        private void AbortButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            _control.AbortButtonAction();
        }

        /// <summary>
        /// Invoked when the start button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        private async void StartButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await InitOperation();
            Frame.Navigate(typeof(MainPage));
        }

        /// <summary>
        /// Invoked when the panel that holds the output path has been tapped.
        /// As a result, the user can pick an output folder for the archive.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private async void OutputPathPanel_Tap(object sender, TappedRoutedEventArgs args)
        {
            await PickOutputPath();
        }

        /// <summary>
        /// Invoked when output path text block got focus.
        /// As a result, the user can pick an output folder for the archive.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private async void OutputPathTextBlock_GotFocus(object sender, RoutedEventArgs args)
        {
            if (!ProgressRing.IsActive)
            {
                await PickOutputPath();
                FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
            }
        }

        /// <summary>
        /// Invoked after navigating to this page.
        /// </summary>
        /// <param name="args">Arguments that may have been passed.</param>
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

        /// <summary>
        /// Initializes the archiving operation and waits for the result.
        /// </summary>
        private async Task<bool> InitOperation()
        {
            SetOperationActive(true);
            var archiveInfo = new ArchiveInfo(_selectedFiles, ArchiveInfo.CompressionMode.Decompress);
            var result = await _control.StartButtonAction(archiveInfo);

            // move focus to avoid accidental focus event on text block
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next);

            await _control.CreateResultDialog(result).ShowAsync();
            return result.StatusCode == Result.Status.Success;
        }

        /// <summary>
        /// Delegates the action to pick an output folder.
        /// Shows the name of the output folder in the UI after successful selection.
        /// </summary>
        private async Task<bool> PickOutputPath()
        {
            var folder = await _control.OutputPathPanelAction();
            OutputPathTextBlock.Text = folder?.Name ?? "";
            StartButton.IsEnabled = OutputPathTextBlock.Text.Length > 0;
            return StartButton.IsEnabled;
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
                OutputPathTextBlock.IsEnabled = false;
            }
            else
            {
                ProgressRing.IsActive = false;
                ProgressRing.Visibility = Visibility.Collapsed;
                StartButton.IsEnabled = true;
                OutputPathTextBlock.IsEnabled = true;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            SetOperationActive(false);
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            e.Cancel = _control.IsRunning || _control.IsCancelRequest;
            await _control.ConfirmCancellationRequest();
        }
    }
}
