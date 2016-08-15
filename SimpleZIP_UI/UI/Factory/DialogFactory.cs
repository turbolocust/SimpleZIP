using Windows.UI.Popups;

namespace SimpleZIP_UI.UI.Factory
{
    internal class DialogFactory
    {
        private DialogFactory()
        {
            // currently holds static members only
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static MessageDialog CreateInformationDialog(string title, string message)
        {
            return new MessageDialog(message, title);
        }
    }
}
