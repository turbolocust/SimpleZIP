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

using System;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace SimpleZIP_UI.Application.Util
{
    internal static class CoreDispatcherUtils
    {
        // see https://github.com/Microsoft/Windows-task-snippets/blob/master/tasks/UI-thread-task-await-from-background-thread.md

        /// <summary>
        /// Returns a task for the specified function which then can be run asynchronously.
        /// </summary>
        /// <typeparam name="T">Return type of task.</typeparam>
        /// <param name="dispatcher">Instance of <see cref="CoreDispatcher"/>.</param>
        /// <param name="func">Function to be executed asynchronously.</param>
        /// <param name="priority">Priority for core dispatcher.</param>
        /// <returns>A task which can be awaited.</returns>
        public static async Task<T> RunTaskAsync<T>(this CoreDispatcher dispatcher, Func<Task<T>> func,
            CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            await dispatcher.RunAsync(priority, async () =>
            {
                try
                {
                    taskCompletionSource.SetResult(await func());
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });

            return await taskCompletionSource.Task;
        }
    }
}
