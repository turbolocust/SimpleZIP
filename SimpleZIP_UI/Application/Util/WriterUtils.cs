// ==++==
// 
// Copyright (C) 2018 Matthias Fussenegger
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

using SharpCompress.Writers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
        /// <param name="token">The token to be aggregated with the task.</param>
        /// <returns>A task which can be awaited.</returns>
        /// <exception cref="OperationCanceledException">Thrown if operation got canceled.</exception>
        public static Task WriteAsync(this IWriter writer, string entryName, Stream stream, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var task = Task.Run(() =>
            {
                var childTask = Task.Run(() =>
                {
                    try
                    {
                        writer.Write(entryName, stream);
                    }
                    catch
                    {
                        // ignored because an exception on a cancellation request 
                        // cannot be avoided if the stream gets disposed afterwards 
                    }
                }, token);

                Task.WaitAll(new[] {childTask}, token);
            }, token);

            return task;
        }
    }
}