using UnityEditor;
using UnityEngine;

namespace UnusedAssetsFinder.Editor
{
    internal class UnusedAssetsIssueReporter : MonoBehaviour
    {
        [MenuItem("Tools/Unused Assets Finder/Report Issue", false, 300)]
        private static void OpenIssuePage()
        {
            const string URL = "https://bitbucket.org/UnusedAssets/unusedassetsplugin/issues/new";
            Application.OpenURL(URL);
        }
    }
}
