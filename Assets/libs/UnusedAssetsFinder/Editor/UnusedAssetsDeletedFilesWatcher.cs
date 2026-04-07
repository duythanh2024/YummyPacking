using UnityEditor;
using UnusedAssetsFinder.Editor.Popups;

namespace UnusedAssetsFinder.Editor
{
    internal class UnusedAssetsDeletedFilesWatcher : AssetPostprocessor
    {
        /// <summary>
        /// Called when assets have been changed from outside of Unity. This will run for every file and folder that is deleted as part of the plugin process
        /// </summary>
        /// <param name="importedAssets">Assets that were imported</param>
        /// <param name="deletedAssets">Deleted Assets</param>
        /// <param name="movedAssets">Moved Assets new path</param>
        /// <param name="movedFromAssetPaths">Moved Assets old path</param>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            //if the scan has not been run, then do nothing
            if (!UnusedAssetsEditorWindow.unusedAssetsFound) return;

            //AssetDatabase.IsValidFolder will not work here, as the folder has already been deleted
            //if(deletedAssets.All(AssetDatabase.IsValidFolder)) return;
            if (AssetDatabaseUtils.isDeletingEmptyFolders) return;

            //don't popup if deleting assets as part of the plugin process
            if (UnusedAssets.currentlyDeletingAssets) return;

            //files could have been moved into or out of a special named Unity folder in that case we just want to invalidate
            if (deletedAssets.Length > 0 || movedAssets.Length > 0)
            {
                var stop = MessagePrompts.RescanScenes();

                //want to invalidate anyway
                UnusedAssetsEditorWindow.InvalidateSearch();
                if (!stop)
                {
                    UnusedAssetsEditorWindow.ScanForUnusedAssets();
                }
            }
        }
    }
}
