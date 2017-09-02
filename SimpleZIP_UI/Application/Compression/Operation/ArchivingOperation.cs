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
        /// Associated algorithm instance.
        /// </summary>
        protected ICompressionAlgorithm Algorithm;

        /// <summary>
        /// Total amount of bytes to be processed.
        /// </summary>
        private ulong _totalBytesToProcess;

        /// <summary>
        /// Current progress in bytes. Used to calculate difference
        /// between previous amount of processed bytes and updated one.
        /// </summary>
        private long _previousBytesProcessed;

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
        /// The total amount of bytes already processed by this instance.
        /// This value can be reset, see <see cref="Perform"/>.
        /// </summary>
        public long TotalBytesProcessed { get; private set; }

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
        /// <param name="resetBytesProcessed">True to reset the current amount 
        /// of <see cref="TotalBytesProcessed"/>. Defaults to true.</param>
        /// <returns>A result object consisting of further details.</returns>
        public async Task<Result> Perform(T operationInfo, bool resetBytesProcessed = true)
        {
            IsRunning = true;
            SetAlgorithm(operationInfo);
            _totalBytesToProcess = operationInfo.TotalFileSize;
            try
            {
                Algorithm.TotalBytesProcessed += OnTotalBytesProcessed;
                return await StartOperation(operationInfo);
            }
            finally
            {
                IsRunning = false;
                Algorithm.TotalBytesProcessed -= OnTotalBytesProcessed;
                if (resetBytesProcessed) TotalBytesProcessed = 0L;
                _previousBytesProcessed = 0L;
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
        protected virtual void OnTotalBytesProcessed(object sender, TotalBytesProcessedEventArgs args)
        {
            // calculate difference to update total amount of bytes
            // that have already been processed by this instance
            var totalBytesProcessed = args.TotalBytesProcessed;
            TotalBytesProcessed += totalBytesProcessed - _previousBytesProcessed;
            _previousBytesProcessed = totalBytesProcessed;
            // actually calculate progress in percentage
            var progress = TotalBytesProcessed / (double)_totalBytesToProcess * 100;
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
