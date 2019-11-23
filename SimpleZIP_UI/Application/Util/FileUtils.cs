// ==++==
// 
// Copyright (C) 2019 Matthias Fussenegger
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

using SimpleZIP_UI.Application.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.Application.Util
{
    public static class FileUtils
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
        public static bool ContainsIllegalChars(string filename)
        {
            return IllegalChars.Any(character => filename.IndexOf(character) != -1);
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
        /// Returns the file name extension of the specified path. Paths with multiple file name 
        /// extensions are also being considered, e.g. ".tar.gz" or ".tar.bz2".
        /// </summary>
        /// <param name="path">The path string from which to get the extension(s).</param>
        /// <returns>The extension(s) as string or <c>null</c> if path is <c>null</c>
        /// or <c>String.Empty</c> if path does not have an extension.</returns>
        public static string GetFileNameExtension(string path)
        {
            var fileNameExtension = string.Empty;
            if (ContainsMultipleFileNameExtensions(path))
            {
                foreach (var extendedFileType in Archives.ArchiveExtendedFileTypes)
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
        /// Deletes the specified item safely.
        /// </summary>
        /// <param name="item">The item to be deleted.</param>
        /// <param name="permanent">True to delete the item permanently.</param>
        public static async void DeleteSafe(IStorageItem item, bool permanent = true)
        {
            try
            {
                if (permanent)
                {
                    await item.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                else
                {
                    await item.DeleteAsync(StorageDeleteOption.Default);
                }
            }
            catch
            {
                // swallow exception
            }
        }

        /// <summary>
        /// Returns the file size of the specified file.
        /// </summary>
        /// <param name="file">The file from which to get the file size.</param>
        /// <returns>The file size as unsigned long.</returns>
        public static async Task<ulong> GetFileSizeAsync(StorageFile file)
        {
            var properties = await file.GetBasicPropertiesAsync();
            return properties.Size;
        }

        /// <summary>
        /// Returns the total file size of all files in the specified list.
        /// </summary>
        /// <param name="files">List consisting of <see cref="StorageFile"/>.</param>
        /// <returns>The total size of all files as unsigned long.</returns>
        public static async Task<ulong> GetFileSizesAsync(IReadOnlyList<StorageFile> files)
        {
            ulong totalSize = 0L;
            foreach (var file in files)
            {
                totalSize += await GetFileSizeAsync(file);
            }
            return totalSize;
        }

        /// <summary>
        /// Returns the specified temporary folder.
        /// </summary>
        /// <param name="location">The temporary folder to be get.</param>
        /// <returns>The specified temporary folder.</returns>
        internal static async Task<StorageFolder> GetTempFolderAsync(
            TempFolder location = TempFolder.Default)
        {
            string path = Path.GetTempPath();
            var folder = await StorageFolder.GetFolderFromPathAsync(path);

            if (location == TempFolder.Archives)
            {
                const string tempArchivesFolderName = "Archives";
                folder = await folder.CreateFolderAsync(
                    tempArchivesFolderName,
                    CreationCollisionOption.OpenIfExists);
            }

            return folder;
        }

        /// <summary>
        /// Deletes all files (non-recursive) from the specified storage folder.
        /// </summary>
        /// <returns>A task which can be awaited.</returns>
        internal static async Task CleanFolderAsync(StorageFolder folder)
        {
            if (folder != null)
            {
                var files = await folder.GetFilesAsync();
                foreach (var file in files)
                {
                    DeleteSafe(file);
                }
            }
        }

        /// <summary>
        /// Recursively gets a relative <c>filePath</c> from the specified <c>location</c>.
        /// </summary>
        /// <param name="location">Used to get directories and eventually the file.</param>
        /// <param name="filePath">Path to the file below <c>location.Path</c>.
        /// Can either be absolute or relative.</param>
        /// <returns>The file or <c>null</c> if the path equals <see cref="string.Empty"/>.</returns>
        internal static async Task<StorageFile> GetFileAsync(StorageFolder location, string filePath)
        {
            char separatorChar = Path.DirectorySeparatorChar;
            string dirPath = filePath.Replace('/', separatorChar);
            string locPath = location.Path;

            if (dirPath.StartsWith(locPath))
            {
                dirPath = dirPath.Remove(0, locPath.Length);
            }

            var pathMembers = dirPath.Split(separatorChar);

            StorageFile file = null;
            if (pathMembers.Length > 1) // at least one directory in path
            {
                int lastPos = pathMembers.Length - 1;
                var folder = location;
                // ignore last position because it's supposed be a file
                for (int i = 0; i < lastPos; ++i)
                {
                    string folderName = pathMembers[i];
                    if (!string.IsNullOrEmpty(folderName))
                    {
                        folder = await folder.GetFolderAsync(folderName);
                    }
                }

                file = await folder.GetFileAsync(pathMembers[lastPos]);
            }
            else if (pathMembers.Length == 1)
            {
                file = await location.GetFileAsync(pathMembers[0]);
            }

            return file;
        }

        /// <summary>
        /// Creates a file by its relative <c>filePath</c> in the specified <c>location</c>
        /// including all the missing directories in the path.
        /// </summary>
        /// <param name="location">Used to create directories and eventually the file.</param>
        /// <param name="filePath">Relative path to the file to be created.</param>
        /// <returns>The created file or <c>null</c> if path is <see cref="string.Empty"/>.</returns>
        internal static async Task<StorageFile> CreateFileAsync(StorageFolder location, string filePath)
        {
            var separatorChar = Path.DirectorySeparatorChar;
            var dirPath = filePath.Replace('/', separatorChar);
            var pathMembers = dirPath.Split(separatorChar);

            StorageFile file = null;
            if (pathMembers.Length > 1) // at least one directory in path
            {
                var lastPos = pathMembers.Length - 1;
                var folder = location;
                // ignore last position because it's supposed be a file
                for (var i = 0; i < lastPos; ++i)
                {
                    var folderName = pathMembers[i];
                    folder = await folder.CreateFolderAsync(folderName,
                        CreationCollisionOption.OpenIfExists);
                }
                file = await folder.CreateFileAsync(pathMembers[lastPos],
                    CreationCollisionOption.GenerateUniqueName);
            }
            else if (pathMembers.Length == 1)
            {
                file = await location.CreateFileAsync(pathMembers[0],
                    CreationCollisionOption.GenerateUniqueName);
            }

            return file;
        }
    }
}
