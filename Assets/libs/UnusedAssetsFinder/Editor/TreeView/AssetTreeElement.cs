using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnusedAssetsFinder.Editor.Util;

namespace UnusedAssetsFinder.Editor.TreeView
{
    /// <summary>
    /// Backend data element for each Unused Asset Found by the plugin
    /// </summary>
    [Serializable]
    public class AssetTreeElement
    {
        public  int                    id;
        public  string                 name;
        public  int                    depth;
        public  AssetTreeElement       parent;
        public  List<AssetTreeElement> children;
        public  string                 path;
        public  long                   rawFileSize;
        public  ConvertedFileSize      convertedFileSize;
        private bool                   shouldDelete;

        /// <summary>
        /// Create a new Tree Element - NON ROOT ELEMENTS ONLY
        /// </summary>
        /// <param name="id">Unique Id for element</param>
        /// <param name="name">Name for the element</param>
        /// <param name="path">Path for the element - local to the project 'Assets/'</param>
        public AssetTreeElement(int id, string name, string path)
        {
            this.id   = id;
            this.name = name;
            this.path = path;
        }

        /// <summary>
        /// Create a new Tree Element - ROOT ELEMENT ONLY
        /// </summary>
        /// <param name="id">Unique Id for element</param>
        /// <param name="name">Name for the element</param>
        /// <param name="path">Path for the element - local to the project 'Assets/'</param>
        /// <param name="depth">Always 0 for root element</param>
        public AssetTreeElement(int id, string name, int depth, string path)
        {
            this.id    = id;
            this.name  = name;
            this.depth = depth;
            this.path  = path;
        }

        /// <summary>
        /// Does the element have any children
        /// </summary>
        public bool hasChildren
        {
            get { return children != null && children.Count > 0; }
        }

        /// <summary>
        /// Return whether the element has been marked for deletion
        /// </summary>
        public bool GetShouldDelete()
        {
            return shouldDelete;
        }

        /// <summary>
        /// Sets the deletion status of this Object and also checks and updates the deletion status of its ancestors if necessary, 
        /// Also checks and updates the deletion status of its ancestors if necessary
        /// </summary>
        /// <param name="value"></param>
        /// <param name="childRecursive"> If true and Object has children, will recursively set the deletion status of all children</param>
        /// <param name="searchQuery">Search Query</param>
        public void SetShouldDelete(bool value, bool childRecursive, string searchQuery)
        {
            if (searchQuery == null) searchQuery = "";
            shouldDelete = value; //set to new value
            if (hasChildren && childRecursive)
            {
                //if this element has children, the value has changed and the function should be called recursively
                foreach (AssetTreeElement childElement in children)
                {
                    //foreach child
                    if (childElement.name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        childElement.SetShouldDelete(value, childRecursive, searchQuery); //set the value of children and set whether it should call recursively
                    }
                }
            }

            if (parent != null)
            {
                //if the parent element is not null
                //check if all of the parents have the same value as the parent
                if (parent.children.Any(child => child.shouldDelete == false))
                {
                    //if any child is set to false
                    parent.SetShouldDelete(false, false, searchQuery); // then the parent should also be set to false as not all of its children are selected
                }
                else
                {
                    //if they are all true
                    parent.SetShouldDelete(true, false, searchQuery); //then the parent should also be set to true - all children are selected
                }
            }
        }

        /// <summary>
        /// Helper method to add children to Object, 
        /// Initializes a new children list if it is null, 
        /// Also sets the parent of the new child to be this Object when added as a child to this object
        /// </summary>
        /// <param name="newChild">New Child</param>
        public void AddChild(AssetTreeElement newChild)
        {
            if (children == null)
            {
                children = new List<AssetTreeElement>();
            }

            children.Add(newChild);
            newChild.parent = this;
            newChild.depth  = depth + 1;
        }

        public override string ToString()
        {
            return string.Format("AssetTreeElement: {0} has parent: {1} and has {2} children", name, parent.name, children.Count);
        }

        /// <summary>
        /// Calculate the file size for the element, 
        /// If the element is a file then it will calculate the total file size for all its child elements and their children
        /// </summary>
        public void CalculateFileSizes()
        {
            if (System.IO.Directory.Exists(path))
            {
                //if folder - get value of all its children and cumulate it together
                if (children == null || children.Count == 0)
                {
                    return;
                }

                rawFileSize = 0;
                foreach (AssetTreeElement child in children)
                {
                    child.CalculateFileSizes();
                    rawFileSize += child.rawFileSize;
                }

                convertedFileSize = new ConvertedFileSize(rawFileSize);
            }
            else
            {
                //else its a file and get its value
                var dataPathParent = Directory.GetParent(Application.dataPath);
                if (dataPathParent == null)
                {
                    return;
                }

                var filePath = dataPathParent.FullName.Replace("\\", "/") + "/" + path;

                bool fileExists = File.Exists(filePath);

                if (fileExists)
                {
                    var fileInfo = new FileInfo(filePath);

                    rawFileSize = fileInfo.Length;

                    convertedFileSize = new ConvertedFileSize(rawFileSize);
                }
                else
                {
                    Debug.LogWarning($"UnusedAssetsFinder: Cannot find file at path [{filePath}]\r\nNot adding the file length to the total");
                }
            }
        }
    }
}