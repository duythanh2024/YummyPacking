using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnusedAssetsFinder.Editor.AssetBundle;
using UnusedAssetsFinder.Editor.Exceptions;
using UnusedAssetsFinder.Editor.Packaging;
using UnusedAssetsFinder.Editor.Popups;
using UnusedAssetsFinder.Editor.RuleSet;
using UnusedAssetsFinder.Editor.TreeView;
using Object = UnityEngine.Object;

namespace UnusedAssetsFinder.Editor
{
    /// <summary>
    /// Logic for Unused Assets
    /// </summary>
    public static class UnusedAssets
    {
        /// <summary>
        /// Rule set used to filter files, reset every time the user searches
        /// </summary>
        private static UnusedAssetsRuleSet cachedRuleSet;

        /// <summary>
        /// Finds all unused assets
        /// </summary>
        /// <param name="ruleSet">Rule set to cache and filter with</param>
        /// <param name="allUnusedAssets">List of all unused assets, will be cleared before being populated</param>
        public static void FindUnusedAssets(UnusedAssetsRuleSet ruleSet, ref List<string> allUnusedAssets)
        {
            //start with a empty list
            allUnusedAssets.Clear();

            cachedRuleSet = ruleSet;
            cachedRuleSet.PruneEntries();

            FindScenesNotInBuildSettings(ruleSet);

            FindSpecialFoldersIncludedInScan();
            
            EnsureAssetsAreText();

            //Save scene if dirty
            var currentScene = EditorSceneManager.GetActiveScene();
            if (currentScene.isDirty)
                EditorSceneManager.SaveScene(currentScene);

            //build initial list
            BuildUnusedAssetsList(ref allUnusedAssets);

            //filter out project settings
            FilterOutProjectSettingFiles(ref allUnusedAssets);

            //filter in
            FilterOutRuleSets(ref allUnusedAssets);

            //filter out folders
            FilterOutFolderDependenciesFromList(ref allUnusedAssets);

            //filter out extensions
            FilterOutFileExtensions(ref allUnusedAssets);

            //filter out specific paths
            FilterOutSpecificPaths(ref allUnusedAssets);

            //Filter out editor tile palettes
            FilterOutTilePalettes(ref allUnusedAssets);

            //find all scene dependencies
            RemoveSceneDependencies(ref allUnusedAssets);

            //check for assets in a asset bundle
            AssetBundleUtils.FindAssetsInAllAssetBundles(ref allUnusedAssets, ruleSet);
        }

        /// <summary>
        /// Finds any special Unity folders that may be included in the scan
        /// </summary>
        private static void FindSpecialFoldersIncludedInScan()
        {
            var specialFoldersNotExcluded = UnusedAssetsRuleSet.SpecialUnityFolders.Where(folder => !cachedRuleSet.ignoreAssetsInSpecificallyNamedFolders.Contains(folder))
                                                               .ToList();

            // if there any special folders marked for deletion, throw up a prompt that can stop execution via throwing an exception
            if (specialFoldersNotExcluded.Count > 0)
            {
                var @continue = MessagePrompts.SpecialFolderFoundForDeletion(specialFoldersNotExcluded);

                if (!@continue)
                    throw new SpecialFolderIncludedInScanException(specialFoldersNotExcluded);
            }
        }

        /// <summary>
        /// Finds any scenes that are not in the editor build settings window
        /// </summary>
        /// <param name="ruleSet">Rule Set</param>
        private static void FindScenesNotInBuildSettings(UnusedAssetsRuleSet ruleSet)
        {
            var environmentScenePaths = new List<string>();

            // We have to check the version here as we're not scraping the project files at this point
            // and we need to know if there are scenes set as the editing prefab environments so we don't
            // throw up false positives
#if UNITY_2018_3_OR_NEWER
            string GetPathOfObject(Object obj)
            {
                if (obj == null)
                    return null;

                var success = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid, out long localId);

                if (!success)
                    return null;

                var path = AssetDatabase.GUIDToAssetPath(guid);

                return path;
            }

            environmentScenePaths.Add(GetPathOfObject(EditorSettings.prefabRegularEnvironment));
            environmentScenePaths.Add(GetPathOfObject(EditorSettings.prefabUIEnvironment));
            environmentScenePaths.RemoveAll(string.IsNullOrEmpty);
#endif // UNITY_2018_3_OR_NEWER

            var allScenePaths = AssetDatabase.FindAssets("t:Scene")
                                             .Select(AssetDatabase.GUIDToAssetPath)
                                             .Where(path => path.StartsWith("Assets"))
                                             .Where(path => !environmentScenePaths.Contains(path))
                                             .Where(path => !ruleSet.specificAssetsAndFoldersToIgnorePaths.Contains(path));

            var scenesInBuildPaths = EditorBuildSettings.scenes
                                                        .Select(scene => scene.path);

