using System.Linq;
using System.Text.RegularExpressions;

namespace UnusedAssetsFinder.Editor.Extensions
{
    public static class MatchCollectionExtensions
    {
        public static string[] ToStringArray(this MatchCollection matchCollection, int groupIndex = 1)
        {
            return matchCollection
                   .Cast<Match>()
                   .Select(m => m.Groups[groupIndex].Value)
                   .ToArray();
        }
    }
}
