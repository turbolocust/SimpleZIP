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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SharpCompress.Writers;

namespace SimpleZIP_UI.Application.Util
{
    internal static class WriterUtils
    {
        /// <summary>
        /// Writes a new entry with the specified name from the specified stream
        /// to the output stream of the archive. This method extends the functionality
        /// of the <see cref="IWriter"/> interface.
        /// </summary>
        /// <param name="writer">Instance of <see cref="IWriter"/>.</param>
        /// <param name="entryName">The name of the entry.</param>
        /// <param name="stream">The source stream.</param>
        /// <returns>An asynchronous operation that can be awaited.</returns>
        public static Task WriteAsync(this IWriter writer, string entryName, Stream stream)
        {
            return WriteAsync(writer, entryName, stream, CancellationToken.None);
        }

        /// <summary>
        /// Writes a new entry with the specified name from the specified stream
        /// to the output stream of the archive. This method extends the functionality
        /// of the <see cref="IWriter"/> interface.
        /// </summary>
        /// <param name="writer">Instance of <see cref="IWriter"/>.</param>
        /// <param name="entryName">The name of the entry.</param>
        /// <param name="stream">The source stream.</param>
        /// <param name="token">The token to be aggregated with the task.</param>
        /// <returns>An asynchronous operation that can be awaited.</returns>
        /// <exception cref="TaskCanceledException">Thrown if task got canceled.</exception>
        public static Task WriteAsync(this IWriter writer, string entryName,
            Stream stream, CancellationToken token)
        {
            if (token.IsCancellationRequested) return Task.FromCanceled(token);

            var task = Task.Run(() =>
            {
                // execute actual write option in child task
                var childTask = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        writer.Write(entryName, stream);
                    }
                    catch (Exception)
                    {
                        // ignored because an exception on a cancellation request 
                        // cannot be avoided if the stream gets disposed afterwards 
                    }
                }, TaskCreationOptions.AttachedToParent);

                var awaiter = childTask.GetAwaiter();
                while (!awaiter.IsCompleted)
                {
                    if (token.IsCancellationRequested)
                    {
                        throw new TaskCanceledException(nameof(childTask));
                    }
                }
            }, token);

            return task;
        }
    }
}
