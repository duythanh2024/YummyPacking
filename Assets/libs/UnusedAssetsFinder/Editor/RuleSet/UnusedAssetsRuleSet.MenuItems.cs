using UnityEditor;
using UnusedAssetsFinder.Editor.Util;

namespace UnusedAssetsFinder.Editor.RuleSet
{
    public partial class UnusedAssetsRuleSet
    {
        [MenuItem("Assets/Unused Assets Finder/New Rule Set", false, 1)]
        private static void CreateNewRuleSet()
        {
            var newRuleSet = CreateDefaultRuleSet(Strings.FileNames.DefaultRuleSetFilename);
            
            var currentProjectWindowPath = EditorWindowUtil.GetSelectedPathInProjectWindow();
            
            AssetDatabase.CreateAsset(newRuleSet, currentProjectWindowPath + "/" + Strings.FileNames.DefaultRuleSetFilename + ".asset");
            AssetDatabase.Refresh();
        }
    }
}
