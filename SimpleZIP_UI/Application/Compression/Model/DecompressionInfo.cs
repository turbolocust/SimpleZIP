namespace SimpleZIP_UI.Application.Compression.Model
{
    internal class DecompressionInfo : OperationInfo
    {
        /// <summary>
        /// Aggregated item which can be extracted.
        /// </summary>
        internal ExtractableItem Item { get; }

        public DecompressionInfo(ExtractableItem item)
        {
            Item = item;
        }
    }
}
