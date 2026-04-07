namespace UnusedAssetsFinder.Editor.Util
{
    public struct ConvertedFileSize
    {
        /// <summary>
        /// File size in bytes
        /// </summary>
        private readonly long fileSize;

        /// <summary>
        /// File size denomination
        /// </summary>
        private readonly string sizeDenomination;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileSizeInBytes">File size in bytes</param>
        public ConvertedFileSize(long fileSizeInBytes)
        {
            fileSize = fileSizeInBytes;

            var order = 0;
            while (fileSize >= 1024 && order < FileSizes.Length - 1)
            {
                order++;
                fileSize /= 1024;
            }

            sizeDenomination = FileSizes[order];
        }

        /// <summary>
        /// Displays the converted file size to a string
        /// </summary>
        /// <returns>Converted file size as a string</returns>
        public override string ToString()
        {
            return $"{fileSize} {sizeDenomination}";
        }

        /// <summary>
        /// File size suffixes
        /// </summary>
        private static readonly string[] FileSizes =
        {
            "B",
            "KB",
            "MB",
            "GB",
            "TB"
        };
    }
}
