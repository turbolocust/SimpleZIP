// ==++==
// 
// Copyright (C) 2020 Matthias Fussenegger
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

using SimpleZIP_UI.Application.Compression.Algorithm;
using SimpleZIP_UI.Application.Compression.Algorithm.Event;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Compression.Operation.Event;
using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleZIP_UI.Application.Compression.Algorithm.Factory;

namespace SimpleZIP_UI.Application.Compression.Operation
{
    internal abstract class ArchivingOperation<T> : IDisposable, ICancellable where T : OperationInfo
    {
        /// <summary>
        /// Total amount of bytes to be processed.
        /// </summary>
        private long _totalBytesToProcess;

        /// <summary>
        /// True if an operation is running, false otherwise.
        /// </summary>
        private volatile bool _isRunning;

        /// <summary>
        /// Source for cancellation token.
        /// </summary>
        protected readonly CancellationTokenSource TokenSource;

        /// <summary>
        /// Associated algorithm instance.
        /// </summary>
        protected ICompressionAlgorithm Algorithm { get; private set; }

        /// <summary>
        /// Event handler for progress updates.
        /// </summary>
        public event EventHandler<ProgressUpdateEventArgs> ProgressUpdate;

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

        /// <summary>
        /// Constructs a new instance of this class and initializes <see cref="TokenSource"/>.
        /// </summary>
        protected ArchivingOperation()
        {
            TokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Updates the operation state based on whether to reset the state of processed bytes or not.
        /// </summary>
        /// <remarks>
        /// This is required so that this instance may be re-used multiple times, while still considering
        /// the total amount of bytes to be processed, e.g. if multiple archives are to be processed as
        /// part of a bigger operation (the job).
        /// </remarks>
        /// <param name="resetBytesProcessed">True if to forget about processed bytes, false otherwise.</param>
        private void UpdateOperationState(bool resetBytesProcessed)
        {
            if (resetBytesProcessed) TotalBytesProcessed = 0L;
        }

        /// <inheritdoc />
        public void Cancel()
        {
            TokenSource.Cancel();
        }

        /// <summary>
        /// Performs the operation as specified in the <c>OperationInfo</c> object.
        /// </summary>
        /// <param name="operationInfo">Information required for the operation.</param>
        /// <param name="resetBytesProcessed">True to reset the current amount 
        /// of <see cref="TotalBytesProcessed"/>. Defaults to true.</param>
        /// <returns>A result object consisting of further details.</returns>
        public async Task<Result> Perform(T operationInfo, bool resetBytesProcessed = true)
        {
            const uint defaultUpdateDelayRate = 100;

            IsRunning = true;

            var options = new AlgorithmOptions(defaultUpdateDelayRate);
            Algorithm = await GetAlgorithmAsync(operationInfo, options);
            _totalBytesToProcess = (long)operationInfo.TotalFileSize;

            try
            {
                Algorithm.BytesProcessed += OnBytesProcessed;
                return await StartOperation(operationInfo).ConfigureAwait(false);
            }
            finally
            {
                IsRunning = false;
                Algorithm.BytesProcessed -= OnBytesProcessed;
                UpdateOperationState(resetBytesProcessed);
            }
        }

        /// <summary>
        /// Sets the algorithm to be used based on the information in <see cref="OperationInfo"/>.
        /// </summary>
        /// <param name="info">Info about the operation.</param>
        /// <param name="options">Options to be considered by the algorithm.</param>
        /// <returns>A task which returns <see cref="ICompressionAlgorithm"/>.</returns>
        protected abstract Task<ICompressionAlgorithm> GetAlgorithmAsync(T info, AlgorithmOptions options);

        /// <summary>
        /// Actually starts this operation.
        /// </summary>
        /// <param name="operationInfo">Information required for the operation.</param>
        /// <returns>A result object consisting of further details.</returns>
        protected abstract Task<Result> StartOperation(T operationInfo);

        /// <summary>
        /// Calculates the total progress and informs all listeners about the progress.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="args">Consists of event parameters.</param>
        protected virtual void OnBytesProcessed(object sender, BytesProcessedEventArgs args)
        {
            TotalBytesProcessed += args.BytesProcessed;

            // fire event to inform listeners about progress update
            var progress = new Progress.Progress(_totalBytesToProcess, TotalBytesProcessed);
            var evtArgs = new ProgressUpdateEventArgs(progress);

            ProgressUpdate?.Invoke(this, evtArgs);
        }

        /// <summary>
        /// Evaluates the operation and returns the result.
        /// </summary>
        /// <param name="name">The name of the archive.</param>
        /// <param name="message">The message to be evaluated.</param>
        /// <param name="verboseMsg">A verbose message (usually exception message).</param>
        /// <param name="isSuccess">True if operation was successful, false otherwise.</param>
        /// <returns>An object that consists of result parameters.</returns>
        /// <exception cref="OperationCanceledException">Thrown if operation has been canceled.</exception>
        protected Result EvaluateResult(string name, string message, string verboseMsg, bool isSuccess)
        {
            if (TokenSource.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
            var status = isSuccess
                ? Result.Status.Success
                : Result.Status.Fail;

            return new Result(name)
            {
                StatusCode = status,
                Message = message,
                VerboseMessage = verboseMsg
            };
        }

        public void Dispose()
        {
            IsRunning = false;
            TokenSource.Dispose();
        }
    }
}
