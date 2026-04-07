using UnityEditor;
using UnusedAssetsFinder.Editor.RuleSet;
using UnusedAssetsFinder.Editor.Util;

namespace UnusedAssetsFinder.Editor.MenuItems
{
    public static class ShowAllRuleSetsInProjectWindow
    {
        /// <summary>
        /// String to put in the project search bar
        /// </summary>
        private static readonly string SearchString = $"t:{nameof(UnusedAssetsRuleSet)}";

        /// <summary>
        /// Finds all rule sets in the project
        /// </summary>
        [MenuItem("Tools/Unused Assets Finder/Find All Rule Sets In Project", false, 102)]
        private static void FindAllRuleSets()
        {
            EditorWindowUtil.SetSearchInProjectWindow(SearchString);
        }
    }
}
