// ==++==
// 
// Copyright (C) 2017 Matthias Fussenegger
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// ==--==
using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleZIP_UI.Application.Compression.Algorithm;
using SimpleZIP_UI.Application.Compression.Algorithm.Event;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Operation.Event;

namespace SimpleZIP_UI.Application.Compression.Operation
{
    /// <summary>
    /// Delegate for progress updates.
    /// </summary>
    /// <param name="evtArgs">Consists of event parameters.</param>
    public delegate void ProgressUpdateEventHandler(ProgressUpdateEventArgs evtArgs);

    internal abstract class ArchivingOperation<T> : IDisposable where T : OperationInfo
    {
        /// <summary>
        /// Event handler for progress updates.
        /// </summary>
        public event EventHandler<ProgressUpdateEventArgs> ProgressUpdate;

        /// <summary>
        /// Source for cancellation token.
        /// </summary>
        protected readonly CancellationTokenSource TokenSource;

        /// <summary>
        /// Holds information about the current operation.
        /// </summary>
        protected OperationInfo OperationInfo;

        /// <summary>
        /// Associated algorithm instance.
        /// </summary>
        protected ICompressionAlgorithm Algorithm;

        /// <summary>
        /// True if an operation is running, false otherwise.
        /// </summary>
        private volatile bool _isRunning;

        /// <summary>
        /// Checks whether this operation is currently running or not.
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            protected set => _isRunning = value;
        }

        /// <summary>
        /// Total amount of bytes this instance has processed
        /// during the course of its existence.
        /// </summary>
        public long TotalBytesProcessed { get; protected set; }

        protected ArchivingOperation()
        {
            TokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Cancels this operation.
        /// </summary>
        public void Cancel()
        {
            TokenSource.Cancel();
        }

        /// <summary>
        /// Performs this operation.
        /// </summary>
        /// <param name="operationInfo">Information required for the operation.</param>
        /// <returns>A result object consisting of further details.</returns>
        public async Task<Result> Perform(T operationInfo)
        {
            IsRunning = true;
            OperationInfo = operationInfo;
            SetAlgorithm(operationInfo);
            try
            {
                Algorithm.TotalBytesProcessed += OnTotalBytesRead;
                return await StartOperation(operationInfo);
            }
            finally
            {
                IsRunning = false;
                Algorithm.TotalBytesProcessed -= OnTotalBytesRead;
            }
        }

        /// <summary>
        /// Sets the algorithm to be used based on the information in <see cref="OperationInfo"/>.
        /// </summary>
        /// <param name="info">Info about the operation. Explicit parameter 
        /// is used to avoid future programming errors.</param>
        protected abstract void SetAlgorithm(T info);

        /// <summary>
        /// Actually starts this operation.
        /// </summary>
        /// <param name="operationInfo">Information required for the operation.</param>
        /// <returns>A result object consisting of further details.</returns>
        protected abstract Task<Result> StartOperation(T operationInfo);

        /// <summary>
        /// Calculates the total progress by using the specified amount of bytes that 
        /// have already been processed and then delegates the progress to all listeners.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">Consists of event parameters.</param>
        protected virtual void OnTotalBytesRead(object sender, TotalBytesProcessedEventArgs args)
        {
            var processedBytes = TotalBytesProcessed += args.TotalBytesProcessed;
            var progress = processedBytes / (double)OperationInfo.TotalFileSize * 100;
            var evtArgs = new ProgressUpdateEventArgs
            {
                Progress = progress
            };
            ProgressUpdate?.Invoke(this, evtArgs);
        }

        /// <summary>
        /// Evaluates the operation and returns the result.
        /// </summary>
        /// <param name="message">The message to be evaluated.</param>
        /// <param name="isSuccess">True if operation was successful, false otherwise.</param>
        /// <returns>An object that consists of result parameters.</returns>
        /// <exception cref="OperationCanceledException">Thrown if operation has been canceled.</exception>
        protected Result EvaluateResult(string message, bool isSuccess)
        {
            if (TokenSource.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
            var status = isSuccess ? Result.Status.Success : Result.Status.Fail;
            return new Result { StatusCode = status, Message = message };
        }

        public void Dispose()
        {
            IsRunning = false;
            TokenSource.Dispose();
        }
    }
}
