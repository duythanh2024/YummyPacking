using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnusedAssetsFinder.Editor.Util;

namespace UnusedAssetsFinder.Editor.Packaging
{
    public static class ExportPackage
    {
        /// <summary> 
        /// Export a list of assets to a .unitypackage
        /// </summary>
        public static async Task Export(List<string> pathsToExport)
        {
            var productName     = ObjectNames.NicifyVariableName(Application.productName);
            var packageFilename = $"{productName} Unused Assets {DateTime.Now:yyyy-dd-MMM HH-mm-ss}";
            var exportPath      = EditorUtility.SaveFilePanel(Strings.MessageBoxElements.ExportPackageSavePanelTitle, "", packageFilename, "unitypackage");

            if (string.IsNullOrEmpty(exportPath))
                return;

            string[] paths = pathsToExport.Where(path => !path.EndsWith("/")).ToArray();

            AssetDatabase.ExportPackage(paths, exportPath, ExportPackageOptions.Interactive);

            while (!File.Exists(exportPath))
                await Task.Delay(DefinedTimeSpans.OneFrame);
        }
    }
}
