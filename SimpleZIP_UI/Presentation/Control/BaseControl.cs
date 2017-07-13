using System;
using System.Text;
using Windows.System.Display;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Presentation.Factory;
using SimpleZIP_UI.Presentation.View;

namespace SimpleZIP_UI.Presentation.Control
{
    internal abstract class BaseControl
    {
        /// <summary>
        /// The parent page to whom this control belongs to.
        /// </summary>
        protected Page ParentPage { get; }

        /// <summary>
        /// Display request which can be used to keep screen active.
        /// </summary>
        protected DisplayRequest DisplayRequest { get; set; }

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
                        dialog = DialogFactory.CreateInformationDialog(
                            I18N.Resources.GetString("Success/Text"), durationText);
                        break;
                    }
                case Result.Status.Fail:
                    {
                        var message = !string.IsNullOrEmpty(result.Message)
                            ? result.Message : I18N.Resources.GetString("SomethingWentWrong/Text");
                        dialog = DialogFactory.CreateErrorDialog(message);
                        break;
                    }
                case Result.Status.Interrupt:
                    {
                        dialog = DialogFactory.CreateInformationDialog(
                            I18N.Resources.GetString("Interrupted/Text"),
                            I18N.Resources.GetString("OperationCancelled/Text"));
                        break;
                    }
                case Result.Status.PartialFail:
                    {
                        dialog = DialogFactory.CreateErrorDialog(
                            I18N.Resources.GetString("NotAllExtracted/Text"));
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
            var durationText = new StringBuilder(I18N.Resources.GetString("TotalDuration/Text"));
            durationText.Append(": ");

            if (timeSpan.Seconds < 1)
            {
                durationText.Append(I18N.Resources.GetString("LessThanSecond/Text"));
            }
            else
            {
                durationText.Append(timeSpan.ToString(@"hh\:mm\:ss")).Append(" ");
                if (timeSpan.Minutes < 1)
                {
                    durationText.Append(I18N.Resources.GetString("seconds/Text"));
                }
                else if (timeSpan.Hours < 1)
                {
                    durationText.Append(I18N.Resources.GetString("minutes/Text"));
                }
                else
                {
                    durationText.Append(I18N.Resources.GetString("hours/Text"));
                }
                durationText.Append(".");
            }
            return durationText.ToString();
        }
    }
}