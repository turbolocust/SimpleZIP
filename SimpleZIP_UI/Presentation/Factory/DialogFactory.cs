﻿// ==++==
// 
// Copyright (C) 2017 Matthias Fussenegger
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
using SimpleZIP_UI.Presentation.View.Dialog;
using Windows.UI.Popups;

namespace SimpleZIP_UI.Presentation.Factory
{
    internal static class DialogFactory
    {
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
            dialog.Commands.Add(new UICommand(I18N.Resources.GetString("Yes/Text")) { Id = 0 });
            dialog.Commands.Add(new UICommand(I18N.Resources.GetString("No/Text")) { Id = 1 });
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            return dialog;
        }

        /// <summary>
        /// Creates a new message dialog titled with the resource "SomethingWentWrong.Text".
        /// </summary>
        /// <param name="message">The message to be displayed by the dialog.</param>
        /// <returns>The newly created message dialog.</returns>
        public static MessageDialog CreateErrorDialog(string message)
        {
            string text = I18N.Resources.GetString("SomethingWentWrong/Text");
            return new MessageDialog(message, text);
        }

        /// <summary>
        /// Creates a new message dialog which displays the specified title and message.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message to be displayed by the dialog.</param>
        /// <returns>The newly created message dialog.</returns>
        public static MessageDialog CreateInformationDialog(string title, string message)
        {
            return new MessageDialog(message, title);
        }

        /// <summary>
        /// Creates a new dialog which allows the user to enter a password.
        /// </summary>
        /// <param name="fileName">The name of the password protected file.</param>
        /// <returns>The newly created dialog for entering a password.</returns>
        public static EnterPasswordDialog CreateRequestPasswordDialog(string fileName)
        {
            return new EnterPasswordDialog(fileName);
        }
    }
}
