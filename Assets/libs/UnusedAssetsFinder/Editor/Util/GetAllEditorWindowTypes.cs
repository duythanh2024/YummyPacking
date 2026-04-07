using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace UnusedAssetsFinder.Editor.Util
{
    public static class GetAllEditorWindowTypes
    {
        /// <summary>
        /// Gets all editor window types
        /// <para>Based on this Unity Forum post https://forum.unity.com/threads/opening-the-built-in-windows-inspector-scene-etc-without-hard-coding-a-menu-path.546617</para>
        /// </summary>
        /// <returns>List of editor window types</returns>
        public static List<Type> GetEditorWindowTypes()
        {
            var baseType          = typeof(EditorWindow);
            var requiredAttribute = baseType.Assembly.GetType("UnityEditor.EditorWindowTitleAttribute");

            var assemblies    = AppDomain.CurrentDomain.GetAssemblies();
            var typesToReturn = new List<Type>();

            foreach (var assembly in assemblies)
            {
                var types                  = assembly.GetTypes();
                var windowTypes            = types.Where(type => baseType.IsAssignableFrom(type));
                var typesThatHaveAttribute = windowTypes.Where(type => type.GetCustomAttributes(requiredAttribute, true).Length > 0);

                typesToReturn.AddRange(typesThatHaveAttribute);
            }

            return typesToReturn.Distinct()
                                .ToList();
        }
    }
}
