using System;
using System.IO;
using Windows.Storage;
using SimpleZIP_UI.Control;
using SimpleZIP_UI.Appl.Compression.Algorithm;
using SimpleZIP_UI.Exceptions;

namespace SimpleZIP_UI.Appl.Compression
{
    internal class CompressionHandler
    {
        private static CompressionHandler _instance;

        public static CompressionHandler Instance => _instance ?? (_instance = new CompressionHandler());

        private CompressionHandler()
        {
            // singleton
        }

        /// <summary>
        /// The algorithm that is used for compressing and decompressing operations.
        /// </summary>
        private ICompressionAlgorithm _compressionAlgorithm;

        public async void MakeArchive(FileInfo[] files, string archiveName, string location)
        {
            //TODO
        }

        public async void ExtractFromArchive(StorageFile file)
        {
            Control.Control.Algorithm value;
            // try to get enum type by file type
            if (!Control.Control.AlgorithmFileTypes.TryGetValue(file.FileType, out value))
            {
                ChooseStrategy(value);
                if (_compressionAlgorithm != null)
                {
                    // get the parent folder of the archive
                    var parent = await file.GetParentAsync();
                    if (parent != null)
                    {
                        // create new output folder with display name of archive
                        var outputFolder = await parent.CreateFolderAsync(file.DisplayName);
                        if (outputFolder != null)
                        {
                            // then extract archive to newly created output folder
                            _compressionAlgorithm.Extract(@file.Path, @outputFolder.Path);

                        }
                    }
                }
            }
            else
            {
                throw new InvalidFileTypeException("The selected file format is not supported.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void ChooseStrategy(Control.Control.Algorithm value)
        {
            switch (value) // assign correct instance to use the right algorithm
            {
                case Control.Control.Algorithm.Zip:
                    _compressionAlgorithm = Zipper.Instance;
                    break;

                case Control.Control.Algorithm.Gzip:
                    _compressionAlgorithm = GZipper.Instance;
                    break;
                case Control.Control.Algorithm.TarGz:
                    break;
                case Control.Control.Algorithm.TarBz2:
                    break;
                default:
                    throw new InvalidFileTypeException();
            }
        }
    }
}
