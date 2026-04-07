using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnusedAssetsFinder.Editor
{
    [InitializeOnLoad]
    internal class UnusedAssetsDepreciatedFiles : MonoBehaviour
    {
        /// <summary>
        /// List of GUIDs of assets that are now depreciated
        /// </summary>
        private static readonly string[] DepreciatedFileGUIDs =
        {
            // v1.0.0 -> v1.1.0
            "3b69e61f79e05cf4e8fe57a4d15b1734", //Assets/UnusedAssets/Editor/Util/Utils.cs
            "d727ef5b2db0add44b105ce2b684dfa3", //Assets/UnusedAssets/Editor/DeleteUnusedAssets.cs
            "ccdf596a37f2850488e794669a05a3f9", //Assets/UnusedAssets/Unused Assets Finder ReadMe v1.0.pdf

            // v1.1.0 -> v1.1.1
            "2fb052ff4067a554d8ed3ecc087bf3b4", //Assets/UnusedAssets/Unused Assets Finder Read Me v1.1.0.pdf

            // v1.1.1 -> v1.1.2
            "11cf8d948b63910489eeb94123eb3dc6", //Assets/UnusedAssets/Editor/UnusedAssetDefineSymbols.cs
            "0a201764da9c47e488c3876c22a59cb1", //Assets/UnusedAssets/Unused Assets Finder Read Me v1.1.1.pdf

            // v1.1.2 -> v1.2
            "be570232e279e0044ab956ae1f476818", //Assets/UnusedAssets/Unused Assets Finder Read Me v1.1.2.pdf
            "79191f2abb737884e9de32c703ff5c7a", //Assets/UnusedAssets/Editor/Plugins
            "42df8c3e84e7f094e9a5b7ecf20d46c0", //Assets/UnusedAssets/Editor/Plugins/UnusedAssetsDefineSymbols.dll
            "c523a62dead9e85438e01c4166bf1fcd", //Assets/UnusedAssets/Editor/ScreenshotHelper.cs
            "0b0d23ef911c25b4da347a57dac1f790", //Assets/UnusedAssets/Editor/UnusedAssetsUnityPackageBuilder.cs

            // v1.2.0 -> v1.3.0
            // We moved to internally hosting the plugin as a package here so the path has changed, this doesn't change anything functionality
            "dfc9bc771da6e674988b80c41e9163b7", //Packages/com.unusedassetsfinder.unusedassetsfinder/Unused Assets Finder Read Me v1.2.0.pdf
            "8cf07e44423e22b4cb601ea6dae99311", //Packages/com.unusedassetsfinder.unusedassetsfinder/UnusedAssetsFinder.Editor.asmdef
            "f422d0dedf0332347a9b535dc1ebfb81", //Packages/com.unusedassetsfinder.unusedassetsfinder/Editor/Extensions/IListExtensions.cs
            "d66c296e4bcf7ff4cb7df64b98e68d83", //Packages/com.unusedassetsfinder.unusedassetsfinder/Editor/UnusedAssetsGitIgnore.cs

            // v1.3.0 -> v1.3.1
            "60a03330d4619be4f877d05cc2c304fb", //Packages/com.unusedassetsfinder.unusedassetsfinder/Unused Assets Finder Read Me v1.3.0.0.pdf

            // v1.3.1 -> v1.4.0
            // Moved back to submodules to allow quicker fixes of bugs without needing a PHD to setup the project
            "0ba957d117dda74478520a896292a049", //Assets/UnusedAssetsFinder/Unused Assets Finder Read Me v1.3.1.pdf
        };

        static UnusedAssetsDepreciatedFiles()
        {
            CheckForDepreciatedFiles();
        }

        /// <summary>
        /// Checks for any depreciated assets by GUID
        /// </summary>
        private static void CheckForDepreciatedFiles()
        {
            var allPaths = AssetDatabase.GetAllAssetPaths();

            var allGUIDs = new List<string>();
            for (var i = 0; i < allPaths.Length; i++)
            {
                if (!allPaths[i].StartsWith("Assets"))
                    continue;

                allGUIDs.Add(AssetDatabase.AssetPathToGUID(allPaths[i]));
            }

            var assetsToRemove = new List<string>();
            for (var i = 0; i < DepreciatedFileGUIDs.Length; i++)
            {
                if (allGUIDs.Contains(DepreciatedFileGUIDs[i]))
                    assetsToRemove.Add(DepreciatedFileGUIDs[i]);
            }

            var output = "Unused Assets Finder: Removed depreciated files\r\n";
            for (var i = 0; i < assetsToRemove.Count; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetsToRemove[i]);
                output += path + "\r\n";
                AssetDatabase.DeleteAsset(path);
            }

            if (assetsToRemove.Count > 0)
                Debug.Log(output);

            AssetDatabase.Refresh();
        }
    }
}