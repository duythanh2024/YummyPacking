namespace UnusedAssetsFinder.Editor.Exceptions
{
    public class ProgressBarCancelledException : UnusedAssetsFinderBaseException
    {
        public ProgressBarCancelledException(string message) : base(message)
        {
        }
    }
}
