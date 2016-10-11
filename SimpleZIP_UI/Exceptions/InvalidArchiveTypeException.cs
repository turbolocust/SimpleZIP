using System;

namespace SimpleZIP_UI.Exceptions
{
    internal class InvalidArchiveTypeException : Exception
    {
        public InvalidArchiveTypeException()
        {
        }

        public InvalidArchiveTypeException(string message) : base(message)
        {
        }

        public InvalidArchiveTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}