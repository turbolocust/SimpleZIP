using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using SimpleZIP_UI.UI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SimpleZIP_UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExtractionSummaryPage : Page
    {
        private readonly ExtractionSummaryPageControl _control;

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
            throw new NotImplementedException();
        }
        private void OutputPathPanel_Tap(object sender, TappedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OutputPathTextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
