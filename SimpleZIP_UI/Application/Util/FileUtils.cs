using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression;

namespace SimpleZIP_UI.Application.Util
{
    internal static class FileUtils
    {
        /// <summary>
        /// Array that consists of characters which are not allowed in file names.
        /// </summary>
        public static readonly char[] IllegalChars = { '<', '>', '/', '\\', '|', ':', '*', '\"', '?' };

        /// <summary>
        /// Checks if the specified string contains illegal characters which are not allowed in file names.
        /// </summary>
        /// <param name="filename">The file name to be validated.</param>
        /// <returns>True if file name contains illegal characters, false otherwise.</returns>
        public static bool ContainsIllegalChars(this string filename)
        {
            return IllegalChars.Any(character => filename.IndexOf(character) != -1);
        }

        /// <summary>
        /// Checks whether a specified path has multiple file name extensions.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>True if the path has at least two file name extensions.</returns>
        public static bool ContainsMultipleFileNameExtensions(this string path)
        {
            return !string.IsNullOrEmpty(path) && path.Split('.').Length - 1 > 1;
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
            var fileNameExtension = "";
            if (path.ContainsMultipleFileNameExtensions())
            {
                foreach (var extendedFileType in Archive.AlgorithmExtendedFileTypes)
                {
                    var key = extendedFileType.Key;
                    if (path.EndsWith(key))
                    {
                        fileNameExtension = key;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(fileNameExtension))
            {
                fileNameExtension = Path.GetExtension(path);
            }

            return fileNameExtension;
        }

        /// <summary>
        /// Deletes the specified item. This may be called if an operation has been canceled.
        /// </summary>
        /// <param name="item">The item to be deleted.</param>
        public static async void Delete(IStorageItem item)
        {
            await item.DeleteAsync();
        }

        /// <summary>
        /// Returns the file size of the specified file.
        /// </summary>
        /// <param name="file">The file from which to get the file size.</param>
        /// <returns>The file size as unsigned long.</returns>
        public static async Task<ulong> GetFileSize(StorageFile file)
        {
            var properties = await file.GetBasicPropertiesAsync();
            return properties.Size;
        }

        /// <summary>
        /// Creates a file (<see cref="string"/>) at the specified location 
        /// (<see cref="StorageFolder"/>) including all the missing directories in the path.
        /// </summary>
        /// <param name="location">Used to create directories and the file.</param>
        /// <param name="path">Dynamic path to the file to be created.</param>
        /// <returns>The created file or <code>null</code> if path is <code>String.Empty</code>.</returns>
        /// <exception cref="ArgumentException">Thrown if path is invalid and thus creation of 
        /// a folder or file failed.</exception>
        /// <exception cref="NullReferenceException">Thrown if any argument is <code>null</code>.</exception>
        public static async Task<StorageFile> CreateFileAsync(StorageFolder location, string path)
        {
            var separatorChar = Path.DirectorySeparatorChar;
            var dirPath = path.Replace('/', separatorChar);
            var pathMembers = dirPath.Split(separatorChar);

            StorageFile file = null;
            if (pathMembers.Length > 1) // at least one directory in path
            {
                var lastPos = pathMembers.Length - 1;
                var folder = location;
                // ignore last position as it's supposed be a file
                for (var i = 0; i < lastPos; ++i)
                {
                    var folderName = pathMembers[i];
                    folder = await folder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
                }
                file = await folder.CreateFileAsync(pathMembers[lastPos], CreationCollisionOption.GenerateUniqueName);
            }
            else if (pathMembers.Length > 0)
            {
                file = await location.CreateFileAsync(pathMembers[0], CreationCollisionOption.GenerateUniqueName);
            }
            return file;
        }
    }
}
