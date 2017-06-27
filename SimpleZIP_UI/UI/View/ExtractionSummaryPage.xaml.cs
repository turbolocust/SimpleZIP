using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.UI.Factory;

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
        /// <param name="e">The event that invoked this method.</param>
        private void AbortButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            _control.AbortButtonAction();
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

            // move focus to avoid accidential focus event on text block
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next);

            if (result.StatusCode >= 0) // success
            {
                var durationText = _control.BuildDurationText(result.ElapsedTime);

                await DialogFactory.CreateInformationDialog(
                    "Success", durationText).ShowAsync();
            }
            else // error
            {
                await DialogFactory.CreateErrorDialog(result.Message).ShowAsync();
            }

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
            var list = e.Parameter as IReadOnlyList<StorageFile>;
            if (list != null)
            {
                _selectedFiles = list;
            }
            else // file opened from file explorer
            {
                var args = e.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;
                if (args?.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
                {
                    var fileArgs = args as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
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
        /// Invoked after navigating from this page.
        /// </summary>
        /// <param name="args">The arguments of the navigation event.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs args)
        {
            SetOperationActive(false);
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
