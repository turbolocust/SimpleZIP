namespace SimpleZIP_UI.Common.Util
{
    internal class FileValidator
    {
        private FileValidator()
        {
            // currently holds static members only
        }

        /// <summary>
        /// Checks if the specified string contains illegal characters that are not allowed in file names.
        /// </summary>
        /// <param name="fileName">The file name to be validated.</param>
        /// <returns>True if file name contains illegal characters, false otherwise.</returns>
        public static bool ContainsIllegalChars(string fileName)
        {
            return fileName.Contains("<") || fileName.Contains(">") || fileName.Contains("/") || fileName.Contains("\\") ||
                   fileName.Contains("|") || fileName.Contains(":") || fileName.Contains("*") || fileName.Contains("\"") ||
                   fileName.Contains("?");
        }
    }
}
