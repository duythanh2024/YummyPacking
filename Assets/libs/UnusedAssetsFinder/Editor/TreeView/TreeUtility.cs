using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace UnusedAssetsFinder.Editor.TreeView {
    /// <summary>
    /// A Utility class to hold useful TreeView related methods
    /// </summary>
    public static class TreeUtility {

        /// <summary>
        /// Given a list of paths for Unused assets in the project
        /// Convert them into a list of AssetTreeElements, one for each unique Asset and create a unique root element
        /// </summary>
        public static IList<AssetTreeElement> RefineData(List<string> allUnusedAssets) {

            int elementId = 1;
            IList<AssetTreeElement> treeElements = new List<AssetTreeElement>(allUnusedAssets.Count);

            AssetTreeElement lastAssetTreeElement = null;
            string subPathAgg;

            var root = new AssetTreeElement(0, "Root", -1, "Root");

            treeElements.Add(root);

            foreach(string path in allUnusedAssets) { //foreach unused asset
                subPathAgg = "";
                string[] subPaths = path.Split('/'); //Split the path of the current unusedAsset
                for(int i = 0; i < subPaths.Length; i++) { //for all the different sub paths
                    string subPath = subPaths[i];
                    subPathAgg += subPath; //Aggregate the different sub paths together and add a '/' if it is not the last sub path
                    subPathAgg += i == subPaths.Length - 1 ? "" : "/"; //Aggregate the different sub paths together and add a '/' if it is not the last sub path
                    AssetTreeElement existingElement = treeElements.FirstOrDefault(element => element.path.Equals(subPathAgg)); //Search the list of already existing elements to find if any match the current path being queried
                    if(existingElement == null) { //if no elements match the current path, then this is a newElement
                        var newElement = new AssetTreeElement(elementId, subPath, subPathAgg);
                        if(lastAssetTreeElement == null) { //if the last element processed is null
                            root.AddChild(newElement); //add this directly to the hidden root element
                            lastAssetTreeElement = newElement; //set the last element processed to be the newly made element
                        }
                        else { //the last element processed is not null
                            lastAssetTreeElement.AddChild(newElement); //add the new element to the last processed element
                            lastAssetTreeElement = newElement; //set the last element processed to be the newly made element
                        }
                        treeElements.Add(newElement); //add the new element to a list of all existing elements
                        elementId++; //increment the unique element id
                    }
                    else { //an element with this path already exists
                        lastAssetTreeElement = existingElement; //set the last element processed to be the already existing element
                    }
                    //subPathAgg += i == subPaths.Length - 1 ? "" : "/"; //Aggregate the different sub paths together and add a '/' if it is not the last sub path
                }
                lastAssetTreeElement = null;
            }

            return treeElements;
        }

        /// <summary>
        /// Take the backend data in a complete tree model and flatten it down into a single list
        /// </summary>
        public static void TreeDataToList<T>(T root, IList<T> result) where T : AssetTreeElement {
            if(result == null) {
                throw new NullReferenceException("The input 'IList<T> result' list is null");
            }
            result.Clear();

            Stack<T> stack = new Stack<T>();
            stack.Push(root);

            while (stack.Count > 0) {
                T current = stack.Pop();
                result.Add(current);

                if(current.children != null && current.children.Count > 0) {
                    for(int i = current.children.Count - 1; i >= 0; i--) {
                        stack.Push((T) current.children[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Take the rows of a TreeView and flatten them into a single list
        /// </summary>
        /// <param name="root"></param>
        /// <param name="result"></param>
        public static void TreeItemToList(TreeViewItem root, IList<TreeViewItem> result) {

            if(root == null) {
                throw new NullReferenceException("root");
            }
            if(result == null) {
                throw new NullReferenceException("result");
            }

            result.Clear();

            if(root.children == null) {
                return;
            }

            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            for(int i = root.children.Count - 1; i >= 0; i--) {
                stack.Push(root.children[i]);
            }

            while (stack.Count > 0) {
                TreeViewItem current = stack.Pop();
                result.Add(current);

                if(current.hasChildren && current.children[0] != null) {
                    for(int i = current.children.Count - 1; i >= 0; i--) {
                        stack.Push(current.children[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Find any common ancestors of a list of given elements
        /// </summary>
        /// <param name="elements">The list of elements to find common ancestors in</param>
        public static IList<T> FindCommonAncestors<T>(IList<T> elements) where T : AssetTreeElement {
            if(elements.Count == 1) {
                return new List<T>(elements);
            }

            List<T> result = new List<T>(elements);
            result.RemoveAll(element => IsChildOf(element, elements));
            return result;
        }

        /// <summary>
        /// Identifies if an element is a direct child of another element
        /// </summary>
        private static bool IsChildOf<T>(T child, IList<T> elements) where T : AssetTreeElement {
            while (child != null) {
                child = (T) child.parent;
                if(elements.Contains(child)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Utility method to check if any of the descendants of a given element match the search criteria
        /// </summary>
        /// <param name="elementToSearchFrom">The element to search from</param>
        /// <param name="searchQuery">The search query to check against</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if a descendant is found that matches, False if none found</returns>
        public static bool DoDescendantsMatchSearch<T>(T elementToSearchFrom, string searchQuery) where T : AssetTreeElement {

            if(!elementToSearchFrom.hasChildren) { return false; } //if the given element has no children, then return false

            foreach(T child in elementToSearchFrom.children) { //foreach child
                if(child.name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0) { //check if its name matches
                    return true; //if it does, return true
                }

                if(DoDescendantsMatchSearch(child, searchQuery)) { //else if the child's name doesn't match, check if any of its descendants do
                    return true;
                }
            }

            return false;
        }
    }
}
