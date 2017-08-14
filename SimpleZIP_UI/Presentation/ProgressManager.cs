using System.Threading;

namespace SimpleZIP_UI.Presentation
{
    internal sealed class ProgressManager
    {
        /// <summary>
        /// Placeholder value which can be set if current progress value
        /// has been received. Works like a lock and can be used to e.g. 
        /// avoid flooding of the UI thread.
        /// </summary>
        internal const double Sentinel = -1d;

        private double _progressValue;

        internal ProgressManager()
        {
            _progressValue = Sentinel;

        }

        /// <summary>
        /// Returns the current progress value and sets the specified value 
        /// as the current progress value. The specified value is set atomically.
        /// </summary>
        /// <param name="newValue">The value to be exchanged.</param>
        /// <returns>The previous assigned value.</returns>
        internal double Exchange(double newValue)
        {
            return Interlocked.Exchange(ref _progressValue, newValue);
        }
    }
}
