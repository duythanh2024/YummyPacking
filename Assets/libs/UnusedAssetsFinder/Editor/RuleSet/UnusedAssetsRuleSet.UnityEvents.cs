using UnityEditor;

namespace UnusedAssetsFinder.Editor.RuleSet
{
    public partial class UnusedAssetsRuleSet
    {
        private void OnValidate()
        {
            ConvertSpecificAssetsAndFoldersToIgnoreObjectGuidsToPaths();
        }

        private void ConvertSpecificAssetsAndFoldersToIgnoreObjectGuidsToPaths()
        {
            specificAssetsAndFoldersToIgnorePaths.Clear();
            foreach (var asset in specificAssetsAndFoldersToIgnore)
            {
                var path = AssetDatabase.GetAssetPath(asset);
                specificAssetsAndFoldersToIgnorePaths.Add(path);
            }
        }
    }
}
