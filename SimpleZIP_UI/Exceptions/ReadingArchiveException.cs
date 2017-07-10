using System;

namespace SimpleZIP_UI.Exceptions
{
    internal class ReadingArchiveException : Exception
    {
        public ReadingArchiveException()
        {
        }

        public ReadingArchiveException(string message) : base(message)
        {
        }

        public ReadingArchiveException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
