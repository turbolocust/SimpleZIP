﻿// ==++==
// 
// Copyright (C) 2019 Matthias Fussenegger
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// ==--==

using SimpleZIP_UI.Application;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Presentation.Factory;
using SimpleZIP_UI.Presentation.View;
using System;
using System.Text;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.System.Display;
using Windows.UI.Notifications;
using Windows.UI.Popups;

namespace SimpleZIP_UI.Presentation.Controller
{
    /// <inheritdoc cref="IGuiController" />
    /// <summary>
    /// Base GUI controller. Other GUI controllers should derive from this one.
    /// </summary>
    internal abstract class BaseController : IGuiController
    {
        /// <inheritdoc />
        public INavigation Navigation { get; }

        /// <inheritdoc />
        public IPasswordRequest PasswordRequest { get; }

        /// <summary>
        /// Display request which can be used to keep screen active.
        /// </summary>
        protected DisplayRequest DisplayRequest { get; set; }

        /// <summary>
        /// The aggregated <see cref="ShareOperation"/>.
        /// </summary>
        internal ShareOperation ShareOperation { private get; set; }

        protected BaseController(INavigation navHandler, IPasswordRequest pwRequest)
        {
            Navigation = navHandler;
            PasswordRequest = pwRequest;
        }

        /// <summary>
        /// Checks if this controller was created for a share target event.
        /// </summary>
        /// <returns>True if share target is active, false otherwise.</returns>
        internal bool IsShareTargetActivated()
        {
            return ShareOperation != null;
        }

        /// <summary>
        /// Navigates back to the main page considering <see cref="ShareOperation"/>.
        /// </summary>
        internal void NavigateBackHome()
        {
            if (ShareOperation != null)
            {
                ShareOperation.ReportCompleted();
            }
            else
            {
                Navigation.Navigate(typeof(HomePage));
            }
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
                            ? result.Message
                            : I18N.Resources.GetString("SomethingWentWrong/Text");
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
                        var message = new StringBuilder(I18N.Resources
                            .GetString("NotAllProcessed/Text"));
                        message.AppendLine();
                        message.AppendLine(result.Message);
                        dialog = DialogFactory.CreateErrorDialog(message.ToString());
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
        /// <param name="seconds">Timer in seconds after which toast will disappear. Defaults to <c>8</c>.</param>
        /// <param name="muteAudio">True to mute audio, false otherwise. Defaults to <c>false</c>.</param>
        internal void ShowToastNotification(string title, string content, uint seconds = 8, bool muteAudio = false)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0)?.AppendChild(toastXml.CreateTextNode(title));
            toastNodeList.Item(1)?.AppendChild(toastXml.CreateTextNode(content));
            toastXml.SelectSingleNode("/toast");
            var audio = toastXml.CreateElement("audio");
            audio.SetAttribute("src", "ms-winsoundevent:Notification.SMS");
            if (muteAudio)
            {
                audio.SetAttribute("silent", "true");
            }
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