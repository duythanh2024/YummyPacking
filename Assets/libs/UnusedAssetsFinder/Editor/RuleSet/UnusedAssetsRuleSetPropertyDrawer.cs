using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnusedAssetsFinder.Editor.RuleSet
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnusedAssetsRuleSet))]
    public class UnusedAssetsRuleSetPropertyDrawer : UnityEditor.Editor
    {
        private UnusedAssetsRuleSet instance;

        private ReorderableList assetExtensionsToExcludeList;
        private ReorderableList ignoreAssetsInSpecificallyNamedFoldersList;
        private ReorderableList specificAssetsAndFoldersToIgnore;
        private ReorderableList assetBundlesToIncludeList;

        private void OnEnable()
        {
            assetExtensionsToExcludeList               = CreateList(nameof(instance.assetExtensionsToExclude),               Strings.RuleSet.PropertyDrawer.AssetExtensionsReorderableHeader,                        Strings.Tooltips.AssetExtensionsReorderableTooltip);
            ignoreAssetsInSpecificallyNamedFoldersList = CreateList(nameof(instance.ignoreAssetsInSpecificallyNamedFolders), Strings.RuleSet.PropertyDrawer.IgnoreAssetsInSpecificallyNamedFoldersReorderableHeader, Strings.Tooltips.IgnoreAssetsInSpecificallyNamedFoldersTooltip);
            specificAssetsAndFoldersToIgnore           = CreateList(nameof(instance.specificAssetsAndFoldersToIgnore),       Strings.RuleSet.PropertyDrawer.SpecificAssetsAndFoldersIgnoreReorderableHeader,         Strings.Tooltips.SpecificAssetsAndFoldersToIgnoreReorderableTooltip);
            assetBundlesToIncludeList                  = CreateList(nameof(instance.assetBundlesToInclude),                  Strings.RuleSet.PropertyDrawer.AssetBundlesReorderableHeader,                           Strings.Tooltips.AssetBundlesReorderableTooltip);
        }

        public override void OnInspectorGUI()
        {
            instance = (UnusedAssetsRuleSet) target;

            DrawReorderableLists();
            DrawButtons();

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private void DrawReorderableLists()
        {
            assetExtensionsToExcludeList.DoLayoutList();
            ignoreAssetsInSpecificallyNamedFoldersList.DoLayoutList();
            specificAssetsAndFoldersToIgnore.DoLayoutList();
            assetBundlesToIncludeList.DoLayoutList();
        }

        private void DrawButtons()
        {
            if (GUILayout.Button(Strings.RuleSet.PropertyDrawer.PruneButton))
                instance.PruneEntries();
        }

        /// <summary>
        /// Create and return a new ReorderableList that can be drawn later on
        /// </summary>
        private ReorderableList CreateList(string propName, string headerName, string tooltip)
        {
            var prop = serializedObject.FindProperty(propName);

            var list = new ReorderableList(serializedObject,
                                           prop,
                                           draggable: true,
                                           displayHeader: true,
                                           displayAddButton: true,
                                           displayRemoveButton: true);

            var boldLabel = new GUIStyle
                            {
                                fontStyle = FontStyle.Bold
                            };

            list.drawHeaderCallback = rect =>
                                      {
                                          //Callback to draw the contents of the list header
                                          EditorGUI.LabelField(rect, new GUIContent(headerName, tooltip), boldLabel);
                                      };

            list.drawElementCallback = (rect, index, active, focused) =>
                                       {
                                           //Callback to draw the contents of each element
                                           var element = list.serializedProperty.GetArrayElementAtIndex(index);
                                           EditorGUI.PropertyField(rect, element, GUIContent.none, false);
                                       };

            return list;
        }
    }
}
