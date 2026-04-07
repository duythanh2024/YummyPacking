using System;
using System.Collections.Generic;
using System.Linq;

namespace UnusedAssetsFinder.Editor.TreeView {

    /// <summary>
    /// The backend data structure composed of several AssetTreeElements to give structure to the Unused Assets found by the plugin
    /// </summary>
    public class TreeModel<T> where T : AssetTreeElement {

        /// <summary>
        /// The root element of the TreeModel - it is hidden to mimick the front end TreeView
        /// </summary>
        public T root;

        /// <summary>
        /// An event which is executed whenever an element of the backend data changes - used to trigger the redrawing of the front end TreeVIew
        /// </summary>
        public event Action modelChanged;

        /// <summary>
        /// A list of all backend data elements
        /// </summary>
        private IList<T> data;

        /// <summary>
        /// Construct a new TreeModel with given data
        /// </summary>
        /// <param name="data"></param>
        public TreeModel(IList<T> data) {
            Init(data);
        }

        /// <summary>
        /// Initialise the data and calculate file sizes of elements found
        /// </summary>
        public void Init(IList<T> newData) {
            if(newData == null) {
                throw new ArgumentNullException(nameof(newData), "Input data is null. Ensure input is a non-null list.");
            }

            data = newData;
            root = data[0];

            foreach(var child in root.children) {
                child.CalculateFileSizes();
            }
        }

        /// <summary>
        /// Find the first data element in the tree which matches the id
        /// </summary>
        public T Find(int id) {
            return data.FirstOrDefault(element => element.id == id);
        }

        /// <summary>
        /// Get all the ancestors - up to and including the hidden root - of a given element
        /// </summary>
        public IList<int> GetAncestors(int id) {
            var parents = new List<int>();
            AssetTreeElement current = Find(id);

            if(current != null) {
                while (current.parent != null) {
                    parents.Add(current.parent.id);
                    current = current.parent;
                }
            }
            return parents;
        }

        /// <summary>
        /// Get a list of all descendents from a given element that also have children
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IList<int> GetDescendantsThatHaveChildren(int id) {
            T searchFrom = Find(id);
            if(searchFrom != null) {
                return GetParentsBelowStackBased(searchFrom);
            }
            return new List<int>();
        }

        /// <summary>
        /// /// Get a list of all descendents from a given element that are parents
        /// </summary>
        private IList<int> GetParentsBelowStackBased(AssetTreeElement searchFrom) {
            Stack<AssetTreeElement> stack = new Stack<AssetTreeElement>();
            stack.Push(searchFrom);

            var parentsBelow = new List<int>();
            while (stack.Count > 0) {
                AssetTreeElement current = stack.Pop();
                if(current.hasChildren) {
                    parentsBelow.Add(current.id);
                    foreach(AssetTreeElement child in current.children) {
                        stack.Push(child);
                    }
                }
            }

            return parentsBelow;
        }

        /// <summary>
        /// Remove a list of elements from the tree view and any of their descendants
        /// </summary>
        public void RemoveElements(IList<T> elements) {

            foreach(var element in elements) {
                if(element == root) {
                    throw new ArgumentException("It is not allowed to remove the root element");
                }
            }

            var commonAncestors = TreeUtility.FindCommonAncestors(elements);

            foreach(var element in commonAncestors) {
                element.parent.children.Remove(element);
                element.parent = null;
            }

            TreeUtility.TreeDataToList(root, data);

            Changed();
        }

        ///<summary>
        /// Called whenever the contents of the TreeModel is changed either via adding elements or removing them
        /// This causes the Reload method to be called to redraw the TreeView
        /// </summary>
        private void Changed() {
            var modelChangedEvent = modelChanged;
            if(modelChangedEvent != null) modelChangedEvent.Invoke();
        }

        /// <summary>
        /// Get the total number of selected elements - includes both files and folders
        /// </summary>
        public int GetNumSelectedItems() {
            return GetAllSelectedItems().Count;
        }

        /// <summary>
        /// Get the file size of all selected elements
        /// Called by UnityEvent invoke 
        /// </summary>
        public long GetSelectedTotalFileSizeRaw() {
            var selectedItems = GetAllSelectedItems();
            selectedItems = selectedItems.Where(item => !item.hasChildren && item != root).ToList(); //Remove selected folder elements
            long totalRawFileSize = 0;
            foreach(var item in selectedItems) {
                totalRawFileSize += item.rawFileSize;
            }
            return totalRawFileSize;
        }

        /// <summary>
        /// Get all the selected items - both folders and files
        /// </summary>
        /// <returns></returns>
        private List<T> GetAllSelectedItems() {
            return data.Where(item => item.GetShouldDelete() && item != root).ToList();
        }
    }
}
