using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleZIP_UI.Application.Compression.Model;

namespace SimpleZIP_UI.Application.Compression.Operation
{
    internal abstract class ArchivingOperation<T> : IDisposable where T : OperationInfo
    {
        /// <summary>
        /// Source for cancellation token.
        /// </summary>
        protected readonly CancellationTokenSource TokenSource;

        /// <summary>
        /// True if an operation is in progress, false otherwise.
        /// </summary>
        internal bool IsRunning { get; private set; }

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
            try
            {
                return await StartOperation(operationInfo);
            }
            finally
            {
                IsRunning = false;
            }
        }

        /// <summary>
        /// Actually starts this operation.
        /// </summary>
        /// <param name="operationInfo">Information required for the operation.</param>
        /// <returns>A result object consisting of further details.</returns>
        protected abstract Task<Result> StartOperation(T operationInfo);

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
