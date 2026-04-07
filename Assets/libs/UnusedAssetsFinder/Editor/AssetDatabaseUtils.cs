using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnusedAssetsFinder.Editor
{
    public static class AssetDatabaseUtils
    {
        /// <summary>
        /// Gets all subfolders within a folder
        /// </summary>
        /// <param name="folderPath">Path of the root folder to find all subfolders for</param>
        /// <returns>List of all subfolders</returns>
        public static string[] GetAssetFolderSubFolders(string folderPath)
        {
            var subfolderPaths = new List<string>();
            if (!AssetDatabase.IsValidFolder(folderPath)) return subfolderPaths.ToArray();

            var folders = AssetDatabase.GetSubFolders(folderPath);
            subfolderPaths.AddRange(folders);

            foreach (var folder in folders)
            {
                var subFolders = GetAssetFolderSubFolders(folder);
                subfolderPaths.AddRange(subFolders);
            }

            return subfolderPaths.ToArray();
        }

        /// <summary>
        /// Gets all assets inside the folder paths given
        /// </summary>
        /// <param name="folderPaths">Folder Paths to find assets in</param>
        /// <returns>Paths of all assets found</returns>
        public static string[] GetAllAssetsInFolders(string[] folderPaths)
        {
            return FindAssetPaths("t:Object", folderPaths);
        }

        /// <summary>
        /// Gets all assets inside the folder path given
        /// </summary>
        /// <param name="folderPath">Folder Path to find assets in</param>
        /// <returns>Paths of all assets found</returns>
        public static string[] GetPathsOfAllAssetsInFolder(string folderPath)
        {
            return FindAssetPaths("t:Object", new[] { folderPath });
        }

        #region Path Conversion
        /// <summary>
        /// Converts a relative asset path to the full system path
        /// </summary>
        /// <param name="relativePath">Relative Path</param>
        /// <returns>Full Path</returns>
        public static string ConvertRelativePathToFullPath(string relativePath)
        {
            //if it doesn't start with "Assets" then its not a relative path
            if (!relativePath.StartsWith("Assets/")) return relativePath;

            var projectPath = Application.dataPath.Replace("/Assets", "/");
            var fullPath = projectPath + relativePath;

            return fullPath;
        }

        /// <summary>
        /// Converts a full path to a project relative path
        /// </summary>
        /// <param name="fullPath">Full Path</param>
        /// <returns>Relative Path</returns>
        public static string ConvertFullPathToRelativePath(string fullPath)
        {
            fullPath = fullPath.Replace("\\", "/");
            //if the Path starts with "Assets/" then the path is already relative
            if (fullPath.StartsWith("Assets/")) return fullPath;

            var assetIndex = fullPath.IndexOf("/Assets/");
            var relativePath = fullPath.Substring(assetIndex + 1);

            return relativePath;
        }
        #endregion

        #region Folders
        /// <summary>
        /// Is the plugin currently deleting empty folders
        /// </summary>
        public static bool isDeletingEmptyFolders = false;

        /// <summary>
        /// Deletes all empty folders
        /// </summary>
        public static void DeleteAllEmptyFolders()
        {
            isDeletingEmptyFolders = true;
            var allEmptyRelativeDirs = FindEmptyFolders(Application.dataPath);

            for (var i = 0; i < allEmptyRelativeDirs.Length; i++)
            {
                var fullPath = ConvertRelativePathToFullPath(allEmptyRelativeDirs[i]);
                
                Directory.Delete(fullPath);
                File.Delete(fullPath + ".meta");
            }

            isDeletingEmptyFolders = false;
        }

        #region Find Empty Folders
        /// <summary>
        /// Finds all empty folders in the project
        /// </summary>
        /// <param name="rootPath">Root path to start the search from</param>
        /// <returns>Project relative paths of empty folders</returns>
        private static string[] FindEmptyFolders(string rootPath)
        {
            var emptyFolders = new List<string>();

            var allFolders = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);

            //loop through backwards so that the deepest directories are scanned first
            for (var i = allFolders.Length - 1; i >= 0; i--)
            {
                var folder = allFolders[i];
                var contents = Directory.GetFileSystemEntries(folder);

                if (contents.Length == 0)
                {
                    emptyFolders.Add(folder);
                    continue;
                }

                //check folders that are not declared empty
                var contentsList = contents.ToList();
                contentsList.RemoveAll(item => item.EndsWith(".meta")); //remove all meta files
                contentsList.RemoveAll(item => emptyFolders.Contains(item)); //remove all known empty folders

                if (contentsList.Count == 0) emptyFolders.Add(folder);
            }

            for (var i = 0; i < emptyFolders.Count; i++)
            {
                emptyFolders[i] = ConvertFullPathToRelativePath(emptyFolders[i]);
            }

            return emptyFolders.ToArray();
        }
        #endregion
        #endregion

        #region Move Assets To Trash

        /// <summary>
        /// Deletes all files from the list given, array should be relative project paths
        /// </summary>
        /// <param name="assetsToDelete">Array of all assets to delete</param>
        /// <param name="showProgressBar">Should the progress bar be shown</param>
        public static void MoveAssetsToTrash(string[] assetsToDelete, bool showProgressBar = false)
        {
            for (var i = 0; i < assetsToDelete.Length; i++)
            {
                var relativePath = "";
                var fullPath = "";

                //is relative path
                if (assetsToDelete[i].StartsWith("Assets/"))
                {
                    relativePath = assetsToDelete[i];
                    fullPath = ConvertRelativePathToFullPath(assetsToDelete[i]);
                }
                else if (!assetsToDelete[i].StartsWith("Assets/"))
                {
                    relativePath = ConvertFullPathToRelativePath(assetsToDelete[i]);
                    fullPath = assetsToDelete[i];
                }

                if (showProgressBar)
                {
                    var percent = Mathf.Max(1f, i) / assetsToDelete.Length;
                    var title = "Deleting Assets (" + i + "/" + assetsToDelete.Length + ")";

                    var stop = EditorUtility.DisplayCancelableProgressBar(title, relativePath, percent);
                    if (stop)
                    {
                        EditorUtility.ClearProgressBar();
                        return;
                    }
                }

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                else
                {
                    Debug.LogWarning($"UnusedAssetsFinder: File marked for deletion but not found at [{fullPath}]");
                }
                
                string fullPathMeta = fullPath + ".meta";
                if (File.Exists(fullPathMeta))
                {
                    File.Delete(fullPathMeta);
                }
                else
                {
                    Debug.LogWarning($"UnusedAssetsFinder: File marked for deletion but not found at [{fullPathMeta}]");
                }
            }

            if (showProgressBar) EditorUtility.ClearProgressBar();
        }
        #endregion

        #region Find Asset Paths
        /// <summary>
        /// Search the asset database using the search filter string.
        /// </summary>
        /// <param name="filter">The filter string can contain search data.</param>
        /// <returns>Array of matching assets. Note that paths will be returned.</returns>
        public static string[] FindAssetPaths(string filter)
        {
            return FindAssetPaths(filter, new[] { "Assets" });
        }

        /// <summary>
        /// Search the asset database using the search filter string.
        /// </summary>
        /// <param name="filter">The filter string can contain search data.</param>
        /// <param name="searchInFolders">The folders where the search will start.</param>
        /// <returns>Array of matching assets. Note that paths will be returned.</returns>
        public static string[] FindAssetPaths(string filter, string[] searchInFolders)
        {
            var assetGUIDs = AssetDatabase.FindAssets(filter, searchInFolders);
            var paths = new List<string>();

            foreach (var assetGUID in assetGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                paths.Add(assetPath);
            }
            return paths.ToArray();
        }
        #endregion

        #region Find And Load Assets
        /// <summary>
        /// Finds and loads assets
        /// </summary>
        /// <typeparam name="T">Type of the asset</typeparam>
        /// <param name="filter">Filter to use</param>
        /// <returns>Array of loaded assets.</returns>
        public static T[] FindAndLoadAssets<T>(string filter) where T : UnityEngine.Object
        {
            return FindAndLoadAssets<T>(filter, new[] { "Assets" });
        }

        /// <summary>
        /// Finds and loads assets
        /// </summary>
        /// <typeparam name="T">Type of the asset</typeparam>
        /// <param name="filter">Filter to use</param>
        /// <param name="searchInFolders">The folders where the search will start.</param>
        /// <returns>Array of loaded assets.</returns>
        public static T[] FindAndLoadAssets<T>(string filter, string[] searchInFolders) where T : UnityEngine.Object
        {
            var assetPaths = FindAssetPaths(filter, searchInFolders);
            var assets = new List<T>();

            foreach (var path in assetPaths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }
            return assets.ToArray();
        }
        #endregion

        #region Load All Assets Of Type
        /// <summary>
        /// Loads all assets of a certain type
        /// </summary>
        /// <returns>Array of loaded assets</returns>
        public static T[] LoadAllAssetsOfType<T>() where T : UnityEngine.Object
        {
            var tName = typeof(T).ToString();
            var assets = FindAndLoadAssets<T>("t:" + tName);
            return assets;
        }
        #endregion

        #region Object To Path
        /// <summary>
        /// Gets the paths of the objects given
        /// </summary>
        /// <param name="objects">Objects to get paths for</param>
        /// <returns>Array of object paths</returns>
        public static string[] GetPathsOfObjects(Object[] objects)
        {
            var paths = new List<string>();

            for (var i = 0; i < objects.Length; i++)
            {
                var path = AssetDatabase.GetAssetPath(objects[i]);
                paths.Add(path);
            }

            return paths.ToArray();
        }
        #endregion

        #region ProjectSettings Properties
        /// <summary>
        /// Get a property from a project setting
        /// </summary>
        /// <param name="projectSettingsAssetName">Name of the project setting</param>
        /// <param name="propertyName">Property Name</param>
        /// <returns>Object</returns>
        public static Object GetProjectSettingProperty(string projectSettingsAssetName, string propertyName)
        {
            //this find object that are already loaded, no need to unload
            var editorObjectCache = Resources.FindObjectsOfTypeAll<Object>();
            Object[] projectSettingsAsset = null;

            var hasAsset = editorObjectCache.Any(item => item.name.Equals(projectSettingsAssetName));
            if (hasAsset) projectSettingsAsset = editorObjectCache.Where(item => item.name.Equals(projectSettingsAssetName)).ToArray();
            var projectSettingSerializedObject = new SerializedObject(projectSettingsAsset);
            var obj = projectSettingSerializedObject.FindProperty(propertyName).objectReferenceValue;

            return obj;
        }
        #endregion
    }
}
