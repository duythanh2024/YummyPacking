using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnusedAssetsFinder.Editor.Util
{
    public static class EditorWindowUtil
    {
        /// <summary>
        /// Returns the path the project window is looking at
        /// </summary>
        /// <returns>Path of the selected folder in the project window</returns>
        public static string GetSelectedPathInProjectWindow()
        {
            var projectBrowser = GetProjectEditorWindow();

            var selectedPathMethodInfo = projectBrowser.GetType()
                                                       .GetMethod("GetActiveFolderPath", BindingFlags.NonPublic | BindingFlags.Instance);

            if (selectedPathMethodInfo == null)
                return null;

            var selectedPath = (string) selectedPathMethodInfo.Invoke(projectBrowser, null);

            return selectedPath;
        }

        /// <summary>
        /// Sets a string in the project windows search bar
        /// </summary>
        /// <param name="query">Query to put in the project search bar</param>
        public static void SetSearchInProjectWindow(string query)
        {
            var projectBrowser = GetProjectEditorWindow();

            var getAllBrowsersMethodInfo = projectBrowser.GetType()
                                                         .GetMethod("GetAllProjectBrowsers", BindingFlags.Public | BindingFlags.Static);

            if (getAllBrowsersMethodInfo == null)
                return;

            var allBrowsers = (IEnumerable) getAllBrowsersMethodInfo.Invoke(null, null);

            foreach (var browser in allBrowsers)
            {
                var type = browser.GetType();

                var setSearchMethod = type.GetMethod("SetSearch", new[]
                                                                  {
                                                                      typeof(string)
                                                                  });

                setSearchMethod?.Invoke(browser, new object[]
                                                 {
                                                     query
                                                 });
            }
        }

        private static EditorWindow GetProjectEditorWindow()
        {
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            return windows.First(window => window.titleContent.text.Equals("Project"));
        }
    }
}
