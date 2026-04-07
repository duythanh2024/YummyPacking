using System.Linq;

namespace UnusedAssetsFinder.Editor.RuleSet
{
    public partial class UnusedAssetsRuleSet
    {
        /// <summary>
        /// Creates a new default ruleset
        /// </summary>
        /// <returns>Unused Asset Rule Set</returns>
        public static UnusedAssetsRuleSet CreateDefaultRuleSet(string name = null)
        {
            var instance = CreateInstance<UnusedAssetsRuleSet>();
            instance.name = name ?? "Default Rule Set";
            return instance;
        }
        
        /// <summary>
        /// Prune duplicate entries and empties
        /// </summary>
        public void PruneEntries()
        {
            assetExtensionsToExclude = assetExtensionsToExclude.Distinct().ToList();
            assetExtensionsToExclude.RemoveAll(string.IsNullOrEmpty);

            ignoreAssetsInSpecificallyNamedFolders = ignoreAssetsInSpecificallyNamedFolders.Distinct().ToList();
            ignoreAssetsInSpecificallyNamedFolders.RemoveAll(string.IsNullOrEmpty);

            specificAssetsAndFoldersToIgnore = specificAssetsAndFoldersToIgnore.Distinct().ToList();

            assetBundlesToInclude = assetBundlesToInclude.Distinct().ToList();
            assetBundlesToInclude.RemoveAll(string.IsNullOrEmpty);
        }
    }
}
