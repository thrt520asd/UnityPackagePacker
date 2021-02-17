namespace Assets.Tools.Script.Editor.Tool.Tree
{
    using System;
    using System.Collections.Generic;

    using Assets.Tools.Script.Helper;

    using UnityEngine;

    public class TreeViewDirectoryItem : ITreeViewDirectory
    {
        public List<ITreeViewItem> Child = new List<ITreeViewItem>();

        public Action<object> OnSelect;

        public string Name { get; set; }

        public bool IsSelect
        {
            get
            {
                return isSelect;
            }
            set
            {
//                if (isSelect == value)
//                {
//                    return;
//                }
                isSelect = value;
                if (isSelect)
                {
                    if (this.OnSelect != null)
                    {
                        this.OnSelect(null);
                    }
                }
            }
        }

        private bool isSelect;

        public bool IsOpen { get; set; }

        public int Depth { get; set; }

        public GUITreeView GuiTreeView { get; set; }

        public ITreeViewDirectory ParentDirectory { get; set; }

        public TreeViewItemState Show()
        {
            var treeViewItemState = new TreeViewItemState();

            if (IsSelect)
            {
                GUILayout.BeginHorizontal(GUITreeView.selectBackgroundStyle);
            }
            else
            {
                GUILayout.BeginHorizontal(GUITreeView.deselectBackgroundStyle);
            }
            if (this.IsOpen)
            {
                bool selectButton = GUITool.LabelButton("", GUILayout.Width(this.Depth * 20));
                bool closeButton = GUITool.LabelButton("\u25BC" + (char)0x200a, GUILayout.Width(12));
                selectButton = GUITool.LabelButton(this.Name) || selectButton;
                if (closeButton)
                {
                    treeViewItemState.OpenOperation = TreeViewOpenOperation.Close;
                }

                if (selectButton || closeButton)
                {
                    treeViewItemState.SelectOperation = IsSelect ? TreeViewSelectOperation.Deselect : TreeViewSelectOperation.Select;
                }
            }
            else
            {
                bool selectButton = GUITool.LabelButton("", GUILayout.Width(this.Depth * 20));
                var openButton = GUITool.LabelButton("\u25BA" + (char)0x200a, GUILayout.Width(12));
                selectButton = GUITool.LabelButton(this.Name) || selectButton;
                if (openButton)
                {
                    treeViewItemState.OpenOperation = TreeViewOpenOperation.Open;
                }

                if (selectButton)
                {
                    treeViewItemState.SelectOperation = IsSelect ? TreeViewSelectOperation.Deselect : TreeViewSelectOperation.Select;
                }
                    
            }
            GUILayout.EndHorizontal();
            return treeViewItemState;
        }

        public void AddChild(ITreeViewItem item)
        {
            if (item.ParentDirectory != null)
            {
                item.ParentDirectory.RemoveChild(item);
            }
            this.Child.Add(item);
            item.ParentDirectory = this;
        }

        public void RemoveChild(ITreeViewItem item)
        {
            this.Child.Remove(item);
            item.ParentDirectory = null;
        }

        public ITreeViewItem GetChild(string name)
        {
            return this.Child.FirstOrDefaultValue(e => e.Name == name);
        }

        public List<ITreeViewItem> GetChildren()
        {
            return this.Child;
        }

        public void BuildItemList(List<ITreeViewItem> list,int depth,bool withNotOpen)
        {
            Child.Sort(
                (l, r) =>
                {
                    if (l is ITreeViewDirectory && r is ITreeViewDirectory)
                    {
                        return StringComparer.CurrentCulture.Compare(l.Name, r.Name);
                    }
                    if (l is ITreeViewDirectory)
                    {
                        return -1;
                    }
                    if (r is ITreeViewDirectory)
                    {
                        return 1;
                    }
                    return StringComparer.CurrentCulture.Compare(l.Name, r.Name);
                });
            foreach (var treeViewItem in Child)
            {
                treeViewItem.Depth = depth >= 0 ? depth : treeViewItem.Depth;
                list.Add(treeViewItem);
                var treeViewDirectoryItem = treeViewItem as TreeViewDirectoryItem;
                if (treeViewDirectoryItem != null && (withNotOpen || treeViewDirectoryItem.IsOpen))
                {
                    treeViewDirectoryItem.BuildItemList(list, depth >= 0 ? depth + 1 : depth, withNotOpen);
                }
            }
        }
    }
}