using System.Collections.Generic;
using UnityEngine;

namespace UnusedAssetsFinder.Editor.RuleSet
{
    /// <summary>
    /// Unused Asset Rule Set
    /// </summary>
    public partial class UnusedAssetsRuleSet : ScriptableObject
    {
        /// <summary>
        /// Asset extensions to exclude
        /// </summary>
        [Tooltip(Strings.Tooltips.AssetExtensionsReorderableTooltip)]
        public List<string> assetExtensionsToExclude = new List<string>
                                                       {
                                                           //C#
                                                           ".cs",
                                                           ".dll",
                                                           ".pdb",

                                                           //Shader
                                                           ".shader",
                                                           ".shadervariants",
                                                           ".cginc",
                                                           ".compute",
                                                           ".hlsl",

                                                           //Java
                                                           ".aar",
                                                           ".jar",
                                                           ".gradle",
                                                           ".so",
                                                           ".keystore",
                                                           ".pom",
                                                           ".srcaar",

                                                           //iOS
                                                           ".h",
                                                           ".m",
                                                           ".mm",
                                                           ".bundle",
                                                           ".framework",
                                                           // Required by Apple in April 2020
                                                           ".storyboard",
                                                           ".xib", // Xcode Interface Builder

                                                           //Misc general files used by plugins
                                                           ".json",
                                                           ".xml",
                                                           ".plist",

                                                           //Unity project files
                                                           ".preset",      //component presets
                                                           ".asmdef",      //assembly definition file
                                                           ".spriteatlas", //Sprite Atlas

                                                           //FB SDK
                                                           ".modulemap",
                                                           ".jslib",
                                                           ".projmods",

                                                           //Global custom #defines
                                                           //https://docs.unity3d.com/Manual/PlatformDependentCompilation.html
                                                           ".rsp", //Used to reference unreferenced DLL's

                                                           //UWP
                                                           ".pri",
                                                       };

        /// <summary>
        /// Any path that has this specific folder name somewhere in the path will be excluded.
        /// E.g. "../../Resources/"
        /// </summary>
        [Tooltip(Strings.Tooltips.IgnoreAssetsInSpecificallyNamedFoldersTooltip)]
        public List<string> ignoreAssetsInSpecificallyNamedFolders = new List<string>(SpecialUnityFolders);

        /// <summary>
        /// List of all special unity folders
        /// <para>https://docs.unity3d.com/Manual/SpecialFolders.html</para>
        /// <para>https://docs.unity3d.com/Manual/ScriptCompileOrderFolders.html</para>
        /// </summary>
        public static readonly List<string> SpecialUnityFolders = new List<string>
                                                                  {
                                                                      "Editor",
                                                                      "Editor Default Resources",
                                                                      "Gizmos",
                                                                      "Plugins",
                                                                      "Resources",
                                                                      "StreamingAssets",
                                                                  };

        /// <summary>
        /// Any asset or folder here will be ignored in the search. This list is used to convert objects to their paths in the editor
        /// </summary>
        [Tooltip(Strings.Tooltips.SpecificAssetsAndFoldersToIgnoreReorderableTooltip)]
        public List<Object> specificAssetsAndFoldersToIgnore = new List<Object>();

        /// <summary>
        /// Relative paths for the objects stored in <see cref="specificAssetsAndFoldersToIgnore"/>
        /// </summary>
        [HideInInspector] public List<string> specificAssetsAndFoldersToIgnorePaths = new List<string>();

        /// <summary>
        /// Any AssetBundle name list here will be excluded
        /// </summary>
        [Tooltip(Strings.Tooltips.AssetBundlesReorderableTooltip)]
        public List<string> assetBundlesToInclude = new List<string>();
    }
}
