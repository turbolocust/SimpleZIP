using Windows.UI.Popups;

namespace SimpleZIP_UI.Presentation.Factory
{
    internal class DialogFactory
    {
        private DialogFactory()
        {
            // holds static members only
        }

        /// <summary>
        /// Creates a new message dialog with two buttons for confirmation.
        /// One labeled "Yes" with index 0. The other labeled "No" with index 1.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message to be displayed by the dialog.</param>
        /// <returns>The newly created message dialog.</returns>
        public static MessageDialog CreateConfirmationDialog(string title, string message)
        {
            var dialog = new MessageDialog(message, title);
            dialog.Commands.Add(new UICommand("Yes") { Id = 0 });
            dialog.Commands.Add(new UICommand("No") { Id = 1 });
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            return dialog;
        }

        /// <summary>
        /// Creates a new message dialog titled "Error" that displays the specified message.
        /// </summary>
        /// <param name="message">The message to be displayed by the dialog.</param>
        /// <returns>The newly created message dialog.</returns>
        public static MessageDialog CreateErrorDialog(string message)
        {
            return new MessageDialog(message, "Error");
        }

        /// <summary>
        /// Creates a new message dialog that displays the specified title and message.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message to be displayed by the dialog.</param>
        /// <returns>The newly created message dialog.</returns>
        public static MessageDialog CreateInformationDialog(string title, string message)
        {
            return new MessageDialog(message, title);
        }
    }
}
