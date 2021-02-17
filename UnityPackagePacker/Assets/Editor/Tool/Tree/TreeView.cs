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

    public class TreeView : ITreeViewDirectory
    {
        public bool IsDirty = true;

        /// <summary>
        /// 选择数据回调
        /// </summary>
        public Action<object> OnSelected;

        /// <summary>
        /// 生成新数据回调
        /// </summary>
        public Action<string> OnCreateTo;

        /// <summary>
        /// 生成新数据回调
        /// </summary>
        public Action<object> OnDelete;

        /// <summary>
        /// 路径更新回调
        /// </summary>
        public Action<object, string> OnPathUpdate;

        /// <summary>
        /// 选择改名回调
        /// </summary>
        public Action<object, string> OnRename;

        /// <summary>
        /// 复制回调
        /// </summary>
        public Action<object, string> OnCopyTo;


        public List<ITreeViewItem> Child = new List<ITreeViewItem>();

        public List<ITreeViewItem> ViewList = new List<ITreeViewItem>();

        private Vector2 scrollView;

        private ITreeViewItem lastSelectData;

        private Dictionary<ITreeViewItem, bool> moveList = new Dictionary<ITreeViewItem, bool>();

        private Dictionary<TreeViewLeafItem, bool> copyList = new Dictionary<TreeViewLeafItem, bool>();

        public Dictionary<ITreeViewItem, bool> selectedData = new Dictionary<ITreeViewItem, bool>();

        public Dictionary<object, TreeViewLeafItem> dataItems = new Dictionary<object, TreeViewLeafItem>();

        private Dictionary<object, string> dirtyDataItems = new Dictionary<object, string>();

        private PopInputWindow renameInputWindow;

        public void BeginView()
        {
            if (IsDirty)
            {
                //                this.Child.Clear();
                foreach (var key in selectedData.Keys.ToArray())
                {
                    DeselectItem(key);
                }
                selectedData.Clear();
                moveList.Clear();
                copyList.Clear();
                if (lastSelectData != null)
                {
                    DeselectItem(lastSelectData);
                    lastSelectData = null;
                }

                ViewList.Clear();
                scrollView = Vector2.zero;
            }
        }

        public void EndView()
        {
            var isDirty = this.IsDirty;
            if (IsDirty)
            {
                RebuildItems();
                BuildItemList(ViewList, 0, false);
            }
            IsDirty = false;
            try
            {
                Show();
            }
            catch (System.ArgumentException e)
            {
                Debug.Log(e);
            }
            
            if (isDirty)
            {
                EditorWindow.focusedWindow.Repaint();
            }
        }

        private void RebuildItems()
        {
            foreach (var drityData in dirtyDataItems)
            {
                var path = drityData.Value;
                var userdata = drityData.Key;

                string[] strings = path.Split('/');
                if (strings == null || strings.Length == 0)
                {
                    return;
                }

                ITreeViewDirectory directory = this;

                for (int i = 0; i < strings.Length; i++)
                {
                    string name = strings[i];

                    ITreeViewItem treeViewItem = directory.GetChild(name);

                    if (treeViewItem == null)
                    {
                        if (i == strings.Length - 1)
                        {
                            TreeViewLeafItem treeViewLeafItem;
                            var tryGetValue = this.dataItems.TryGetValue(userdata, out treeViewLeafItem);
                            if (!tryGetValue)
                            {
                                treeViewLeafItem = new TreeViewLeafItem();
                                dataItems.Add(userdata, treeViewLeafItem);
                            }
                            //最后一个是数据节点，生成这个节点
                            treeViewLeafItem.userdata = userdata;
                            treeViewLeafItem.OnSelected = this.OnSelected;

                            treeViewItem = treeViewLeafItem;
                        }
                        else
                        {
                            //是路径节点，生成这个节点
                            TreeViewDirectoryItem treeViewDirectoryItem = new TreeViewDirectoryItem();
                            treeViewDirectoryItem.OnSelect = this.OnSelected;
                            treeViewItem = treeViewDirectoryItem;
                        }

                        //                    treeViewItem.ParentDirectory = parent;
                        //                    treeViewItem.Depth = i;
                        treeViewItem.Name = name;
                        //                    treeViewItem.GuiTreeView = this;
                        directory.AddChild(treeViewItem);
                    }
                    if (treeViewItem is ITreeViewDirectory)
                    {
                        //进入路径的下一个目录
                        directory = (treeViewItem as ITreeViewDirectory);
                    }
                }
            }

            foreach (var treeViewLeafItem in dataItems.ToArray())
            {
                var userData = treeViewLeafItem.Key;
                var item = treeViewLeafItem.Value;

                //新数据中不存在了
                if (!dirtyDataItems.ContainsKey(userData))
                {
                    item.ParentDirectory.RemoveChild(item);
                    dataItems.Remove(item.userdata);
                }
            }

            dirtyDataItems.Clear();
        }

        public void ShowItem(object userdata, string path)
        {
            if (!IsDirty)
            {
                return;
            }

            dirtyDataItems.Add(userdata, path);
        }

        private void Operation()
        {
            var eventType = GUITreeView.CurrEvent.type;
            var keyCode = GUITreeView.CurrEvent.keyCode;
            if (eventType == EventType.KeyDown)
            {
                this.OperationKeyDown(keyCode, GUITreeView.CurrEvent.control, GUITreeView.CurrEvent.alt, GUITreeView.CurrEvent.shift);
            }
            if (eventType == EventType.KeyUp)
            {
                this.OperationKeyUp(keyCode, GUITreeView.CurrEvent.control, GUITreeView.CurrEvent.alt, GUITreeView.CurrEvent.shift);
            }

            if (eventType == EventType.MouseUp && GUITreeView.CurrEvent.button == 1)
            {
                PopMenuWindow menu = new PopMenuWindow();
                menu.HasSelectTag = false;
                menu.AddItem("新建项", false,
                    () =>
                    {
                        this.Create();
                    });
                menu.AddItem("新建组", false,
                    () =>
                    {
                        this.CreateDirectory();
                    });
                if (selectedData.Count == 1)
                {
                    menu.AddItem("重命名", false,
                    () =>
                    {
                        this.Rename();
                    });
                }
                if (selectedData.Count > 0)
                {
                    menu.AddItem("删除", false,
                    () =>
                    {
                        this.Delete();
                    });
                }
                if (selectedData.Count > 0 && !selectedData.Keys.Any(e => e is ITreeViewDirectory))
                {
                    menu.AddItem("复制", false,
                    () =>
                    {
                        this.Copy();
                    });
                }
                if (selectedData.Count > 0)
                {
                    menu.AddItem("剪切", false,
                    () =>
                    {
                        this.Cut();
                    });
                }
                if ((this.moveList.Count > 0 || this.copyList.Count > 0) && this.lastSelectData != null)
                {
                    menu.AddItem("粘帖", false,
                    () =>
                    {
                        this.Paste();
                    });
                }
                menu.PopWindow();
            }
        }

        private void OperationKeyUp(KeyCode keyCode, bool ctrl, bool alt, bool shift)
        {
            if (keyCode == KeyCode.C && ctrl && alt)
            {
                this.Copy();
            }
            if (keyCode == KeyCode.V && ctrl && alt)
            {
                this.Paste();
            }
            if (keyCode == KeyCode.X && ctrl && alt)
            {
                this.Cut();
            }
        }

        private void OperationKeyDown(KeyCode keyCode,bool ctrl,bool alt,bool shift)
        {
            if (keyCode == KeyCode.RightArrow && alt)
            {
                var treeViewDirectory = this.lastSelectData as TreeViewDirectoryItem;
                if (treeViewDirectory != null)
                {
                    OpenItem(treeViewDirectory);
                    Event.current.Use();
                    EditorWindow.focusedWindow.Repaint();
                }
            }
            if (keyCode == KeyCode.LeftArrow && alt)
            {
                if (this.lastSelectData != null)
                {
                    var treeViewDirectory = this.lastSelectData as TreeViewDirectoryItem;
                    if (treeViewDirectory != null)
                    {
                        if (treeViewDirectory.IsOpen)
                        {
                            CloseItem(treeViewDirectory);
                            Event.current.Use();
                            EditorWindow.focusedWindow.Repaint();
                        }
                        else if (this.lastSelectData.ParentDirectory != null
                                 && this.lastSelectData.ParentDirectory != this)
                        {
                            SelectItem(this.lastSelectData.ParentDirectory, GUITreeView.CurrEvent.control);
                            Event.current.Use();
                            EditorWindow.focusedWindow.Repaint();
                        }
                    }
                    else if (this.lastSelectData.ParentDirectory != null && this.lastSelectData.ParentDirectory != this)
                    {
                        SelectItem(this.lastSelectData.ParentDirectory, GUITreeView.CurrEvent.control);
                        Event.current.Use();
                        EditorWindow.focusedWindow.Repaint();
                    }
                }
            }
            if (keyCode == KeyCode.UpArrow && alt)
            {
                var treeViewItem = this.GetPreItem(this.lastSelectData);
                if (treeViewItem != null)
                {
                    if (GUITreeView.CurrEvent.control && treeViewItem.IsSelect)
                    {
                        this.DeselectItem(lastSelectData);
                        this.SelectItem(treeViewItem, GUITreeView.CurrEvent.control);
                    }
                    else
                    {
                        this.SelectItem(treeViewItem, GUITreeView.CurrEvent.control);
                    }
                    Event.current.Use();
                    EditorWindow.focusedWindow.Repaint();
                }
            }
            if (keyCode == KeyCode.DownArrow && alt)
            {
                var treeViewItem = this.GetNextItem(this.lastSelectData);
                if (treeViewItem != null)
                {
                    if (GUITreeView.CurrEvent.control && treeViewItem.IsSelect)
                    {
                        this.DeselectItem(lastSelectData);
                        this.SelectItem(treeViewItem, GUITreeView.CurrEvent.control);
                    }
                    else
                    {
                        this.SelectItem(treeViewItem, GUITreeView.CurrEvent.control);
                    }
                    Event.current.Use();
                    EditorWindow.focusedWindow.Repaint();
                }
            }

            //            if (currEvent.control)
            //            {
            //                if (keyCode == KeyCode.X)
            //                {
            //                    Copy();
            //                }
            //                if (keyCode == KeyCode.C)
            //                {
            //                    Cut();
            //                }
            //                if (keyCode == KeyCode.V)
            //                {
            //                    Paste();
            //                }
            //            }
        }

        private void Rename()
        {
            if (lastSelectData == null)
            {
                return;
            }
            this.renameInputWindow = new PopInputWindow();
            this.renameInputWindow.InputString = lastSelectData.Name;
            if (lastSelectData is ITreeViewDirectory)
            {
                this.renameInputWindow.OnInput = (newName) =>
                {
                    if (newName.IsNullOrEmpty())
                    {
                        return;
                    }
                    var treeViewDirectory = this.lastSelectData as ITreeViewDirectory;
                    treeViewDirectory.Name = newName;

                    var moveItems = new List<ITreeViewItem>();
                    treeViewDirectory.BuildItemList(moveItems, -1, true);

                    foreach (var item in moveItems)
                    {
                        var treeViewLeafItem = item as TreeViewLeafItem;
                        if (treeViewLeafItem != null)
                        {
                            var toPath = this.GetPath(item);
                            if (OnPathUpdate != null)
                            {
                                OnPathUpdate(treeViewLeafItem.userdata, toPath);
                            }
                        }
                    }
                };
            }
            else if (lastSelectData is TreeViewLeafItem)
            {
                this.renameInputWindow.OnInput = (newName) =>
                {
                    OnRename((lastSelectData as TreeViewLeafItem).userdata, newName);
                };
            }

            EditorWindow.focusedWindow.Repaint();
        }

        private void CreateDirectory()
        {
            var currSelectDirectory = this.GetCurrSelectDirectory();
            if (currSelectDirectory != null)
            {
                string folderName = "NewFolder";
                int index = 0;
                while (currSelectDirectory.GetChild(folderName) != null)
                {
                    folderName = string.Format("NewFolder{0}", ++index);
                }
                currSelectDirectory.AddChild(new TreeViewDirectoryItem() { Name = folderName,OnSelect = this.OnSelected});
            }
            this.IsDirty = true;
        }

        private void Create()
        {
            if (this.OnCreateTo != null)
            {
                this.OnCreateTo(this.GetCurrSelectDirectoryPath());
            }
        }

        private void Delete()
        {
            if (this.OnDelete != null)
            {
                List<ITreeViewItem> items = new List<ITreeViewItem>();
                foreach (var item in selectedData.Keys)
                {
                    if (item is ITreeViewDirectory)
                    {
                        (item as ITreeViewDirectory).BuildItemList(items, -1, true);
                    }
                    else if (item is TreeViewLeafItem)
                    {
                        items.Add(item);
                    }
                }
                foreach (var treeViewItem in items)
                {
                    if (treeViewItem is TreeViewLeafItem)
                    {
                        this.OnDelete((treeViewItem as TreeViewLeafItem).userdata);
                    }
                }
                foreach (var item in selectedData.Keys)
                {
                    if (item is ITreeViewDirectory)
                    {
                        item.ParentDirectory.RemoveChild(item);
                    }
                }
                IsDirty = true;
            }
        }

        private void Copy()
        {
            moveList.Clear();
            copyList.Clear();
            foreach (var b in selectedData)
            {
                if (b.Key is TreeViewLeafItem)
                {
                    copyList[b.Key as TreeViewLeafItem] = true;
                }
            }
        }

        private void Cut()
        {
            moveList.Clear();
            copyList.Clear();
            foreach (var b in selectedData)
            {
                moveList[b.Key] = true;
            }
        }

        private void Paste()
        {
            var selectDirectory = this.lastSelectData as ITreeViewDirectory;
            if (selectDirectory == null)
            {
                selectDirectory = this.lastSelectData != null ? this.lastSelectData.ParentDirectory : null;
            }

            //移动
            if (moveList.Count > 0 && selectDirectory != null)
            {
                List<ITreeViewItem> moveItems = new List<ITreeViewItem>();
                foreach (var item in moveList.Keys)
                {
                    selectDirectory.AddChild(item);
                }
                foreach (var item in moveList.Keys)
                {
                    if (item is ITreeViewDirectory)
                    {
                        (item as ITreeViewDirectory).BuildItemList(moveItems, -1, true);
                    }
                    else if (item is TreeViewLeafItem)
                    {
                        moveItems.Add(item);
                    }
                }
                foreach (var item in moveItems)
                {
                    var treeViewLeafItem = item as TreeViewLeafItem;
                    if (treeViewLeafItem != null)
                    {
                        var toPath = this.GetPath(item);
                        if (OnPathUpdate != null)
                        {
                            OnPathUpdate(treeViewLeafItem.userdata, toPath);
                        }
                        //                                Debug.Log(string.Format("{0} -> {1}", path1, toPath));
                    }
                }
                moveList.Clear();
                //                        EditorWindow.focusedWindow.Repaint();
                IsDirty = true;
            }

            //复制
            if (copyList.Count > 0 && selectDirectory != null)
            {
                foreach (var b in copyList.Keys)
                {
                    this.OnCopyTo(b.userdata, this.GetCurrSelectDirectoryPath());
                }

                copyList.Clear();
                EditorWindow.focusedWindow.Repaint();
            }
        }

        private ITreeViewItem GetNextItem(ITreeViewItem item)
        {
            var findIndex = this.ViewList.FindIndex(e => e == item);
            if (findIndex >= 0 && findIndex < ViewList.Count - 1)
            {
                return this.ViewList[findIndex + 1];
            }
            return null;
        }

        private ITreeViewItem GetPreItem(ITreeViewItem item)
        {
            if (item == null)
            {
                return null;
            }
            var findIndex = this.ViewList.FindIndex(e => e == item);
            if (findIndex > 0)
            {
                return this.ViewList[findIndex - 1];
            }
            return null;
        }

        public void OpenItem(ITreeViewItem item)
        {
            item.IsOpen = true;
            BuildItemList(ViewList, 0, false);
        }

        public void CloseItem(ITreeViewItem item)
        {
            item.IsOpen = false;
            BuildItemList(ViewList, 0, false);
        }

        public void SelectItem(ITreeViewItem item, bool multiChoice)
        {
            if (!multiChoice)
            {
                foreach (var b in this.selectedData.Keys)
                {
                    b.IsSelect = false;
                }
                this.selectedData.Clear();
            }

            this.lastSelectData = item;
            this.selectedData[item] = true;
            item.IsSelect = true;

        }

        public void DeselectItem(ITreeViewItem item)
        {
            if (item == lastSelectData)
            {
                this.lastSelectData = null;
            }
            selectedData.Remove(item);
            item.IsSelect = false;
        }

        public string GetCurrSelectDirectoryPath()
        {
            if (lastSelectData != null)
            {
                if (lastSelectData is ITreeViewDirectory)
                {
                    return GetPath(lastSelectData);
                }
                else
                {
                    return GetPath(lastSelectData.ParentDirectory);
                }
            }
            return string.Empty;
        }

        private ITreeViewDirectory GetCurrSelectDirectory()
        {
            if (lastSelectData != null)
            {
                if (lastSelectData is ITreeViewDirectory)
                {
                    return lastSelectData as ITreeViewDirectory;
                }
                return this.lastSelectData.ParentDirectory;
            }
            return null;
        }

        private string GetPath(ITreeViewItem item)
        {
            string s = string.Empty;
            while (item != this)
            {
                s = string.Format("/{0}{1}", item.Name, s);
                item = item.ParentDirectory;
            }
            if (s == string.Empty)
            {
                return s;
            }
            //            Debug.Log(s);
            return s.Substring(1, s.Length - 1);
        }

        public string Name { get; set; }

        public bool IsSelect { get; set; }

        public bool IsOpen { get; set; }

        public int Depth { get; set; }

        public GUITreeView GuiTreeView { get; set; }

        public ITreeViewDirectory ParentDirectory { get; set; }

        public TreeViewItemState Show()
        {
            scrollView = GUILayout.BeginScrollView(scrollView);

            for (int i = 0; i < ViewList.Count; i++)
            {
                var treeViewItem = this.ViewList[i];
                var treeViewItemState = treeViewItem.Show();
                
                if (treeViewItemState.SelectOperation == TreeViewSelectOperation.Select)
                {
                    this.SelectItem(treeViewItem, GUITreeView.CurrEvent.control);
                }
                else if (treeViewItemState.SelectOperation == TreeViewSelectOperation.Deselect)
                {
                    if (GUITreeView.CurrEvent.control)
                    {
                        this.DeselectItem(treeViewItem);
                    }
                    else
                    {
                        this.SelectItem(treeViewItem, GUITreeView.CurrEvent.control);
                    }
                }

                if (treeViewItemState.OpenOperation == TreeViewOpenOperation.Open)
                {
                    OpenItem(treeViewItem);
                }
                else if (treeViewItemState.OpenOperation == TreeViewOpenOperation.Close)
                {
                    CloseItem(treeViewItem);
                }
            }
            GUILayout.EndScrollView();

            Operation();

            if (this.renameInputWindow != null)
            {
                this.renameInputWindow.PopWindow();
                this.renameInputWindow = null;
            }

            return new TreeViewItemState();
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

        public void BuildItemList(List<ITreeViewItem> list, int depth, bool withNotOpen)
        {
            if (list == null)
            {
                list = ViewList;
            }
            list.Clear();
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
            foreach (var key in selectedData.Keys.ToArray())
            {
                if (!list.Contains(key))
                {
                    DeselectItem(key);
                }
            }
        }
    }
}