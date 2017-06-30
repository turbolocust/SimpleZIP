using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Common.Model;
using SimpleZIP_UI.UI.Factory;
using SimpleZIP_UI.UI.View;

namespace SimpleZIP_UI.UI
{
    internal abstract class BaseControl
    {
        /// <summary>
        /// The parent page to whom this control belongs to.
        /// </summary>
        protected Page ParentPage { get; }

        /// <summary>
        /// Enumeration type to identify algorithms.
        /// </summary>
        public enum Algorithm
        {
            Zip, GZip, SevenZip, Rar, TarGz, TarBz2
        }

        /// <summary>
        /// Stores the file type for each algorithm type.
        /// </summary>
        internal static readonly Dictionary<string, Algorithm> AlgorithmFileTypes = new Dictionary<string, Algorithm>();

        static BaseControl()
        {
            AlgorithmFileTypes.Add(".zip", Algorithm.Zip);
            AlgorithmFileTypes.Add(".7z", Algorithm.SevenZip);
            AlgorithmFileTypes.Add(".rar", Algorithm.Rar);
            AlgorithmFileTypes.Add(".z", Algorithm.GZip);
            AlgorithmFileTypes.Add(".gzip", Algorithm.GZip);
            AlgorithmFileTypes.Add(".gz", Algorithm.TarGz);
            AlgorithmFileTypes.Add(".tgz", Algorithm.TarGz);
            AlgorithmFileTypes.Add(".bz2", Algorithm.TarBz2);
            AlgorithmFileTypes.Add(".tbz2", Algorithm.TarBz2);
        }

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
                    var durationText = BuildDurationText(result.ElapsedTime);
                    dialog = DialogFactory.CreateInformationDialog(
                        "Success", durationText);
                    break;
                case Result.Status.Fail:
                    dialog = DialogFactory.CreateErrorDialog(result.Message);
                    break;
                case Result.Status.Interrupt:
                    dialog = DialogFactory.CreateInformationDialog("Interrupted",
                        "Operation has been canceled.");
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
            return dialog;
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