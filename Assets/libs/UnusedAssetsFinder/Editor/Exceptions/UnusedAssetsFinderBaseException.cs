using System;

namespace UnusedAssetsFinder.Editor.Exceptions
{
    public abstract class UnusedAssetsFinderBaseException : Exception
    {
        protected UnusedAssetsFinderBaseException(string message) : base(message)
        {
        }
    }
}
