using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SimpleZIP_UI.UI.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExtractionSummaryPage : Page
    {
        private readonly ExtractionSummaryPageControl _control;

        private IReadOnlyList<StorageFile> _selectedFiles;

        public ExtractionSummaryPage()
        {
            this.InitializeComponent();
            _control = new ExtractionSummaryPageControl(this);
        }

        /// <summary>
        /// Triggered when the abort button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
        private void AbortButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            _control.AbortButtonAction();
            SetOperationActive(false);
        }

        /// <summary>
        /// Triggered when the start button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
        /// <exception cref="ArgumentOutOfRangeException">May only be thrown on fatal error.</exception>
        private void StartButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            SetOperationActive(true);
            //TODO
        }

        /// <summary>
        /// Triggered when the panel containing the output path has been tapped.
        /// Lets the user then pick an output folder for the archive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The event that invoked this method.</param>
        private void OutputPathPanel_Tap(object sender, TappedRoutedEventArgs e)
        {
            PickOutputPath();
        }

        /// <summary>
        /// Triggered when output path text block got focus.
        /// Lets the user then pick an output folder for the archive.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
        private void OutputPathTextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!this.ProgressRing.IsActive)
            {
                PickOutputPath();
                FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
            }
        }

        /// <summary>
        /// Triggered after navigating to this page.
        /// </summary>
        /// <param name="e">The event that invoked this method.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _selectedFiles = e.Parameter as IReadOnlyList<StorageFile>;

            if (_selectedFiles != null)
            {
                foreach (var f in _selectedFiles) // populate list
                {
                    this.ItemsListBox.Items?.Add(new TextBlock() { Text = f.Name });
                }
            }
        }

        /// <summary>
        /// Delegates the action to pick an output folder.
        /// Shows the name of the output folder in the UI after successful selection.
        /// </summary>
        private async void PickOutputPath()
        {
            var folder = await _control.OutputPathPanelAction();
            this.OutputPathTextBlock.Text = folder?.Name ?? "";
            this.StartButton.IsEnabled = this.OutputPathTextBlock.Text.Length > 0;
        }

        /// <summary>
        /// Sets the archiving operation as active. This means that the UI is in busy state.
        /// </summary>
        /// <param name="isActive">True to set operation as active.</param>
        private void SetOperationActive(bool isActive)
        {
            if (isActive)
            {
                this.ProgressRing.IsActive = true;
                this.ProgressRing.Visibility = Visibility.Visible;
                this.StartButton.IsEnabled = false;
                this.OutputPathTextBlock.IsTapEnabled = false;
            }
            else
            {
                this.ProgressRing.IsActive = false;
                this.ProgressRing.Visibility = Visibility.Collapsed;
                this.StartButton.IsEnabled = true;
                this.OutputPathTextBlock.IsTapEnabled = true;
            }
        }
    }
}
