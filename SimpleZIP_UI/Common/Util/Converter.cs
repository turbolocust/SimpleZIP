using System;

namespace SimpleZIP_UI.Common.Util
{
    internal class Converter
    {
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
    }
}
