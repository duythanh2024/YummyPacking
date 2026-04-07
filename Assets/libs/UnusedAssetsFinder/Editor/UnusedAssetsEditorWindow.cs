using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnusedAssetsFinder.Editor.Exceptions;
using UnusedAssetsFinder.Editor.Popups;
using UnusedAssetsFinder.Editor.RuleSet;
using UnusedAssetsFinder.Editor.TreeView;
using UnusedAssetsFinder.Editor.Util;

namespace UnusedAssetsFinder.Editor
{
    /// <summary>
    /// UI / Drawing for Unused Assets
    /// </summary>
    public class UnusedAssetsEditorWindow : EditorWindow
    {
        #region Variables
        /// <summary>
        /// All unused assets paths - unrefined data
        /// </summary>
        private List<string> allUnusedAssets = new List<string>();

        /// <summary>
        /// Have the unused assets been found
        /// </summary>
        public static bool unusedAssetsFound;

        /// <summary>
        /// Rule set set in the object field
        /// </summary>
        public static UnusedAssetsRuleSet ruleSetToUse;

        /// <summary>
        /// Was the rule set null last OnGUI call?
        /// Used to tell if the rule set was deleted from the window 
        /// </summary>
        private static bool wasRuleSetNullLastOnGUI;

        /// <summary> 
        /// SerializeField is used to ensure the view state is written to the window layout file.
        /// This means that the state survives restarting Unity as long as the window
        /// is not closed. If the attribute is omitted then the state is still serialized/deserialized.
        /// </summary> 
        [SerializeField] private TreeViewState treeViewState;

        /// <summary>
        /// A cache of the state of the Multiple columns of the TreeView
        /// </summary>
        [SerializeField] private MultiColumnHeaderState multiColumnHeaderState;

        /// <summary> 
        /// Tree view that will draw the unused asset data
        /// </summary> 
        private UnusedAssetsTreeView unusedAssetsTreeView;

        /// <summary>
        /// Search field above tree view
        /// </summary>
        private SearchField searchField;

        /// <summary> 
        /// Backend structure holding refined data
        /// </summary> 
        private TreeModel<AssetTreeElement> treeModel;

        /// <summary> 
        /// Scroll position for the TreeView scroll view section
        /// </summary> 
        private Vector2 scrollPos;

        #region Rects
        /// <summary> 
        /// Rect denoting the area of the top section
        /// </summary> 
        private Rect mainButtonRect
        {
            get { return new Rect(HORIZONTAL_PADDING, VERTICAL_PADDING, position.width - (HORIZONTAL_PADDING * 2), 20); }
        }

        /// <summary> 
        /// Rect denoting the area of the search bar
        /// </summary> 
        private Rect searchBarRect
        {
            get { return new Rect(HORIZONTAL_PADDING, mainButtonRect.y + mainButtonRect.height + VERTICAL_PADDING, position.width - (HORIZONTAL_PADDING * 2), 20); }
        }

        /// <summary> 
        /// Rect denoting the area of the main TreeView section
        /// </summary> 
        private Rect treeViewRect
        {
            get { return new Rect(HORIZONTAL_PADDING, searchBarRect.yMax + VERTICAL_PADDING, position.width - (HORIZONTAL_PADDING * 2), position.height - mainButtonRect.height - searchBarRect.height - fileDetailsRect.height - bottomAreaRect.height - (VERTICAL_PADDING * 6)); }
        }

        /// <summary> 
        /// Rect denoting the area of the bottom section
        /// </summary> 
        private Rect bottomAreaRect
        {
            get { return new Rect(HORIZONTAL_PADDING, position.height - 20 - VERTICAL_PADDING, position.width - (HORIZONTAL_PADDING * 2), 20); }
        }

        /// <summary>
        /// Rect denoting the area of the details section
        /// </summary>
        private Rect fileDetailsRect
        {
            get { return new Rect(HORIZONTAL_PADDING, position.height - 20 - (VERTICAL_PADDING * 2) - 20, position.width - (HORIZONTAL_PADDING * 2), 20); }
        }
        #endregion

        /// <summary> 
        /// Horizontal padding between different GUI elements
        /// </summary> 
        private const float HORIZONTAL_PADDING = 20f;

        /// <summary> 
        /// Vertical padding between different GUI elements
        /// </summary> 
        private const float VERTICAL_PADDING = 20f;

