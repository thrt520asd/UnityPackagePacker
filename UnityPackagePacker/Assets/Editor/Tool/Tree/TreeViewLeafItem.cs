namespace Assets.Tools.Script.Editor.Tool.Tree
{
    using System;
    using System.Collections.Generic;

    using UnityEditor;

    using UnityEngine;

    public class TreeViewLeafItem : ITreeViewItem
    {
        public object userdata;

        public Action<object> OnSelected;

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
                    if (this.OnSelected != null)
                    {
                        this.OnSelected(userdata);
                    }
                }
            }
        }

        private bool isSelect;

        public bool IsOpen { get; set; }

        public int Depth { get; set; }

        public ITreeViewDirectory ParentDirectory { get; set; }

        public TreeViewItemState Show()
        {
            var treeViewItemState = new TreeViewItemState();
            if (this.IsSelect)
            {
                GUILayout.BeginHorizontal(GUITreeView.selectBackgroundStyle);
                var labelButton = GUITool.LabelButton("", GUILayout.Width(this.Depth * 20));
                var button = GUITool.LabelButton(this.Name);
                if (labelButton || button)
                {
                    treeViewItemState.SelectOperation = TreeViewSelectOperation.Deselect;
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal(GUITreeView.deselectBackgroundStyle);
                bool button = GUITool.LabelButton("", GUILayout.Width(this.Depth * 20));
                if (GUITool.LabelButton(this.Name) || button)
                {
                    treeViewItemState.SelectOperation  = TreeViewSelectOperation.Select;
                }
                GUILayout.EndHorizontal();
            }
            
            return treeViewItemState;
        }
    }
}