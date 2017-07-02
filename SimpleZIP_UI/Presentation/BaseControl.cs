using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Application.Model;
using SimpleZIP_UI.Presentation.Factory;
using SimpleZIP_UI.Presentation.View;

namespace SimpleZIP_UI.Presentation
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
            Zip, GZip, SevenZip, Rar, Tar, TarGz, TarBz2
        }

        /// <summary>
        /// Maps file types for each algorithm. Consists only of file types with a single file name extension.
        /// </summary>
        internal static readonly Dictionary<string, Algorithm> AlgorithmFileTypes;

        /// <summary>
        /// Maps file types for each algorithm. Consists only of file types with multiple file name extensions.
        /// </summary>
        internal static readonly Dictionary<string, Algorithm> AlgorithmExtendedFileTypes;

        static BaseControl()
        {
            // populate dictionary that maps file types to algorithms
            AlgorithmFileTypes = new Dictionary<string, Algorithm>(Enum.GetNames(typeof(Algorithm)).Length * 2)
            {
                {".zip", Algorithm.Zip},
                {".tar", Algorithm.Tar},
                {".gzip", Algorithm.GZip},
                {".gz", Algorithm.GZip},
                {".tgz", Algorithm.TarGz},
                {".bz2", Algorithm.TarBz2},
                {".tbz2", Algorithm.TarBz2}
            };

            // populate dictionary that maps extended file types to algorithms
            AlgorithmExtendedFileTypes = new Dictionary<string, Algorithm>
            {
                { ".tar.gz", Algorithm.TarGz },
                { ".tar.bz2", Algorithm.TarBz2 }
            };
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