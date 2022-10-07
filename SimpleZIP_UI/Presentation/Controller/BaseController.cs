// ==++==
// 
// Copyright (C) 2020 Matthias Fussenegger
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

using SimpleZIP_UI.Business;
using SimpleZIP_UI.Presentation.View;
using System;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.System.Display;
using Windows.UI.Notifications;

namespace SimpleZIP_UI.Presentation.Controller
{
    /// <inheritdoc cref="IGuiController" />
    /// <summary>
    /// Base GUI controller. Other GUI controllers should derive from this one.
    /// </summary>
    internal abstract class BaseController : IGuiController
    {
        /// <inheritdoc />
        public INavigable Navigation { get; }

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

        /// <summary>
        /// Creates a new controller instance for views.
        /// </summary>
        /// <param name="navHandler">Instance used for navigation.</param>
        /// <param name="pwRequest">Instance used to request passwords.</param>
        protected BaseController(INavigable navHandler, IPasswordRequest pwRequest)
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
        /// Shows a toast notification that will disappear after the specified amount of seconds. 
        /// </summary>
        /// <param name="title">The title of the toast notification.</param>
        /// <param name="content">The content of the toast notification.</param>
        /// <param name="seconds">Timer in seconds after which toast will disappear. Defaults to <c>8</c>.</param>
        internal void ShowToastNotification(string title, string content, uint seconds = 8)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            var toastNodeList = toastXml.GetElementsByTagName("text");
            toastNodeList.Item(0)?.AppendChild(toastXml.CreateTextNode(title));
            toastNodeList.Item(1)?.AppendChild(toastXml.CreateTextNode(content));
            toastXml.SelectSingleNode("/toast");

            var toast = new ToastNotification(toastXml)
            {
                ExpirationTime = DateTime.Now.AddSeconds(seconds),
                ExpiresOnReboot = true,
                Tag = Guid.NewGuid().ToString()
            };

            toast.Dismissed += (sender, args) =>
            {
                ToastNotificationManager.History.Remove(toast.Tag);
            };

            notifier.Show(toast);
        }
    }
}