            var unusedScenes = allScenePaths.Where(path => !scenesInBuildPaths.Contains(path))
                                            .ToList();

            if (unusedScenes.Count == 0)
                return;

            var sceneNames = unusedScenes.Select(Path.GetFileName)
                                         .ToList();

            var @continue = MessagePrompts.ShouldAllowScenesInScanResults(sceneNames);

            if (!@continue)
                throw new ScenesFoundInScanException(sceneNames);
        }

        /// <summary>
        /// Asks the user to allow Unused Assets Finder to change the asset serialisation mode
        /// </summary>
        private static void EnsureAssetsAreText()
        {
            if (EditorSettings.serializationMode == SerializationMode.ForceText)
                return;

            var success = MessagePrompts.TryChangeSerialisationMode();

            if (!success)
                throw new ChangeAssetSerialisationModeRequestDeniedException();

            EditorSettings.serializationMode = SerializationMode.ForceText;
        }

        /// <summary>
        /// Filters out assets that are in the project settings
        /// </summary>
        /// <param name="allUnusedAssets">All Unused Assets</param>
        private static void FilterOutProjectSettingFiles(ref List<string> allUnusedAssets)
        {
            var projectSettingsDependencies = ProjectSettingScrapper.GetAllDependencyPaths();
            allUnusedAssets.RemoveAll(path => projectSettingsDependencies.Contains(path));
        }

        /// <summary>
        /// Builds the initial list of unused assets
        /// </summary>
        /// <param name="allUnusedAssets">List of all unused assets</param>
        private static void BuildUnusedAssetsList(ref List<string> allUnusedAssets)
        {
            //get all assets
            var allAssetsInProject = AssetDatabase.GetAllAssetPaths();

            //exclude files based on the rule set
            for (var i = 0; i < allAssetsInProject.Length; i++)
            {
                bool canAddAsset = true;
                var  asset       = allAssetsInProject[i];

                if (asset.Contains("/UnusedAssetsFinder/")) canAddAsset = false;
                if (asset.Contains("/UnusedAssetsFinder/Demo")) canAddAsset = true; // We want to include the demo folder
                if (!canAddAsset) continue;

                //skips over packages in 2018.x
                if (!asset.StartsWith("Assets/")) canAddAsset = false;
                if (!canAddAsset) continue;

                //Folder, doesn't matter if its used or not, its not an asset that's used
                if (AssetDatabase.IsValidFolder(asset)) canAddAsset = false;
                if (!canAddAsset) continue;

                allUnusedAssets.Add(asset);
            }
        }

        /// <summary>
        /// Filters out the rule sets from the list of unused assets
        /// </summary>
        /// <param name="allUnusedAssets">List of all unused assets</param>
        private static void FilterOutRuleSets(ref List<string> allUnusedAssets)
        {
            var ruleSetGUIDs = AssetDatabase.FindAssets("t:UnusedAssetsRuleSet").ToList();

            //converts GUIDs to paths
            var ruleSetPaths = new List<string>();
            for (var i = 0; i < ruleSetGUIDs.Count; i++)
            {
                ruleSetPaths.Add(AssetDatabase.GUIDToAssetPath(ruleSetGUIDs[i]));
            }

            allUnusedAssets.RemoveAll(path => ruleSetPaths.Contains(path));
        }

        /// <summary>
        /// Removes all assets and their dependencies if the folder is excluded, this stops things like prefabs with dependencies like .fbx outside of the folder being removed by the plugin
        /// </summary>
        /// <param name="allUnusedAssets">List of all unused assets</param>
        private static void FilterOutFolderDependenciesFromList(ref List<string> allUnusedAssets)
        {
            foreach (var folderToExclude in cachedRuleSet.ignoreAssetsInSpecificallyNamedFolders)
            {
                var allSpecialAssetsBeingExcluded = allUnusedAssets.Where(path => path.Contains("/" + folderToExclude + "/")).ToList();
                var dependencies                  = AssetDatabase.GetDependencies(allSpecialAssetsBeingExcluded.ToArray(), true);
                allUnusedAssets = allUnusedAssets.Where(asset => !dependencies.Contains(asset)).ToList();
            }
        }

        /// <summary>
        /// Removes the assets and it's dependencies if the extension is excluded, this stops things like prefabs with dependencies like .fbx outside of the folder being removed by the plugin
        /// </summary>
        /// <param name="allUnusedAssets">List of all unused assets</param>
        private static void FilterOutFileExtensions(ref List<string> allUnusedAssets)
        {
            foreach (var extension in cachedRuleSet.assetExtensionsToExclude)
            {
                var filesToExclude = allUnusedAssets.Where(path => path.EndsWith(extension)).ToList();
                var dependencies   = AssetDatabase.GetDependencies(filesToExclude.ToArray(), true);
                allUnusedAssets = allUnusedAssets.Where(asset => !dependencies.Contains(asset)).ToList();
            }
        }

