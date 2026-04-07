using UnityEngine;

namespace UnusedAssetsFinder.Editor
{
    public static partial class Strings
    {
        public static class Paths //TODO Nick: Clean this up, this is not very clean
        {
            /// <summary>
            /// Converts a relative asset path to the full system path
            /// </summary>
            /// <param name="relativePath">Relative Path</param>
            /// <returns>Full Path</returns>
            public static string ConvertRelativePathToFullPath(string relativePath)
            {
                //if it doesn't start with "Assets"     then its not a relative path
                if (!relativePath.StartsWith("Assets"))
                {
                    return relativePath;
                }
            
                var projectPath = Application.dataPath.Replace("Assets", "");
                var fullPath    = projectPath + relativePath;
                return fullPath;
            }

            /// <summary>
            /// Converts a full path to a project relative path
            /// </summary>
            /// <param name="fullPath">Full Path</param>
            /// <returns>Relative Path</returns>
            public static string ConvertFullPathToRelativePath(string fullPath)
            {
                //if the Path starts with "Assets/" then the path is already relative
                if (fullPath.StartsWith("Assets/"))
                {
                    return fullPath;
                }

                var assetIndex = -1;
                if (assetIndex == -1)
                {
                    assetIndex = fullPath.IndexOf("Assets/");
                }

                if (assetIndex == -1)
                {
                    assetIndex = fullPath.IndexOf("Assets\\");
                }

                var relativePath = fullPath.Substring(assetIndex);

                return relativePath;
            }
        }
    }
}
