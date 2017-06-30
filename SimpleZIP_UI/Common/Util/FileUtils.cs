using System;
using System.IO;
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
        /// Returns the file name extension of the specified path. Paths with multiple file name 
        /// extensions are also being considered, e.g. ".tar.gz" or ".tar.bz2".
        /// </summary>
        /// <param name="path">The path string from which to get the extension(s).</param>
        /// <returns>The extension(s) as string or <code>null</code> if path is <code>null</code>
        /// or <code>String.Empty</code> if path does not have an extension.</returns>
        public static string GetFileNameExtension(string path)
        {
            return ContainsMultipleFileNameExtensions(path)
                ? path.Substring(path.IndexOf('.')) : Path.GetExtension(path);
        }

        /// <summary>
        /// Checks whether a specified path has multiple file name extensions.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>True if the path has at least two file name extensions.</returns>
        public static bool ContainsMultipleFileNameExtensions(string path)
        {
            return !string.IsNullOrEmpty(path) && path.Split('.').Length - 1 > 1;
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
