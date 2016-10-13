using System;

namespace SimpleZIP_UI.Common.Util
{
    internal class Calculator
    {
        private Calculator()
        {
            // currently holds static members only
        }

        /// <summary>
        /// Converts milliseconds to seconds and rounds the specified decimal places.
        /// </summary>
        /// <param name="millis">The milliseconds to convert to seconds.</param>
        /// <param name="roundTo">The decimal places to round.</param>
        /// <returns>The converted and rounded value in seconds.</returns>
        public static double ConvertMillisToSeconds(double millis, int roundTo)
        {
            return Math.Round(millis * Math.Pow(10, -3), roundTo);
        }

        /// <summary>
        /// Calculates the elapsed time by substracting the start time from the time of the call.
        /// </summary>
        /// <param name="startTime">The time to substract.</param>
        /// <returns>The elapsed time.</returns>
        public static int CalculateElapsedTime(int startTime)
        {
            return DateTime.Now.Millisecond - startTime;
        }
    }
}
