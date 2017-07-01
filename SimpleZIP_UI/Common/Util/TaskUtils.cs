using System.Threading.Tasks;

namespace SimpleZIP_UI.Common.Util
{
    internal static class TaskUtils
    {
        /// <summary>
        /// Consumes a task and does not do anything with it. This is used to 
        /// suppress warnings if the <code>await</code> keyword is missing.
        /// </summary>
        /// <param name="task">The task to be consumed.</param>
        public static void Forget(this Task task)
        {
        }
    }
}
