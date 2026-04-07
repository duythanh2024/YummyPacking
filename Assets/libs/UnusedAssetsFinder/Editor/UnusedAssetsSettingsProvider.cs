using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnusedAssetsFinder.Editor.Util;

namespace UnusedAssetsFinder.Editor
{
    /// <summary>
    /// Wrapper class to hold the different version compilations for the plugin preferences as well as shared methods and variables
    /// </summary>
    public class UnusedAssetsPreferences
    {
        /// <summary>
        /// Determines in the preferences have been loaded
        /// If they have not, then the will be looked for in the preferences file and loaded
        /// If they are not found, then they will be generated using default values
        /// </summary>
        private static bool prefsLoaded = false;

        /// <summary>
        /// Should the Editor Window be docked next to a given window, or be drawn as a floating window
        /// </summary>
        [Tooltip(Strings.Tooltips.DockWindowNextToAGivenWindowTooltip)]
        private static bool dockWindowNextToAGivenWindow = true;

        /// <summary>
        /// Holds the windows to dock next to as concatenated list separated by commas
        /// </summary>
        private static string concatenatedListOfWindows = "";

        /// <summary>
        /// The reorderable list element that displays the given windows to dock next to
        /// </summary>
        private static ReorderableList windowsToDockNextToReorderableList;

        /// <summary>
        /// A list of windows that the UnusedAssetsEditorWindow may dock next to if an instance of the window exists
        /// </summary>
        [Tooltip(Strings.Tooltips.WindowsToDockNextToReorderableTooltip)]
        private static List<string> windowsToDockNextTo = new List<string> {
            "UnityEditor.SceneView",
            "UnityEditor.GameView",
            "UnityEditor.ProjectBrowser",
            "UnityEditor.ConsoleWindow",
            "UnityEditor.SceneHierarchyWindow",
            "UnityEditor.InspectorWindow",
        };

        /// <summary>
        /// Default windows to dock next top
        /// </summary>
        private static List<string> defaultWindowsToDockNextTo = new List<string> {
                                                                                      "UnityEditor.SceneView",
                                                                                      "UnityEditor.GameView",
                                                                                      "UnityEditor.ProjectBrowser",
                                                                                      "UnityEditor.ConsoleWindow",
                                                                                      "UnityEditor.SceneHierarchyWindow",
                                                                                      "UnityEditor.InspectorWindow",
                                                                                  };

        /// <summary>
        /// Create and return a new ReorderableList that can be drawn later on
        /// </summary>
        private static ReorderableList CreateList(List<string> elements, string headerName, string tooltip)
        {
            var list = new ReorderableList(elements, typeof(string), true, true, true, true);

            var boldLabel = new GUIStyle();
            boldLabel.fontStyle = FontStyle.Bold;

            list.drawHeaderCallback = rect =>
            { //Callback to draw the contents of the list header
                EditorGUI.LabelField(rect, new GUIContent(headerName, tooltip), boldLabel);
            };

            list.drawElementCallback = (rect, index, active, focused) =>
            { //Callback to draw the contents of each element
                list.list[index] = EditorGUI.TextField(rect, list.list[index].ToString());
            };

            list.onAddCallback = (theReorderableList) =>
            {
                list.index = list.list.Add("");
            };
            list.onChangedCallback = (theReorderableList) =>
            {
                windowsToDockNextTo = windowsToDockNextToReorderableList.list as List<string>;
                concatenatedListOfWindows = string.Join(",", windowsToDockNextTo.ToArray());
                EditorPrefs.SetString(nameof(concatenatedListOfWindows) + "Key", concatenatedListOfWindows);
            };
            return list;
        }

        /// <summary>
        /// Return whether the user has chosen a specific window to draw next to
        /// </summary>
        /// <returns></returns>
        public static bool ShouldDockNextToASpecificWindow()
        {
            return EditorPrefs.GetBool(nameof(dockWindowNextToAGivenWindow) + "Key", false);
        }

        /// <summary>
        /// Return the list of possible windows that the plugin can be drawn next to
        /// </summary>
        /// <returns>List of windows that can be docked next to</returns>
        public static List<string> GetListOfWindowsToDockNextTo()
        {
            return EditorPrefs.GetString(nameof(concatenatedListOfWindows) + "Key", string.Join(",", windowsToDockNextTo.ToArray())).Split(',').ToList();
        }

