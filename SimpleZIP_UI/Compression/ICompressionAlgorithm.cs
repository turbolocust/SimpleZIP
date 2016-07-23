using System.IO;

namespace SimpleZIP_UI.Compression
{
    public interface ICompressionAlgorithm
    {
        /// <summary>
        /// Extracts an archive to a specified location.
        /// </summary>
        /// <param name="archiveName">The name of the archive to be extracted</param>
        /// <param name="location">Where the archive will be extracted</param>
        void Extract(string archiveName, string location);

        /// <summary>
        /// Compresses an archive to a specified location.
        /// </summary>
        /// <param name="files">The files to put into the archive</param>
        /// <param name="archiveName">The name of the archive to be compressed</param>
        /// <param name="location">Where the archive will be created</param>
        void Compress(FileInfo[] files, string archiveName, string location);
    }
}
