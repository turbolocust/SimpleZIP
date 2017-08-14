using System;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Event
{
    public class TotalBytesReadEventArgs : EventArgs
    {
        public long TotalBytesRead { get; set; }
    }
}