        /// <summary>
        /// Draw the content of the preferences tab
        /// </summary>
        public static void DrawContent()
        {
            if (!prefsLoaded)
            {
                dockWindowNextToAGivenWindow = EditorPrefs.GetBool(nameof(dockWindowNextToAGivenWindow) + "Key", false);
                concatenatedListOfWindows = EditorPrefs.GetString(nameof(concatenatedListOfWindows) + "Key", string.Join(",", windowsToDockNextTo.ToArray()));
                windowsToDockNextTo = concatenatedListOfWindows.Split(',').ToList();
                prefsLoaded = true;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Strings.RuleSet.PropertyDrawer.DockWindowNextToAGivenWindowText);
            dockWindowNextToAGivenWindow = EditorGUILayout.Toggle(dockWindowNextToAGivenWindow);
            EditorGUILayout.EndHorizontal();

            if (windowsToDockNextToReorderableList == null)
            {
                windowsToDockNextToReorderableList = CreateList(windowsToDockNextTo, Strings.RuleSet.PropertyDrawer.WindowsToDockReorderableHeader, Strings.Tooltips.WindowsToDockNextToReorderableTooltip);
            }

            if (dockWindowNextToAGivenWindow)
            {
                windowsToDockNextToReorderableList.DoLayoutList();

                if (GUILayout.Button("Add All Editor Windows"))
                {
                    windowsToDockNextTo.AddRange(defaultWindowsToDockNextTo);
                    var allEditorTypes = GetAllEditorWindowTypes.GetEditorWindowTypes();

                    foreach (var editorType in allEditorTypes)
                    {
                        var windowName = editorType.FullName;
                        if (windowsToDockNextTo.Contains(windowName)) continue;
                        windowsToDockNextTo.Add(windowName);
                    }

                    windowsToDockNextTo = windowsToDockNextTo.Distinct().ToList();
                }
            }

            //saves list to editor prefs
            if (GUI.changed)
            {
                EditorPrefs.SetBool(nameof(dockWindowNextToAGivenWindow) + "Key", dockWindowNextToAGivenWindow);
                windowsToDockNextTo = windowsToDockNextToReorderableList.list as List<string>;
                concatenatedListOfWindows = string.Join(",", windowsToDockNextTo.ToArray());
                EditorPrefs.SetString(nameof(concatenatedListOfWindows) + "Key", concatenatedListOfWindows);
            }
        }

#if UNITY_2018_3_OR_NEWER
        /// <summary>
        /// Create a Settings Preference menu in Unity 2018
        /// </summary>
        public class UnusedAssetsSettingsProvider : SettingsProvider {

            /// <summary>
            /// Default constructor for the class
            /// </summary>
            /// <param name="path">The path at which the menu item will appear</param>
            /// <param name="scope">Whether it will appear in the Project Settings menu or the Preferences menu</param>
            /// <returns></returns>
            public UnusedAssetsSettingsProvider(string path, SettingsScope scope = SettingsScope.User) : base(path, scope) { }

            /// <summary>
            /// The call to draw the elements of the tab
            /// </summary>
            /// <param name="searchContext"></param>
            public override void OnGUI(string searchContext)
            {
                DrawContent();
            }

            /// <summary>
            /// This function registers the SettingsProvider to the system and tells it that it is a menu item to be drawn
            /// </summary>
            /// <returns></returns>
            [SettingsProvider]
            public static SettingsProvider CreateSettingsProvider()
            {
                var provider = new UnusedAssetsSettingsProvider("Preferences/Unused Assets Finder", SettingsScope.User);
                return provider;
            }
        }

#else //!UNITY_2018_3_OR_NEWER

        /// <summary>
        /// Create a Preferences Setting menu in Unity 2017
        /// </summary>
        public class UnusedAssetsSettingsProvider : MonoBehaviour
        {
            /// <summary>
            /// Draw a GUI element in the Preferences menu for the plugin
            /// </summary>
            [PreferenceItem("Unused Assets Finder")]
            public static void PreferencesGUI()
            {
                DrawContent();
            }
        }
#endif
    }
}
