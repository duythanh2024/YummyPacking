using System.Collections.Generic;

namespace UnusedAssetsFinder.Editor.Exceptions
{
    public class SpecialFolderIncludedInScanException : UnusedAssetsFinderBaseException
    {
        public SpecialFolderIncludedInScanException(List<string> specialFoldersToBeDeleted) : base(GenerateMessage(specialFoldersToBeDeleted))
        {
            this.specialFoldersToBeDeleted = specialFoldersToBeDeleted;
        }
        
        public List<string> specialFoldersToBeDeleted { get; }
        
        private static string GenerateMessage(List<string> specialFoldersToBeDeleted)
        {
            return $"The contents of {specialFoldersToBeDeleted.Count} special Unity folders can be potentially deleted. Stopping scan";
        }
    }
}
