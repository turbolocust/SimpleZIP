using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Common.Util;
using SimpleZIP_UI.UI.Factory;

namespace SimpleZIP_UI.UI.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
        /// <param name="e">The event that invoked this method.</param>
        private void AbortButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            _control.AbortButtonAction();
            SetOperationActive(false);
        }

        /// <summary>
        /// Invoked when the start button has been tapped.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
        /// <exception cref="ArgumentOutOfRangeException">May only be thrown on fatal error.</exception>
        private async void StartButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            SetOperationActive(true);
            var result = await _control.StartButtonAction(_selectedFiles);
            var duration = result.ElapsedTime;

            // move focus to avoid accidential focus event on text block
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next);

            if (result.StatusCode >= 0) // success
            {
                duration = Converter.ConvertMillisecondsToSeconds(duration, 3);
                var durationText = "Total duration: ";

                if (duration < 1)
                {
                    durationText += "Less than one second.";
                }
                else
                {
                    durationText += duration + " seconds.";
                }
                await
                    DialogFactory.CreateInformationDialog("Success", durationText)
                        .ShowAsync();
            }
            else // error
            {
                await DialogFactory.CreateErrorDialog(result.Message).ShowAsync();
            }

            SetOperationActive(false);
            Frame.Navigate(typeof(MainPage));
        }

        /// <summary>
        /// Invoked when the panel containing the output path has been tapped.
        /// Lets the user then pick an output folder for the archive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">The event that invoked this method.</param>
        private void OutputPathPanel_Tap(object sender, TappedRoutedEventArgs e)
        {
            PickOutputPath();
        }

        /// <summary>
        /// Invoked when output path text block got focus.
        /// Lets the user then pick an output folder for the archive.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="e">The event that invoked this method.</param>
        private void OutputPathTextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!ProgressRing.IsActive)
            {
                PickOutputPath();
                FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
            }
        }

        /// <summary>
        /// Invoked after navigating to this page.
        /// </summary>
        /// <param name="e">The event that invoked this method.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _selectedFiles = e.Parameter as IReadOnlyList<StorageFile>;

            if (_selectedFiles != null)
            {
                foreach (var f in _selectedFiles) // populate list
                {
                    ItemsListBox.Items?.Add(new TextBlock { Text = f.Name });
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
            OutputPathTextBlock.Text = folder?.Name ?? "";
            StartButton.IsEnabled = OutputPathTextBlock.Text.Length > 0;
        }

        /// <summary>
        /// Sets the archiving operation as active. This means that the UI is in busy state.
        /// </summary>
        /// <param name="isActive">True to set operation as active.</param>
        private void SetOperationActive(bool isActive)
        {
            if (isActive)
            {
                ProgressRing.IsActive = true;
                ProgressRing.Visibility = Visibility.Visible;
                StartButton.IsEnabled = false;
                OutputPathTextBlock.IsTapEnabled = false;
            }
            else
            {
                ProgressRing.IsActive = false;
                ProgressRing.Visibility = Visibility.Collapsed;
                StartButton.IsEnabled = true;
                OutputPathTextBlock.IsTapEnabled = true;
            }
        }
    }
}
