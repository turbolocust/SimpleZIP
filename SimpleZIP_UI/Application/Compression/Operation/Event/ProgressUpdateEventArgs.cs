using System;

namespace SimpleZIP_UI.Application.Compression.Operation.Event
{
    public class ProgressUpdateEventArgs : EventArgs
    {
        public double Progress { get; set; }
    }
}
