using System;

namespace SimpleZIP_UI.Exceptions
{
    internal class InvalidFileTypeException : Exception
    {
        public InvalidFileTypeException()
        {
        }

        public InvalidFileTypeException(string message) : base(message)
        {
        }

        public InvalidFileTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}