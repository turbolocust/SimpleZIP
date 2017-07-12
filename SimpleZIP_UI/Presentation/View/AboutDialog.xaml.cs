using Windows.UI.Xaml.Controls;

namespace SimpleZIP_UI.Presentation.View
{
    public sealed partial class AboutDialog
    {
        public AboutDialog()
        {
            InitializeComponent();
            // display following hard coded strings in UI
            DevelopedByRun.Text = I18N.Resources.GetString("DevelopedBy/Text") + " Matthias Fussenegger";
            LicenseRun.Text = I18N.Resources.GetString("License/Text", "GNU General Public License 3");
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