        /// <summary> 
        /// Preferred size of the window when opened
        /// </summary> 
        private static readonly Vector2 winPrefSize = new Vector2(600, 750);
        #endregion

        /// <summary>
        /// Rescans for new unused assets
        /// </summary>
        public static void ScanForUnusedAssets()
        {
            var instance = (UnusedAssetsEditorWindow)GetWindow(typeof(UnusedAssetsEditorWindow));

            if (ruleSetToUse == null) ruleSetToUse = UnusedAssetsRuleSet.CreateDefaultRuleSet();
            instance.FindUnusedAssets(ruleSetToUse);
            instance.Repaint();
        }

        /// <summary>
        /// Opens the window
        /// </summary>
        [MenuItem("Assets/Unused Assets Finder/Find Unused Assets with Rule Set", false, 11)]
        [MenuItem("Tools/Unused Assets Finder/Find Unused Assets _%#U", false, 11)] // Ctrl-Shift-U
        private static void Init()
        {
            var type = typeof(UnusedAssetsEditorWindow);

            var thisWindow = (UnusedAssetsEditorWindow)GetWindow(type); //find any open instance of the UnusedAssetsEditorWindow
            if (thisWindow != null) { thisWindow.Close(); } //close it so that it may be redrawn next to one of the desired windows

            EditorWindow window = null;

            if (UnusedAssetsPreferences.ShouldDockNextToASpecificWindow()) //if drawn next to a window
            { 
                var viableWindows = UnusedAssetsPreferences.GetListOfWindowsToDockNextTo();
                for (var i = 0; i < viableWindows.Count; i++) //cycle all the names of possible windows
                {
                    window = GetWindow<UnusedAssetsEditorWindow>(Strings.WindowElements.UnusedAssetsWindowTitle, false, typeof(UnityEditor.Editor).Assembly.GetType(viableWindows[i])); //find an open instance of a window and draw next to it
                    if (window != null) //if it finds one, then break and do not try again
                    {
                        break;
                    }
                }
            }

            if (window == null)
            { //if no matching window was found, or the user chooses a floating window
                window = GetWindowWithRect(type, new Rect(HORIZONTAL_PADDING, VERTICAL_PADDING * 2, winPrefSize.x, winPrefSize.y), false, Strings.WindowElements.UnusedAssetsWindowTitle);
            }

            window.Show();
            window.Focus();
        }

