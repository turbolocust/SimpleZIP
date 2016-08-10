﻿using System.IO;

namespace SimpleZIP_UI.Appl.Compression.Algorithm
{
    public interface ICompressionAlgorithm
    {
        /// <summary>
        /// Extracts an archive to a specified location.
        /// </summary>
        /// <param name="archiveName">The name of the archive to be extracted</param>
        /// <param name="location">Where the archive will be extracted</param>
        /// <exception cref="IOException">Thrown on any error when reading from or writing to streams</exception>
        void Extract(string archiveName, string location);

        /// <summary>
        /// Compresses an archive to a specified location.
        /// </summary>
        /// <param name="files">The files to put into the archive</param>
        /// <param name="archiveName">The name of the archive to be compressed</param>
        /// <param name="location">Where the archive will be created</param>
        /// <exception cref="IOException">Thrown on any error when reading from or writing to streams</exception>
        void Compress(FileInfo[] files, string archiveName, string location);
    }
}
