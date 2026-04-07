using UnityEditor;

namespace UnusedAssetsFinder.Editor.Popups
{
    public static class MessageDialogs
    {
        /// <summary>
        /// No unused asset found popup
        /// </summary>
        public static void NoAssetsFound()
        {
            EditorUtility.DisplayDialog(Strings.MessageBoxElements.NoAssetsFoundDialogueTitle,
                                        Strings.MessageBoxElements.NoAssetsFoundDialogueMessage,
                                        Strings.Common.Continue);
        }

        /// <summary>
        /// No assets marked for deletion popup
        /// </summary>
        public static void NoAssetsMarkedForDeletion()
        {
            EditorUtility.DisplayDialog(Strings.MessageBoxElements.NoAssetsMarkedDialogueTitle,
                                        Strings.MessageBoxElements.NoAssetsMarkedDialogueMessage,
                                        Strings.Common.Continue);
        }

        /// <summary>
        /// Popup that tells the user all unused assets were deleted
        /// </summary>
        public static void AllUnusedAssetsDeleted()
        {
            EditorUtility.DisplayDialog(Strings.MessageBoxElements.AllUnusedAssertsDeletedTitle,
                                        Strings.MessageBoxElements.AllUnusedAssertsDeletedMessage,
                                        Strings.Common.Continue);
        }
    }
}