        /// <summary>
        /// Removes the asset and its dependencies if the specific path is excluded, this stops things like prefabs with dependencies like .fbx outside of the folder being removed by the plugin
        /// </summary>
        /// <param name="allUnusedAssets">List of all unused assets</param>
        private static void FilterOutSpecificPaths(ref List<string> allUnusedAssets)
        {
            foreach (var assetPath in cachedRuleSet.specificAssetsAndFoldersToIgnorePaths)
            {
                var isFolder = AssetDatabase.IsValidFolder(assetPath);

                if (isFolder)
                {
                    //terminate the folder path
                    //by default the path will be "Assets/Folder"
                    //and not "Assets/Folder/"
                    var folderPath = assetPath + "/";

                    var allAssetsInFolder = allUnusedAssets.Where(path => path.Contains(folderPath)).ToArray();
                    var dependencies      = AssetDatabase.GetDependencies(allAssetsInFolder, true);
                    allUnusedAssets.RemoveAll(asset => dependencies.Contains(asset));
                }
                else
                {
                    var dependencies = AssetDatabase.GetDependencies(assetPath, true);
                    allUnusedAssets.RemoveAll(asset => dependencies.Contains(asset));
                }
            }
        }

        /// <summary>
        /// Filter out Tile Palettes used by the editor, these aren't referenced anywhere in the editor apart from the Tile Palette editor, which loads them indirectly
        /// </summary>
        /// <param name="allUnusedAssets">List of all unused assets</param>
        private static void FilterOutTilePalettes(ref List<string> allUnusedAssets)
        {
            var allTilePaletteGUIDs = AssetDatabase.FindAssets("t:GridPalette");

            foreach (var paletteGUID in allTilePaletteGUIDs)
            {
                //getting the path to the object gets the path to the prefab that houses it
                var path = AssetDatabase.GUIDToAssetPath(paletteGUID);
                allUnusedAssets.RemoveAll(item => item.Equals(path));
            }
        }

