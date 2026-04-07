using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnusedAssetsFinder.Editor.Extensions;

namespace UnusedAssetsFinder.Editor.TreeView {

	/// <summary>
	/// A class that is an instance of TreeView that is created using the data from a TreeModel and its AssetTreeElements
	/// </summary>
	/// <typeparam name="AssetTreeElement"></typeparam>
	public class UnusedAssetsTreeView : TreeViewWithTreeModel<AssetTreeElement> {

		/// <summary>
		/// Accessor Property to expose the treeModel to other classes
		/// </summary>
		public TreeModel<AssetTreeElement> TreeModel { get { return treeModel; } }

		/// <summary>
		/// Width of Icon elements
		/// </summary>
		private const float ICON_WIDTH = 16f;

		/// <summary>
		/// Padding between different GUI elements
		/// </summary>
		private const float ELEMENT_PADDING = 2f;

		/// <summary>
		/// Enum to hold the different columns for the TreeView
		/// </summary>
		private enum TreeViewColumns { Name, Size }

		/// <summary>
		/// Construct a simple treeview with a treemodel and multiple colunmns
		/// </summary>
		/// <param name="state">The state of the treeview - expanded, with a search and others</param>
		/// <param name="multiColumnHeader">A multicolumn variable to declare that the treeview will have multiple columns</param>
		/// <param name="model">The backend data that will populate the treeview</param>
		public UnusedAssetsTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<AssetTreeElement> model) : base(state, multiColumnHeader, model) {

			columnIndexForTreeFoldouts = 0;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			multiColumnHeader.sortingChanged += OnSortingChanged;

			Reload();
		}

		/// <summary>
		/// OVerride the default BuildRows function to determine which rows will make up the TreeView
		/// </summary>
		protected override IList<TreeViewItem> BuildRows(TreeViewItem root) {
			var rows = base.BuildRows(root);
			rootItem.children = SortByMultipleColumnsRecursively(rootItem);
			TreeUtility.TreeItemToList(root, rows);
			Repaint();
			return rows;
		}

		/// <summary>
		/// Callback for when the current multi header column sorting is changed
		/// </summary>
		private void OnSortingChanged(MultiColumnHeader mch) {
			SortIfNeeded(rootItem, GetRows());
		}

