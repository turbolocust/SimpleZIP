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

using SimpleZIP_UI.Application;
using SimpleZIP_UI.Application.Compression;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleZIP_UI.I18N
{
    internal static class ExceptionMessages
    {
        internal enum OperationType
        {
            None = 0,
            Reading = 1,
            ReadingPasswordSet = 2,
            Writing = 4
        }

        private static bool IsReadOperation(this OperationType type)
        {
            return type == OperationType.Reading || type == OperationType.ReadingPasswordSet;
        }

        /// <summary>
        /// Gets an internationalized string for the specified exception. 
        /// The string is a friendly error message which can be shown to users.
        /// </summary>
        /// <param name="ex">Exception from which to receive an internationalized message.</param>
        /// <param name="operationType">Specifies the type of the operation.
        /// This affects the returned message.</param>
        /// <param name="file">File that was processed when exception occurred.</param>
        /// <returns>Internationalized error message.</returns>
        internal static async Task<string> GetStringFor(Exception ex,
            OperationType operationType = OperationType.None, StorageFile file = null)
        {
            string message;
            bool resolved = false;
            bool prependReason = true;

            switch (ex)
            {
                case ArchiveEncryptedException _:
                    {
                        message = operationType == OperationType.ReadingPasswordSet
                            ? "ErrorReadingArchiveWithPassword/Text"
                            : "FileEncryptedMessage/Text";

                        break;
                    }
                case InvalidOperationException _:
                    {
                        if (file != null && operationType.IsReadOperation())
                        {
                            // to inform that file format is not supported
                            message = await Archives.IsRarArchive(file).ConfigureAwait(false)
                                ? "RAR5FormatNotSupported/Text"
                                : "FileFormatNotSupported/Text";
                        }
                        else
                        {
                            message = "FileFormatNotSupported/Text";
                        }

                        break;
                    }
                case NullReferenceException _:
                    {
                        if (operationType.IsReadOperation())
                        {
                            message = "ErrorReadingArchive/Text"; // assume illegal format
                        }
                        else
                        {
                            goto default;
                        }

                        break;
                    }
                case FileNotFoundException _:
                    {
                        message = Resources.GetString("FileNotFound/Text");
                        resolved = true; // since resource string already set
                        break;
                    }
                case IOException _:
                    {
                        if (operationType == OperationType.Writing && IsDiskFull(ex))
                        {
                            message = "ErrorDiskFull/Text";
                        }
                        else if (operationType == OperationType.Writing)
                        {
                            message = "ErrorWritingFile/Text";
                        }
                        else if (operationType.IsReadOperation())
                        {
                            message = "ErrorReadingArchive/Text";
                        }
                        else
                        {
                            goto default;
                        }

                        break;
                    }
                default:
                    message = Resources.GetString("ErrorMessageDisplay/Text", ex.Message);
                    resolved = true;
                    prependReason = false;
                    break;
            }

            return PostProcessMessage(message, !resolved, prependReason);
        }

        private static string PostProcessMessage(string message, bool resolve, bool prependReasonString)
        {
            string processed = message;

            if (resolve)
            {
                processed = Resources.GetString(processed);
            }

            if (prependReasonString)
            {
                processed = $"{Resources.GetString("Reason/Text")}: {processed}";
            }

            return processed;
        }

        private static bool IsDiskFull(Exception ex)
        {
            const int hResultErrHandleDiskFull = unchecked((int)0x80070027);
            const int hResultErrDiskFull = unchecked((int)0x80070070);

            return ex.HResult == hResultErrHandleDiskFull
                   || ex.HResult == hResultErrDiskFull;
        }
    }
}
