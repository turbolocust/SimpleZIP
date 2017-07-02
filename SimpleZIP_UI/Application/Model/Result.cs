using System;

namespace SimpleZIP_UI.Application.Model
{
    internal class Result
    {
        /// <summary>
        /// Constructs a new instance of this class with status code set to <code>Status.Success</code>.
        /// </summary>
        internal Result()
        {
            StatusCode = Status.Success;
        }

        internal Status StatusCode { get; set; }

        internal string Message { get; set; }

        internal TimeSpan ElapsedTime { get; set; }

        public enum Status
        {
            Fail, Success, Interrupt
        }
    }
}
