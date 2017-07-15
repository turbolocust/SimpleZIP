using SimpleZIP_UI.Application.Compression.Algorithm;

namespace SimpleZIP_UI.Application.Compression.Model
{
    /// <summary>
    /// Options for compressor streams used in <see cref="CompressorAlgorithm"/>.
    /// </summary>
    public class CompressorOptions
    {
        /// <summary>
        /// File name to be set for compression stream.
        /// </summary>
        internal string FileName { get; set; }

        /// <summary>
        /// Comment to be set for compression stream.
        /// </summary>
        internal string Comment { get; set; }

        /// <summary>
        /// True indicates a stream for compression, false for decompression.
        /// </summary>
        internal bool IsCompression { get; }

        internal CompressorOptions(bool isCompression)
        {
            IsCompression = isCompression;
        }
    }
}
