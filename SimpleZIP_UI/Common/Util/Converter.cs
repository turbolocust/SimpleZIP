using System;

namespace SimpleZIP_UI.Common.Util
{
    internal class Converter
    {
        /// <summary>
        /// Converts milliseconds to seconds and rounds for two fraction digits.
        /// </summary>
        /// <param name="millis">The milliseconds to convert to seconds.</param>
        /// <returns>The converted and rounded value in seconds.</returns>
        public static double ConvertMillisecondsToSeconds(double millis)
        {
            return Math.Round((Math.Pow(millis, -6)), 2);
        }
    }
}
