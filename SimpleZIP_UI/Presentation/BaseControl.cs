using System;
using System.Text;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Model;
using SimpleZIP_UI.Presentation.Factory;
using SimpleZIP_UI.Presentation.View;
using Windows.UI.Notifications;

namespace SimpleZIP_UI.Presentation
{
    internal abstract class BaseControl
    {
        /// <summary>
        /// The parent page to whom this control belongs to.
        /// </summary>
        protected Page ParentPage { get; }

        protected BaseControl(Page parent)
        {
            ParentPage = parent;
        }

        /// <summary>
        /// Navigates back to the main page.
        /// </summary>
        protected void NavigateBackHome()
        {
            ParentPage.Frame.Navigate(typeof(MainPage));
        }

        /// <summary>
        /// Evaluates the specified result and shows a dialog depending on the status.
        /// </summary>
        /// <param name="result">The result to be evaluated.</param>
        /// <returns>True on successful evaluation, false otherwise.</returns>
        internal MessageDialog CreateResultDialog(Result result)
        {
            MessageDialog dialog;
            switch (result.StatusCode)
            {
                case Result.Status.Success:
                    {
                        var durationText = BuildDurationText(result.ElapsedTime);
                        dialog = DialogFactory.CreateInformationDialog("Success", durationText);
                        break;
                    }
                case Result.Status.Fail:
                    {
                        dialog = DialogFactory.CreateErrorDialog(result.Message);
                        break;
                    }
                case Result.Status.Interrupt:
                    {
                        dialog = DialogFactory.CreateInformationDialog("Interrupted", "Operation has been canceled.");
                        break;
                    }
                default: throw new ArgumentOutOfRangeException(nameof(result.StatusCode));
            }
            return dialog;
        }

        /// <summary>
        /// Shows a toast notification that will disappear after the specified amount of seconds. 
        /// </summary>
        /// <param name="title">The title of the toast notification.</param>
        /// <param name="content">The content of the toast notification.</param>
        /// <param name="seconds">Timer in seconds after which toast will disappear. Defaults to <code>4</code>.</param>
        internal void ShowToastNotification(string title, string content, uint seconds = 4)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0)?.AppendChild(toastXml.CreateTextNode(title));
            toastNodeList.Item(1)?.AppendChild(toastXml.CreateTextNode(content));
            toastXml.SelectSingleNode("/toast");
            var audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");

            var toast = new ToastNotification(toastXml)
            {
                ExpirationTime = DateTime.Now.AddSeconds(seconds)
            };
            notifier.Show(toast);
        }

        /// <summary>
        /// Builds the text that shows the total duration converted into minutes.
        /// </summary>
        /// <param name="timeSpan">The duration as time span.</param>
        /// <returns>A friendly string that shows the total duration in minutes.
        /// If the duration is less than one second it will not contain a number.</returns>
        private static string BuildDurationText(TimeSpan timeSpan)
        {
            var durationText = new StringBuilder("Total duration: ");

            if (timeSpan.Seconds < 1)
            {
                durationText.Append("Less than one second.");
            }
            else
            {
                durationText.Append(timeSpan.ToString(@"hh\:mm\:ss"));
                if (timeSpan.Minutes < 1)
                {
                    durationText.Append(" seconds.");
                }
                else if (timeSpan.Hours < 1)
                {
                    durationText.Append(" minutes.");
                }
                else
                {
                    durationText.Append(" hours.");
                }
            }
            return durationText.ToString();
        }
    }
}