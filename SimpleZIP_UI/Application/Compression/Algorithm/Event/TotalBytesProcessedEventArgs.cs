using System;

namespace SimpleZIP_UI.Application.Compression.Algorithm.Event
{
    public class TotalBytesProcessedEventArgs : EventArgs
    {
        public long TotalBytesProcessed { get; set; }
    }
}
