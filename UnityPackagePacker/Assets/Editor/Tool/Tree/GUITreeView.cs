namespace Assets.Tools.Script.Editor.Tool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Assets.Tools.Script.Editor.Tool.Tree;
    using Assets.Tools.Script.Editor.Window;
    using Assets.Tools.Script.Helper;
   

    using UnityEditor;

    using UnityEngine;

    public class GUITreeView
    {
        private static GUIStyle _selectBackgroundStyle;
        public static GUIStyle selectBackgroundStyle
        {
            get
            {
                if (_selectBackgroundStyle == null || _selectBackgroundStyle.normal.background == null)
                {
                    _selectBackgroundStyle = GUITool.GetAreaGUIStyle(ColorTool.GetColorFromRGBHexadecimal("3e5f96"));
                    _selectBackgroundStyle.fixedHeight = 16;
                }
                return _selectBackgroundStyle;
            }
        }
        private static GUIStyle _deselectBackgroundStyle;
        public static GUIStyle deselectBackgroundStyle
        {
            get
            {
                if (_deselectBackgroundStyle == null || _deselectBackgroundStyle.normal.background == null)
                {
                    _deselectBackgroundStyle = GUITool.GetAreaGUIStyle(new Color(0, 0, 0, 0.01f));
                    _deselectBackgroundStyle.fixedHeight = 16;
                }
                return _deselectBackgroundStyle;
            }
        }
        public static Event CurrEvent;
        
        /// <summary>
        /// 选择数据回调
        /// </summary>
        public Action<object> OnSelected
        {
            get
            {
                return this.treeView.OnSelected;
            }
            set
            {
                this.treeView.OnSelected = value;
            }
        }

        /// <summary>
        /// 生成新数据回调
        /// </summary>
        public Action<string> OnCreateTo
        {
            get
            {
                return this.treeView.OnCreateTo;
            }
            set
            {
                this.treeView.OnCreateTo = value;
            }
        }

        /// <summary>
        /// 生成新数据回调
        /// </summary>
        public Action<object> OnDelete
        {
            get
            {
                return this.treeView.OnDelete;
            }
            set
            {
                this.treeView.OnDelete = value;
            }
        }

        /// <summary>
        /// 路径更新回调
        /// </summary>
        public Action<object,string> OnPathUpdate
        {
            get
            {
                return this.treeView.OnPathUpdate;
            }
            set
            {
                this.treeView.OnPathUpdate = value;
            }
        }

        /// <summary>
        /// 选择改名回调
        /// </summary>
        public Action<object,string> OnRename
        {
            get
            {
                return this.treeView.OnRename;
            }
            set
            {
                this.treeView.OnRename = value;
            }
        }

        /// <summary>
        /// 复制回调
        /// </summary>
        public Action<object,string> OnCopyTo
        {
            get
            {
                return this.treeView.OnCopyTo;
            }
            set
            {
                this.treeView.OnCopyTo = value;
            }
        }


        private TreeView treeView = new TreeView();


        /// <summary>
        /// 开始显示视图
        /// </summary>
        public void BeginView()
        {
            CurrEvent = new Event(Event.current);
            this.treeView.BeginView();
        }

        /// <summary>
        /// 视图显示结束
        /// </summary>
        public void EndView()
        {
            this.treeView.EndView();
        }

        /// <summary>
        /// 显示数据
        /// 必须在BeginView 和 EndView之间
        /// </summary>
        /// <param name="userdata">The userdata.</param>
        /// <param name="path">The path.</param>
        public void ShowItem(object userdata, string path)
        {
            this.treeView.ShowItem(userdata, path);
        }

        /// <summary>
        /// 获得当前选中的路径
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetCurrSelectDirectoryPath()
        {
            return this.treeView.GetCurrSelectDirectoryPath();
        }

        /// <summary>
        /// 设置为脏之后，视图会得到一次更新
        /// </summary>
        public void SetDirty()
        {
            this.treeView.IsDirty = true;
        }

        public void Select(object userData)
        {
            if (!this.IsSelect(userData) && treeView.dataItems.ContainsKey(userData))
            {
                ITreeViewItem selectItem = this.treeView.dataItems[userData];
                ITreeViewItem treeViewLeafItem = selectItem;

                while (treeViewLeafItem.ParentDirectory != null)
                {
                    treeViewLeafItem.ParentDirectory.IsOpen = true;
                    treeViewLeafItem = treeViewLeafItem.ParentDirectory;
                }

                treeView.SelectItem(selectItem, Event.current.control);
                treeView.BuildItemList(null, 0, false);
            }
        }

        public bool IsSelect(object userData)
        {
            foreach (var b in treeView.selectedData.Keys)
            {
                if (b is TreeViewLeafItem)
                {
                    var treeViewLeafItem = b as TreeViewLeafItem;
                    if (treeViewLeafItem.userdata == userData)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}