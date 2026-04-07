using UnityEditor.IMGUI.Controls;

namespace UnusedAssetsFinder.Editor.TreeView {

    /// <summary>
    /// Custom class to inherit from TreeViewItem that will hold the associated backend data to enable rebuilding and dynamic changing of the backend data
    /// </summary>
    public class AssetTreeViewItem<T> : TreeViewItem where T : AssetTreeElement {

        /// <summary>
        /// The backend data associated with the TreeViewItem row
        /// </summary>
        public T data;

        /// <summary>
        /// Construct a new TreeViewItem row and assign is corresponding backend data
        /// </summary>
        public AssetTreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName) {
            this.data = data;
        }
    }
}