        /// <summary>
        /// A Validation Function for the given Asset Menu Item
        /// If the selected object in the project is of type UnusedAssetsRuleSet, then the MenuItem becomes enabled
        /// </summary>
        [MenuItem("Assets/Unused Assets Finder/Find Unused Assets with Rule Set", true, 11)]
        private static bool InitValidation()
        {
            if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(UnusedAssetsRuleSet))
            {
                ruleSetToUse = Selection.activeObject as UnusedAssetsRuleSet;
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// Called when the script is loaded or a value is changed in the
        /// inspector (Called in the editor only).
        /// Reset unusedAssetsFound to force re-draw of TreeView when button clicked
        /// </summary>
        private void OnValidate()
        {
            InvalidateSearch();
        }

        /// <summary>
        /// Called when this object is destroyed
        /// </summary>
        private void OnDestroy()
        {
            InvalidateSearch();
        }

        /// <summary>
        /// Called when this Editor window is drawn
        /// </summary>
        private void OnGUI()
        {
            DrawTopArea(mainButtonRect);

            if (unusedAssetsFound)
            {
                Handles.color = Color.black;

                DrawLineWithPadding(mainButtonRect.max.y);
                DrawLineWithPadding(searchBarRect.max.y);
                DrawLineWithPadding(treeViewRect.max.y);
                DrawLineWithPadding(fileDetailsRect.max.y);

                DrawSearchBar(searchBarRect);
                DrawTreeView(treeViewRect);
                DrawFileDetailsArea(fileDetailsRect);
                DrawBottomArea(bottomAreaRect);
                unusedAssetsTreeView.UpdateColumnWidths(treeViewRect.width);
            }
        }

        /// <summary>
        /// Draw a horizontal line at a given y position
        /// </summary>
        private void DrawLine(float y)
        {
            Handles.color = Color.black;
            Handles.DrawLine(new Vector3(0, y), new Vector3(position.width, y));
        }

        /// <summary>
        /// Draw a horizontal line at a given y position but add vertical padding to it
        /// </summary>
        private void DrawLineWithPadding(float y)
        {
            DrawLine(y + (VERTICAL_PADDING / 2f));
        }

        #region Drawing
        /// <summary> 
        /// Draw the Top section of the GUI
        /// </summary> 
        private void DrawTopArea(Rect rect)
        {
            GUILayout.BeginArea(rect);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(Strings.WindowElements.FindUnusedAssetsButton))
            {
                if (ruleSetToUse == null) ruleSetToUse = UnusedAssetsRuleSet.CreateDefaultRuleSet();
                FindUnusedAssets(ruleSetToUse);
                Repaint();
            }

            GUILayout.FlexibleSpace(); ///////////////////////////////

            //cache helps tell if the rule set has been changed between searches
            var cache = ruleSetToUse;
            ruleSetToUse = (UnusedAssetsRuleSet)EditorGUILayout.ObjectField(ruleSetToUse, typeof(UnusedAssetsRuleSet), false);
            if (cache != ruleSetToUse) unusedAssetsFound = false;

            //if the asset was deleted from the project tab and the UI hasn't updated
            if (!wasRuleSetNullLastOnGUI && ruleSetToUse == null) unusedAssetsFound = false;
            wasRuleSetNullLastOnGUI = cache == null;

            GUILayout.FlexibleSpace(); ///////////////////////////////

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        /// <summary> 
        /// Draw the Search bar GUI
        /// </summary> 
        private void DrawSearchBar(Rect rect)
        {
            unusedAssetsTreeView.searchString = searchField.OnGUI(rect, unusedAssetsTreeView.searchString);
            unusedAssetsTreeView.cachedSearchString = unusedAssetsTreeView.searchString;
        }

        /// <summary> 
        /// Draw the TreeView GUI
        /// </summary>
        private void DrawTreeView(Rect rect)
        {
            string cachedSearchString = unusedAssetsTreeView.searchString;
            unusedAssetsTreeView.searchString = string.Empty;
            unusedAssetsTreeView.OnGUI(rect);
            unusedAssetsTreeView.multiColumnHeader.ResizeToFit();
            unusedAssetsTreeView.searchString = cachedSearchString;
        }

        /// <summary>
        /// Draw the section of the window that holds the file details
        /// </summary>
        private void DrawFileDetailsArea(Rect rect)
        {
            int numTotalItems = unusedAssetsTreeView.GetRows().Count;
            var numSelectedItems = treeModel.GetNumSelectedItems();
            var totalFileSizeRaw = treeModel.GetSelectedTotalFileSizeRaw();
            ConvertedFileSize convertedFileSize = new ConvertedFileSize(totalFileSizeRaw);

            GUIStyle style = new GUIStyle(GUI.skin.label);

            var totalContent = new GUIContent(numTotalItems + " items");
            var selectedContent = new GUIContent(numSelectedItems + " items selected");
            var sizeContent = new GUIContent(convertedFileSize.ToString());

            var totalRect = new Rect(HORIZONTAL_PADDING, 0, style.CalcSize(totalContent).x, rect.height);
            var selectedRect = new Rect(totalRect.max.x + HORIZONTAL_PADDING, 0, style.CalcSize(selectedContent).x, rect.height);
            var sizeRect = new Rect(selectedRect.max.x + HORIZONTAL_PADDING, 0, style.CalcSize(sizeContent).x, rect.height);

            var versionContent = new GUIContent("Unused Assets Finder v" + UnusedAssetsVersion.Version);
            var versionSize = style.CalcSize(versionContent);
            var versionRect = new Rect((rect.max.x - HORIZONTAL_PADDING - HORIZONTAL_PADDING) - versionSize.x, 0, versionSize.x, rect.height);

            GUILayout.BeginArea(rect);
            EditorGUI.LabelField(totalRect, totalContent);
            EditorGUI.LabelField(selectedRect, selectedContent);
            EditorGUI.LabelField(sizeRect, sizeContent);
            EditorGUI.LabelField(versionRect, versionContent);
            GUILayout.EndArea();
        }

        /// <summary> 
        /// Draw the bottom section GUI
        /// </summary>
        private void DrawBottomArea(Rect rect)
        {
            GUILayout.BeginArea(rect);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (string.IsNullOrEmpty(unusedAssetsTreeView.searchString))
            {
                if (GUILayout.Button(Strings.WindowElements.ExpandAllButton))
                {
                    ToggleExpansionState(true);
                }
                if (GUILayout.Button(Strings.WindowElements.CollapseAllButton))
                {
                    ToggleExpansionState(false);
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(Strings.WindowElements.SelectAllButton))
                {
                    ToggleAllSelection(true);
                }
                if (GUILayout.Button(Strings.WindowElements.DeselectAllButton))
                {
                    ToggleAllSelection(false);
                }
            }
            else
            {
                if (GUILayout.Button(Strings.WindowElements.SelectAllSearchButton))
                {
                    ToggleSearchSelection(true, unusedAssetsTreeView.GetRows().ToList());
                }
                if (GUILayout.Button(Strings.WindowElements.DeselectAllSearchButton))
                {
                    ToggleSearchSelection(false, unusedAssetsTreeView.GetRows().ToList());
                }
            }

            GUILayout.FlexibleSpace();

            if (ruleSetToUse == null)
            {
                ruleSetToUse = UnusedAssetsRuleSet.CreateDefaultRuleSet();
            }

            if (GUILayout.Button(Strings.WindowElements.ExportAndDeleteSelectedButton))
            {
                UnusedAssets.DeleteSelectedUnusedAssets(unusedAssetsTreeView);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        #endregion

        #region Button Methods
        /// <summary> 
        /// Accesses the static func in UnusedAssets class to find all Unused assets
        /// If no assets found, return
        /// If Assets found make a TreeModel data structure and use it to create a TreeView
        /// </summary> 
        private void FindUnusedAssets(UnusedAssetsRuleSet ruleSet)
        {
            if (ruleSet == null) ruleSet = UnusedAssetsRuleSet.CreateDefaultRuleSet();

            try
            {
                UnusedAssets.FindUnusedAssets(ruleSet, ref allUnusedAssets);
            }
            catch (UnusedAssetsFinderBaseException)
            {
                return;
            }

            //if there are no unused assets found, show a popup and return
            if (allUnusedAssets == null || allUnusedAssets.Count == 0)
            {
                MessageDialogs.NoAssetsFound();
                return;
            }

            //sort alphabetically
            allUnusedAssets.Sort();

            //create a new tree view state
            if (treeViewState == null)
            {
                treeViewState = new TreeViewState();
            }

            treeModel = new TreeModel<AssetTreeElement>(TreeUtility.RefineData(allUnusedAssets));

            multiColumnHeaderState = UnusedAssetsTreeView.CreateDefaultMultiColumnHeaderState(treeViewRect.width);
            var multiColumnHeader = new MultiColumnHeader(multiColumnHeaderState);

            unusedAssetsTreeView = new UnusedAssetsTreeView(treeViewState, multiColumnHeader, treeModel);

            searchField = new SearchField();
            searchField.downOrUpArrowKeyPressed += unusedAssetsTreeView.SetFocusAndEnsureSelectedItem;
            unusedAssetsFound = true; //has to be last as this indicates to the GUI that there are valid objects to use to populate the GUI
        }

        /// <summary> 
        /// Toggle the expansion status of all TreeView elements
        /// </summary>
        private void ToggleExpansionState(bool shouldExpand)
        {
            if (shouldExpand)
            {
                unusedAssetsTreeView.ExpandAll();
            }
            else
            {
                unusedAssetsTreeView.CollapseAll();
            }
        }

        /// <summary> 
        /// Toggle the selection status of all TreeView elements
        /// </summary>
        private void ToggleAllSelection(bool value)
        {
            foreach (var assetTreeElement in treeModel.root.children)
            {
                assetTreeElement.SetShouldDelete(value, true, unusedAssetsTreeView.cachedSearchString);
            }
        }

        /// <summary> 
        /// Toggle the selection status for all the TreeViewItem elements found during the search
        /// </summary>
        private void ToggleSearchSelection(bool value, List<TreeViewItem> searchResultTreeViewItems)
        {
            foreach (var item in searchResultTreeViewItems)
            {
                //before there was no check on the cast, even though it should work, just putting this here just in case
                var assetItem = item as AssetTreeViewItem<AssetTreeElement>;
                if (assetItem == null) continue;
                assetItem.data.SetShouldDelete(value, false, unusedAssetsTreeView.cachedSearchString);
            }
        }
        #endregion

        /// <summary>
        /// Invalidates the search / removes the tree view and bottom buttons from the UI
        /// </summary>
        public static void InvalidateSearch()
        {
            unusedAssetsFound = false;
        }
    }
}
