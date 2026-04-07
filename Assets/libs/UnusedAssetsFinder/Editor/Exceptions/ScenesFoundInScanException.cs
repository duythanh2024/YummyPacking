using System.Collections.Generic;

namespace UnusedAssetsFinder.Editor.Exceptions
{
    public class ScenesFoundInScanException : UnusedAssetsFinderBaseException
    {
        public ScenesFoundInScanException(List<string> scenesThatCanBeDeleted) : base(GenerateMessage(scenesThatCanBeDeleted))
        {
            this.scenesThatCanBeDeleted = scenesThatCanBeDeleted;
        }

        public List<string> scenesThatCanBeDeleted { get; }

        private static string GenerateMessage(List<string> scenesThatCanBeDeleted)
        {
            return $"{scenesThatCanBeDeleted.Count} scenes can be potentially deleted. Stopping scan";
        }
    }
}
