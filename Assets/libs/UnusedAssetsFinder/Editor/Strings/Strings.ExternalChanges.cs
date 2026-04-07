namespace UnusedAssetsFinder.Editor
{
    public static partial class Strings
    {
        public static class ExternalChanges
        {
            public const string DeletedAssetsRescanSceneHeader  = "Assets Deleted Or Moved From Outside Of Unity";
            public const string DeletedAssetsRescanSceneMessage = "Assets were deleted or moved after the scan. Would you like to rescan to find possible new Unused Assets?\r\nPressing \"" + Common.No + "\" will invalide the search.";
        }
    }
}