		/// <summary>
		/// Called when the multi header column sorting is changed
		/// Rows are sorted recursively and then the TreeViewItem is Redrawn
		/// </summary>
		private void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows) {
			if(rows.Count <= 1) {
				return;
			}
			if(multiColumnHeader.sortedColumnIndex == -1) {
				return;
			}

			rootItem.children = SortByMultipleColumnsRecursively(rootItem);
			TreeUtility.TreeItemToList(root, rows);
			Repaint();
		}

		/// <summary>
		/// Sort the rows of the TreeView, if a row has children, then its children are recursively sorted
		/// </summary>
		private List<TreeViewItem> SortByMultipleColumnsRecursively(TreeViewItem treeItem) {
			var sortedColumns = multiColumnHeader.state.sortedColumns;

			if(sortedColumns.Length == 0) {
				return treeItem.children;
			}

			var myTypes = treeItem.children.Cast<AssetTreeViewItem<AssetTreeElement>>();
			var orderedQuery = InitialOrder(myTypes, sortedColumns);

			for(int i = 1; i < sortedColumns.Length; i++) {
				TreeViewColumns sortOption = (TreeViewColumns) sortedColumns[i];
				bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

				switch(sortOption) {
					case TreeViewColumns.Name: //sort by name
						orderedQuery = orderedQuery.ThenBy(l => l.data.name, ascending);
						break;
					case TreeViewColumns.Size: //sort by file size
						orderedQuery = orderedQuery.ThenBy(l => l.data.rawFileSize, ascending);
						break;
				}
			}

			if(treeItem.hasChildren) { //if the current tree item has children
				if(IsExpanded(treeItem.id)) { //if it is expanded, and the children should be visible
					if(treeItem.children.Any(item => item == null)) { //if any of the children are null
						treeItem.children.Clear(); //clear current
						var dataElement = treeModel.Find(treeItem.id); //find data correspondent
						foreach(AssetTreeElement childDataElement in dataElement.children) { //repopulate front end row's children
							var childTreeItem = new AssetTreeViewItem<AssetTreeElement>(childDataElement.id, treeItem.depth + 1, childDataElement.name, childDataElement);
							treeItem.AddChild(childTreeItem);
						}
					}
				}
			}

			if(treeItem.children.Count > 0 && treeItem.children[0] != null) { //if the treeItem has children, and the first is not null - it is not a collapsed element
				foreach(var childTreeItem in treeItem.children) { //then foreach child
					if(childTreeItem.children != null && childTreeItem.children.Count > 0 && childTreeItem.children[0] != null) { //if the child also has children and they are not collapsed
						childTreeItem.children = SortByMultipleColumnsRecursively(childTreeItem); //the sort these children as well
					}
				}
			}

			return orderedQuery.Cast<TreeViewItem>().ToList();
		}

		/// <summary>
		/// Draw the columns in the default order before any user sorting is provided
		/// </summary>
		private IOrderedEnumerable<AssetTreeViewItem<AssetTreeElement>> InitialOrder(IEnumerable<AssetTreeViewItem<AssetTreeElement>> myTypes, int[] history) {
			TreeViewColumns sortOption = (TreeViewColumns) history[0];
			bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
			switch(sortOption) {
				case TreeViewColumns.Name:
					return myTypes.Order(treeItem => treeItem.data.name, ascending);
				case TreeViewColumns.Size:
					return myTypes.Order(treeItem => treeItem.data.rawFileSize, ascending);
				default:
					UnityEngine.Assertions.Assert.IsTrue(false, "Unhandled enum");
					break;
			}
			return myTypes.Order(treeItem => treeItem.data.name, ascending);
		}

		/// <summary>
		/// Overriden method to allow for customized drawing of the individual rows
		/// </summary>
		protected override void RowGUI(RowGUIArgs args) {
			var item = (AssetTreeViewItem<AssetTreeElement>) args.item;

			for(int i = 0; i < args.GetNumVisibleColumns(); ++i) {
				CellGUI(args.GetCellRect(i), item, (TreeViewColumns) args.GetColumn(i), ref args);
			}
		}

		/// <summary>
		/// Draw each different cell of a row in a specific way depending on which column the cell belongs to
		/// </summary>
		private void CellGUI(Rect cellRect, AssetTreeViewItem<AssetTreeElement> item, TreeViewColumns column, ref RowGUIArgs args) {
			CenterRectUsingSingleLineHeight(ref cellRect);

			GUIStyle labelStyle = GUI.skin.label;
			switch(column) {
				case TreeViewColumns.Name:
					{
						cellRect.xMin += GetContentIndent(item);

						Rect toggleRect = cellRect;
						toggleRect.x += ELEMENT_PADDING;
						toggleRect.width = ICON_WIDTH;

						var toggleVal = GUI.Toggle(toggleRect, item.data.GetShouldDelete(), GUIContent.none);

						if(toggleVal != item.data.GetShouldDelete()) { //if the toggle value has changed and no longer matches the cached value this frame
							item.data.SetShouldDelete(toggleVal, true, cachedSearchString); //then update the cached value
						}

						Rect iconRect = toggleRect;
						iconRect.x += ELEMENT_PADDING + toggleRect.width;
						iconRect.width = ICON_WIDTH;

						Texture icon = GetIcon(item.data);
						if(icon != null) {
							GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
						}

						Rect labelRect = iconRect;
						labelRect.x += ELEMENT_PADDING + iconRect.width;
						labelRect.width = labelStyle.CalcSize(new GUIContent(item.data.name)).x;

						GUI.Label(labelRect, new GUIContent(item.data.name));

						break;
					}
				case TreeViewColumns.Size:
					{
						Rect labelRect = cellRect;
						DefaultGUI.LabelRightAligned(labelRect, item.data.convertedFileSize.ToString(), args.selected, args.focused);
						break;
					}
			}
		}

		/// <summary>
		/// Find the item associated with the different tree element
		/// </summary>
		private Texture GetIcon(AssetTreeElement item) {
			return item.path.Contains('.') ? AssetDatabase.GetCachedIcon(item.path) : EditorGUIUtility.FindTexture("Folder Icon");
		}

		/// <summary>
		/// Method called when an element in the tree view is double clicked
		/// </summary>
		protected override void DoubleClickedItem(int id) {

			//var itemRow = FindItem(id, rootItem);
			//Debug.Log(itemRow.displayName + " has depth " + itemRow.depth + " has children " + itemRow.hasChildren + " parent name " + itemRow.parent.displayName);

			var element = treeModel.Find(id);
			var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(element.path);
			EditorGUIUtility.PingObject(obj);

			Selection.SetActiveObjectWithContext(obj, null);
		}

		/// <summary>
		/// Create a default column layout for the tree view
		/// </summary>
		public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth) {

			var columns = new [] {

				new MultiColumnHeaderState.Column {
						headerContent = new GUIContent(Strings.TreeViewElements.ColumnHeaders.Name),
							headerTextAlignment = TextAlignment.Left,
							width = treeViewWidth * 0.8f,
							minWidth = treeViewWidth * 0.5f,
							maxWidth = treeViewWidth * 0.8f,
							autoResize = true
					},

					new MultiColumnHeaderState.Column {
						headerContent = new GUIContent(Strings.TreeViewElements.ColumnHeaders.Size),
							headerTextAlignment = TextAlignment.Left,
							width = treeViewWidth * 0.2f,
							minWidth = treeViewWidth * 0.1f,
							maxWidth = treeViewWidth * 0.4f,
							autoResize = true
					}
			};

			var state = new MultiColumnHeaderState(columns);
			return state;
		}

		/// <summary>
		/// Used to update the min and max sizes of the columns of the tree view using the new tree view width
		/// Don't update the actual width - leave it as it is
		/// </summary>
		public void UpdateColumnWidths(float treeViewWidth) {

			for(int i = 0; i < multiColumnHeader.state.columns.Length; i++) {
				var column = multiColumnHeader.state.columns[i];
				switch((TreeViewColumns) i) {
					case TreeViewColumns.Name:
						{
							column.minWidth = treeViewWidth * 0.5f;
							column.maxWidth = treeViewWidth * 0.8f;
							break;
						}
					case TreeViewColumns.Size:
						{
							column.minWidth = treeViewWidth * 0.1f;
							column.maxWidth = treeViewWidth * 0.4f;
							break;
						}
				}
			}
		}
	}
}
