using Windows.UI.Xaml.Controls;

namespace SimpleZIP_UI.Presentation.View
{
    public sealed partial class AboutDialog
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the primary button of this dialog has been pressed. This will simply hide the dialog.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">Arguments that have been passed.</param>
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            sender.Hide();
        }
    }
}
