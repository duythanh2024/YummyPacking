namespace UnusedAssetsFinder.Editor
{
    public static partial class Strings
    {
        public static class MessageBoxElements
        {
            public const string NoAssetsMarkedDialogueTitle   = "No Unused Assets Marked For Deletion";
            public const string NoAssetsMarkedDialogueMessage = "Select Some Assets To Delete";

            public const string ExportPackageSavePanelTitle = "Export Deleted Assets";

            public const string NoAssetsFoundDialogueTitle    = "No Unused Assets Found!";
            public const string NoAssetsFoundDialogueMessage  = "No Unused Assets Found In The Project! All Squeaky Clean";
            public const string DeleteAllFileConfirmationText = "Are you sure you want to delete all selected files?\r\nAll deleted items will be exported in a .unitypackage as a backup";

            public const string SpecialFolderBeingDeletedDialogueTitle   = "Special Unity folder picked up in the scan";
            public const string SpecialFolderBeingDeletedDialogueMessage = "Deletion of assets in these folders may result in Editor Plugins, Native Plugins and Assets called via code being deleted and potentially causing issues.\r\n\r\nSpecial Unity folder(s) included in the scan:\r\n";
            public const string SpecialFolderAssetFoundContinueButton    = "Continue Search";
            public const string SpecialFolderAssetFoundStopButton        = "Stop Search";

            public const string ScenesForDeletionHeader  = "Scenes Marked for Deletion";
            public const string ScenesForDeletionMessage = "Some scenes aren't in the build settings, dependencies for these scenes won't be checked.\r\nAdd scenes to the build settings to check their dependencies.\r\n" + "Scenes: \r\n";

            public const string CancelableProgressBarTitle = "Finding Unused Assets: ";

            public const string AllUnusedAssertsDeletedTitle   = "All Unused Assets Deleted";
            public const string AllUnusedAssertsDeletedMessage = "Your project is sqeaky clean!";
        }
    }
}
