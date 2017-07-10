using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Control;

namespace SimpleZIP_UI.Presentation.View
{
    public sealed partial class DecompressionSummaryPage : Page, IDisposable
    {
        /// <summary>
        /// The aggregated control instance.
        /// </summary>
        private readonly DecompressionSummaryPageControl _control;

        /// <summary>
        /// A list of selected files for decompression.
        /// </summary>
        private IReadOnlyList<ExtractableItem> _selectedItems;

        public DecompressionSummaryPage()
        {
            InitializeComponent();
            _control = new DecompressionSummaryPageControl(this);
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
            var infos = new List<DecompressionInfo>(_selectedItems.Count);
            infos.AddRange(_selectedItems.Select(item => new DecompressionInfo(item)));
            return await _control.StartButtonAction(infos.ToArray());
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
            // check if file has been opened via file explorer
            var eventArgs = args.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;
            if (eventArgs?.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
            {
                var fileArgs = eventArgs as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
                var files = fileArgs?.Files;
                if (!files.IsNullOrEmpty())
                {
                    var itemList = new List<ExtractableItem>(files.Count);
                    itemList.AddRange(files.Select(file
                        => new ExtractableItem(file.Name, file as StorageFile)));
                    _selectedItems = itemList;
                }
            }
            // navigated from MainPage
            else if (args.Parameter is IReadOnlyList<ExtractableItem> items)
            {
                _selectedItems = items;
            }
            // populate list
            var stringBuilder = new StringBuilder();
            foreach (var item in _selectedItems)
            {
                stringBuilder.Append(item.DisplayName).Append(item.Archive.FileType);
                ItemsListBox.Items.Add(new TextBlock { Text = stringBuilder.ToString() });
                if (!item.Entries.IsNullOrEmpty()) // add entries with indent as well
                {
                    foreach (var entry in item.Entries)
                    {
                        stringBuilder.Clear();
                        stringBuilder.Append("   >> ").Append(entry.Name);
                        ItemsListBox.Items.Add(new TextBlock { Text = stringBuilder.ToString() });
                    }
                }
                stringBuilder.Clear();
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
            _control.Dispose();
        }
    }
}
