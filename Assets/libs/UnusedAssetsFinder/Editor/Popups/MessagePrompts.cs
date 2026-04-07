using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnusedAssetsFinder.Editor.Popups
{
    public static class MessagePrompts
    {
        /// <summary>
        /// Popup that asks the user to change serialisation mode
        /// </summary>
        /// <returns>True if the serialisation mode can be made</returns>
        public static bool TryChangeSerialisationMode()
        {
            return EditorUtility.DisplayDialog("Change asset serialisation mode to \"Force Text\"?",
                                                     "Unused Assets Finder needs to change the asset serialisation mode to \"Force Text\"\r\nThis may take a moment and may create changes in your version control system",
                                                     Strings.Common.Continue,
                                                     Strings.Common.Stop);
        }

        /// <summary>
        /// Prompt that asks the user if they want to continue the scan with scenes that are not included in the build
        /// </summary>
        /// <param name="scenesThatCanBeDeleted">List of scenes that can be deleted</param>
        /// <param name="maxScenesToPrint">Max scenes to print in the dialog box</param>
        /// <returns>If the scan should allow scenes to be included in the scan results</returns>
        public static bool ShouldAllowScenesInScanResults(List<string> scenesThatCanBeDeleted, int maxScenesToPrint = 10)
        {
            var message = Strings.MessageBoxElements.ScenesForDeletionMessage;

            var scenesToPrint = Mathf.Min(scenesThatCanBeDeleted.Count, maxScenesToPrint);
            var extraScenes   = Mathf.Max(scenesThatCanBeDeleted.Count - 10, 0);

            for (var i = 0; i < scenesToPrint; i++)
            {
                message += "\t• " + scenesThatCanBeDeleted[i];

                if (i < scenesThatCanBeDeleted.Count - 1)
                    message += "\r\n";
            }

            if (extraScenes > 0)
                message += "+" + extraScenes + " more scenes";

            return EditorUtility.DisplayDialog(Strings.MessageBoxElements.ScenesForDeletionHeader,
                                                        message,
                                                        Strings.Common.Continue,
                                                        Strings.Common.Stop);
        }

        /// <summary>
        /// Popup that warns if a special folder is included in the list of unused assets
        /// </summary>
        /// <param name="specialFoldersToBeDeleted">List of special folders that can be deleted</param>
        public static bool SpecialFolderFoundForDeletion(List<string> specialFoldersToBeDeleted)
        {
            var message = Strings.MessageBoxElements.SpecialFolderBeingDeletedDialogueMessage;
            for (var i = 0; i < specialFoldersToBeDeleted.Count; i++)
            {
                message += "\t" + specialFoldersToBeDeleted[i];

                if (i < specialFoldersToBeDeleted.Count - 1)
                    message += "\r\n";
            }

            return EditorUtility.DisplayDialog(Strings.MessageBoxElements.SpecialFolderBeingDeletedDialogueTitle,
                                                        message,
                                                        Strings.MessageBoxElements.SpecialFolderAssetFoundContinueButton,
                                                        Strings.MessageBoxElements.SpecialFolderAssetFoundStopButton);
        }

        /// <summary>
        /// Asks the user if they want to rescan for unused assets if deleted from outside of Unity
        /// </summary>
        /// <returns>True if execution should continue</returns>
        public static bool RescanScenes()
        {
            return EditorUtility.DisplayDialog(Strings.ExternalChanges.DeletedAssetsRescanSceneHeader,
                                               Strings.ExternalChanges.DeletedAssetsRescanSceneMessage,
                                               Strings.Common.No,
                                               Strings.Common.Rescan);
        }
    }
}