        /// <summary>
        /// Finds all the dependencies of all the scenes and removes them from the list
        /// </summary>
        /// <param name="allUnusedAssets">List of all unused assets</param>
        private static void RemoveSceneDependencies(ref List<string> allUnusedAssets)
        {
            //get all scenes in the build
            var allSceneObjects = EditorBuildSettings.scenes;

            //get all dependencies for each scene in the build settings
            for (var i = 0; i < allSceneObjects.Length; i++)
            {
                string scenePath = allSceneObjects[i].path;
                if (allUnusedAssets.Contains(scenePath)) allUnusedAssets.Remove(scenePath);

                string sceneName = null;

                //only get scenes in the assets folder
                if (scenePath.StartsWith("Assets/"))
                {
                    var lastFolderSlash = scenePath.LastIndexOf("/", StringComparison.Ordinal) + 1;
                    sceneName = scenePath.Substring(lastFolderSlash);
                }

                //if the scene string is null then skip
                if (string.IsNullOrEmpty(scenePath)) continue;

                //get all scene dependencies
                //this is recursive so all dependencies of all assets should be in this list
                var dependencies = AssetDatabase.GetDependencies(scenePath, true);

                //loop through scene dependencies
                for (var j = 0; j < dependencies.Length; j++)
                {
                    var dependencyPath = dependencies[j];

                    if (allUnusedAssets.Contains(dependencyPath))
                        allUnusedAssets.Remove(dependencyPath);

                    //UI to show and stop progress
                    var scenePercent = Mathf.Max(j, 1f) / dependencies.Length;
                    var stop         = EditorUtility.DisplayCancelableProgressBar(Strings.MessageBoxElements.CancelableProgressBarTitle + sceneName, dependencyPath, scenePercent);

                    if (stop)
                    {
                        EditorUtility.ClearProgressBar();
                        throw new ProgressBarCancelledException("Removing scene dependencies operation cancelled via progress bar");
                    }
                }
            }

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Finds all scenes that will be deleted
        /// </summary>
        /// <param name="allUnusedAssets">List of all unused assets</param>
        /// <param name="scenesThatWillBeDeleted">List of all scenes that will be deleted</param>
        private static void FindAllScenesToBeDeletedInAssets(ref List<string> allUnusedAssets, out List<string> scenesThatWillBeDeleted)
        {
            //check to see if there are any scenes in the list that are in the assets folder
            var hasScenesThatWillBeDeleted = allUnusedAssets.Any(item => item.StartsWith("Assets/") && item.EndsWith(".unity"));
            if (!hasScenesThatWillBeDeleted)
            {
                scenesThatWillBeDeleted = new List<string>();
                return;
            }

            scenesThatWillBeDeleted = allUnusedAssets.Where(item => item.StartsWith("Assets/") && item.EndsWith(".unity")).ToList();

            for (var i = 0; i < scenesThatWillBeDeleted.Count; i++)
            {
                var lastFolderSlash = scenesThatWillBeDeleted[i].LastIndexOf("/", StringComparison.Ordinal) + 1;
                scenesThatWillBeDeleted[i] = scenesThatWillBeDeleted[i].Substring(lastFolderSlash);
            }
        }

        #region Export / Delete

        /// <summary>
        /// Cached tree view, have to cache as you cannot pass a ref as a callback in C# 3.5
        /// </summary>
        private static UnusedAssetsTreeView cachedTreeView;

        /// <summary>
        /// Is UnusedAssets currently deleting assets?
        /// </summary>
        public static bool currentlyDeletingAssets = false;

        /// <summary>
        /// Delete any unused assets that user selects
        /// </summary>
        public static async void DeleteSelectedUnusedAssets(UnusedAssetsTreeView treeView)
        {
            cachedTreeView = treeView;

            var allElementsToDelete = IdentifyForDeleteRecursively(treeView.TreeModel.root);

            if (allElementsToDelete.Count == 0)
            {
                MessageDialogs.NoAssetsMarkedForDeletion();
                return;
            }

            var stop = !EditorUtility.DisplayDialog(Strings.Common.Warning, Strings.MessageBoxElements.DeleteAllFileConfirmationText, Strings.Common.Yes, Strings.Common.Cancel);
            if (stop) return;

            // !!!DO NOT DELETE!!! // var pathsToDelete = allElementsToDelete.Where(asset => !asset.hasChildren).Select(asset => asset.path).Where(path => path != null).ToList(); 
            //this will filter out the parent folder paths if it ever causes a problem with the export - but it does not cause the ExportPackageOptions.Interactive - I checked it
            List<string> pathsToDelete = allElementsToDelete.Select(asset => asset.path)
                                                            .Where(path => path != null)
                                                            .ToList();

            await ExportPackage.Export(pathsToDelete);

            var assetsToDelete  = new List<string>();
            var foldersToDelete = new List<string>();

            List<string> allElementPaths = allElementsToDelete.Select(a => a.path)
                                                              .ToList();

            // This is in case a folder is marked as shouldDelete, but contains
            // assets that have been ignored
            foreach (var element in allElementsToDelete)
            {
                if (element.hasChildren) // Is a folder
                {
                    string pathWithoutASlashCauseUnityIsABitch = element.path.TrimEnd('/');

                    string[] allAssetsInPath = AssetDatabaseUtils.GetPathsOfAllAssetsInFolder(pathWithoutASlashCauseUnityIsABitch);

                    bool safeToDelete = allElementPaths.ContainsAllItems(allAssetsInPath);

                    if (safeToDelete)
                    {
                        foldersToDelete.Add(pathWithoutASlashCauseUnityIsABitch);
                    }
                }
                else
                {
                    assetsToDelete.Add(element.path);
                }
            }

            currentlyDeletingAssets = true;
            AssetDatabaseUtils.MoveAssetsToTrash(assetsToDelete.ToArray(), true);

            foreach (var folderPath in foldersToDelete)
            {
                string fullFolderPath = AssetDatabaseUtils.ConvertRelativePathToFullPath(folderPath);

                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(fullFolderPath);
                    File.Delete($"{fullFolderPath}.meta");
                }
            }

            AssetDatabase.Refresh();
            currentlyDeletingAssets = false;

            cachedTreeView.TreeModel.RemoveElements(allElementsToDelete); //Delete all tree elements that are no longer needed

            var children = cachedTreeView.TreeModel.root.children.Count;
            if (children == 0)
            {
                MessageDialogs.AllUnusedAssetsDeleted();
                UnusedAssetsEditorWindow.InvalidateSearch();
            }
        }

        /// <summary> 
        /// Recursively Identify Files to Delete
        /// </summary>
        private static List<AssetTreeElement> IdentifyForDeleteRecursively(AssetTreeElement element)
        {
            var toDelete = new List<AssetTreeElement>();

            foreach (var child in element.children)
            {
                if (child.hasChildren)
                {
                    toDelete.AddRange(IdentifyForDeleteRecursively(child)); //add all child's children before child - will allow the child to be determined if an empty folder later on and thus allow it to be deleted
                }

                if (child.GetShouldDelete())
                {
                    toDelete.Add(child);
                }
            }

            return toDelete;
        }

        #endregion
    }

    public static class ListExtensions
    {
        // source.ContainsAllItems(values)
        public static bool ContainsAllItems<T>(this IEnumerable<T> source, IEnumerable<T> values)
        {
            return values.All(val => source.Contains(val));
        }
    }
}