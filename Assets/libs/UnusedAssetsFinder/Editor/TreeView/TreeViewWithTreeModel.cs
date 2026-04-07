using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnusedAssetsFinder.Editor.TreeView
{
    /// <summary>
    /// A class that is an instance of TreeView that is created using the data from a TreeModel and its AssetTreeElements
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TreeViewWithTreeModel<T> : UnityEditor.IMGUI.Controls.TreeView where T : AssetTreeElement
    {

        /// <summary>
        /// The backend data tree model structure associated with the treeview
        /// </summary>
        protected TreeModel<T> treeModel;

        /// <summary>
        /// A readonly list of rows that are displayed in the treeview
        /// </summary>
        private readonly List<TreeViewItem> rows = new List<TreeViewItem>(100);
        public string cachedSearchString = "";

        /// <summary>
        /// Construct a simple treeview with a treemodel and multiple colunmns
        /// </summary>
        /// <param name="state">The state of the treeview - expanded, with a search and others</param>
        /// <param name="multiColumnHeader">A multicolumn variable to declare that the treeview will have multiple columns</param>
        /// <param name="model">The backend data that will populate the treeview</param>
        public TreeViewWithTreeModel(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model) : base(state, multiColumnHeader)
        {
            Init(model);
        }

        /// <summary>
        /// Initialise the treeModel for this treeview and bind the event for it to reload whenever the data changes
        /// </summary>
        /// <param name="model"></param>
        private void Init(TreeModel<T> model)
        {
            treeModel = model;
            treeModel.modelChanged += Reload;
        }

        /// <summary>
        /// Creates the hidden root TreeViewItem that all other elements descend from
        /// </summary>
        /// <returns></returns>
        protected override TreeViewItem BuildRoot()
        {
            return new AssetTreeViewItem<T>(treeModel.root.id, -1, treeModel.root.name, treeModel.root);
        }

        /// <summary>
        /// Create the rest of the TreeViewItem rows in the TreeView
        /// Either through narrowing the rows to those that match a search query
        /// Or by drawing the entire TreeView providing the individual rows are expanded and they're children visible
        /// </summary>
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            if (treeModel.root == null)
            {
                Debug.LogException(new NullReferenceException(Strings.Exceptions.NullTreeModelRoot));
                return null;
            }

            rows.Clear();
            if (!string.IsNullOrEmpty(cachedSearchString))
            {
                Search(treeModel.root, cachedSearchString, rows);
            }
            else
            {
                if (treeModel.root.hasChildren)
                {
                    AddChildrenRecursively(treeModel.root, 0, rows);
                }
            }

            SetupParentsAndChildrenFromDepths(root, rows);

            return rows;
        }

        /// <summary>
        /// Search the TreeView to find any elements which contain the searchQuery
        /// </summary>
        /// <param name="searchFrom">The tree element to start the search from</param>
        /// <param name="searchQuery">The search query to compare against</param>
        /// <param name="result">The list to store any elements found to match the search, or any elements whose descendants match the search</param>
        public virtual void Search(T searchFrom, string searchQuery, List<TreeViewItem> result)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                throw new ArgumentException(Strings.Exceptions.InvalidSearchArg, nameof(searchQuery));
            }

            Stack<AssetTreeElement> stack = new Stack<AssetTreeElement>();
            foreach (AssetTreeElement element in searchFrom.children)
            {
                stack.Push(element);
            }
            while (stack.Count > 0)
            {
                AssetTreeElement current = stack.Pop();
                if (current.name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 || TreeUtility.DoDescendantsMatchSearch(current, searchQuery))
                {
                    result.Add(new AssetTreeViewItem<AssetTreeElement>(current.id, current.depth, current.name, current));
                }

                if (current.children != null && current.children.Count > 0)
                {
                    foreach (AssetTreeElement element in current.children)
                    {
                        stack.Push(element);
                    }
                }
            }
        }

        /// <summary>
        /// Populate the TreeView by accessing the backend data structure of the TreeModel root
        /// Then recursively create matching AssetTreeViewItem rows that correspond with the backend data
        /// </summary>
        /// <param name="parent">The AssetTreeElement to begin creating from</param>
        /// <param name="depth">The depth at which the element is to be placed in the tree view</param>
        /// <param name="newRows">The list that will hold all new rows</param>
        private void AddChildrenRecursively(T parent, int depth, IList<TreeViewItem> newRows)
        {
            foreach (T child in parent.children)
            {
                var item = new AssetTreeViewItem<T>(child.id, depth, child.name, child);
                newRows.Add(item);

                if (child.hasChildren)
                {
                    if (IsExpanded(child.id))
                    {
                        AddChildrenRecursively(child, depth + 1, newRows);
                    }
                    else
                    {
                        item.children = CreateChildListForCollapsedParent();
                    }
                }
            }
        }

        /// <summary>
        /// A helper method to get all the ancestor ids of a TreeViewItem row
        /// </summary>
        protected override IList<int> GetAncestors(int id)
        {
            return treeModel.GetAncestors(id);
        }

        /// <summary>
        /// A helper method to get all the descendant ids of a TreeViewItem row that also have children
        /// </summary>
        protected override IList<int> GetDescendantsThatHaveChildren(int id)
        {
            return treeModel.GetDescendantsThatHaveChildren(id);
        }
    }
}
