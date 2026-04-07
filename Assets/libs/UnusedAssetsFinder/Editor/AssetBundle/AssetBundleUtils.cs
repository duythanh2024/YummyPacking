using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnusedAssetsFinder.Editor.RuleSet;

namespace UnusedAssetsFinder.Editor.AssetBundle
{
    public static class AssetBundleUtils
    {
        /// <summary>
        /// Finds all the dependencies of all assets in all asset bundles
        /// </summary>
        /// <param name="allUnusedAssets">List of all unused assets</param>
        /// <param name="ruleSet">Rule set to filter with</param>
        public static void FindAssetsInAllAssetBundles(ref List<string> allUnusedAssets, UnusedAssetsRuleSet ruleSet)
        {
            var allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();

            allAssetBundleNames = allAssetBundleNames.Where(assetBundleName => !ruleSet.assetBundlesToInclude.Contains(assetBundleName)).ToArray();

            foreach (var assetBundleName in allAssetBundleNames)
            {
                var assetBundlePaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);

                foreach (var assetBundlePath in assetBundlePaths)
                {
                    var dependencies = AssetDatabase.GetDependencies(assetBundlePath, true);
                    allUnusedAssets = allUnusedAssets.Where(path => !dependencies.Contains(path)).ToList();
                }
            }
        }
    }
}
