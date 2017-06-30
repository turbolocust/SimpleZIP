using System;
using Windows.Storage;

namespace SimpleZIP_UI.Common.Util
{
    internal class FileUtils
    {
        private FileUtils()
        {
            // holds static members only
        }

        /// <summary>
        /// Checks if the specified string contains illegal characters which are not allowed in file names.
        /// </summary>
        /// <param name="filename">The file name to be validated.</param>
        /// <returns>True if file name contains illegal characters, false otherwise.</returns>
        public static bool ContainsIllegalChars(string filename)
        {
            return filename.Contains("<") || filename.Contains(">") || filename.Contains("/") || filename.Contains("\\") ||
                   filename.Contains("|") || filename.Contains(":") || filename.Contains("*") || filename.Contains("\"") ||
                   filename.Contains("?");
        }

        /// <summary>
        /// Deletes the specified item. This may be called if an operation has been canceled.
        /// </summary>
        /// <param name="item">The item to be deleted.</param>
        public static async void Delete(IStorageItem item)
        {
            await item.DeleteAsync();
        }
    }
}
