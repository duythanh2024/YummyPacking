using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnusedAssetsFinder.Editor.Extensions;

namespace UnusedAssetsFinder.Editor
{
    public sealed class ProjectSettingScrapper
    {
        private static readonly string ProjectSettingsPath = Application.dataPath.Replace("/Assets", "/ProjectSettings/");

        private const string GUIDRegexPattern = "guid: ([A-Za-z0-9]+)";

        public static List<string> GetAllDependencyPaths()
        {
            var directoryInfo = new DirectoryInfo(ProjectSettingsPath);

            var filePaths = directoryInfo.GetFiles();

            var tasks = new List<string>();

            foreach (var filePath in filePaths)
            {
                tasks.AddRange(GetGUIDsFromFile(filePath.ToString()));
            }

            return tasks;
        }


        private static List<string> GetGUIDsFromFile(string filePath)
        {
            var contents = File.ReadAllText(filePath);
            
            var guidMatches = Regex.Matches(contents, GUIDRegexPattern)
                                   .ToStringArray();

            var paths = new List<string>();

            foreach (var guid in guidMatches)
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            return paths;
        }
    }
}
