using System;
using System.IO;

namespace SimpleZIP_UI.Exceptions
{
    internal class ReadingArchiveException : IOException
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
