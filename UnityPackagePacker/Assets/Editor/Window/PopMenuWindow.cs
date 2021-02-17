namespace Assets.Tools.Script.Editor.Window
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Assets.Tools.Script.Editor.Tool;
    using Assets.Tools.Script.Helper;

    //using LuaInterface;

    using UnityEditor;

    using UnityEngine;

    #endregion

    /// <summary>
    /// The pop menu window.
    /// </summary>
    public class PopMenuWindow : Script.Editor.Window.PopEditorWindow<PopMenuWindow>
    {
        public const string SearchLevelName = "Search";

        /// <summary>
        /// The has search bar
        /// </summary>
        public bool HasSearchBar;

        /// <summary>
        /// 自动排序菜单项
        /// </summary>
        public bool AutoSortItem = false;

        /// <summary>
        /// 菜单名字
        /// </summary>
        public string MenuName = "";

        /// <summary>
        /// 是否使用分级菜单
        /// </summary>
        public bool Gradable = false;

        /// <summary>
        /// 有选中标志
        /// </summary>
        public bool HasSelectTag = false;

        /// <summary>
        /// 菜单level记录
        /// </summary>
        private List<string> menuNameStack = new List<string>(); 

        #region Fields

        /// <summary>
        /// The items.
        /// </summary>
        protected Dictionary<string,List<PopMenuWindowItem>> Items = new Dictionary<string, List<PopMenuWindowItem>>();

        /// <summary>
        /// The item list
        /// </summary>
        protected List<PopMenuWindowItem> ItemList = new List<PopMenuWindowItem>();

        /// <summary>
        /// The jump items
        /// </summary>
        protected Dictionary<string, PopMenuWindowItemJump> JumpItems = new Dictionary<string, PopMenuWindowItemJump>(); 

        protected GUISearchBar<PopMenuWindowItem> searchBar;

        private StringBuilder jumpItemBuilder = new StringBuilder();

        private Vector2 scroll;

        #endregion

        #region Public Methods and Operators

        public PopMenuWindow()
        {

        }

        public PopMenuWindow(string menuName,bool gradable=true, bool autoSort=true, bool hasSearchBar = true, bool hasSelectTag = false)
        {
            this.MenuName = menuName;
            this.Gradable = gradable;
            this.HasSearchBar = hasSearchBar;
            this.AutoSortItem = autoSort;
            this.HasSelectTag = hasSelectTag;
        }

        /// <summary>
        /// The add disabled item.
        /// </summary>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <param name="on">
        /// The on.
        /// </param>
        public void AddDisabledItem(string content, bool on = false)
        {
            string itemName, itemPath;
            string[] jumpItem, jumpItemPath;
            this.SplitItem(content,out itemName,out itemPath,out jumpItem,out jumpItemPath);
            var popMenuWindowItemDisable = new PopMenuWindowItemDisable(itemName, @on, this);
            this.AddMenuItem(popMenuWindowItemDisable, itemPath, jumpItem, jumpItemPath);
        }

        /// <summary>
        /// The add item.
        /// </summary>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <param name="on">
        /// The on.
        /// </param>
        /// <param name="func">
        /// The func.
        /// </param>
        public void AddItem(string content, bool on, Action func)
        {
            string itemName, itemPath;
            string[] jumpItem, jumpItemPath;
            this.SplitItem(content, out itemName, out itemPath, out jumpItem, out jumpItemPath);
            var popMenuWindowItem1 = new PopMenuWindowItem1(itemName, @on, func, this);
            this.AddMenuItem(popMenuWindowItem1, itemPath, jumpItem, jumpItemPath);
        }

        /// <summary>
        /// The add item.
        /// </summary>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <param name="on">
        /// The on.
        /// </param>
        /// <param name="func">
        /// The func.
        /// </param>
        /// <param name="userData">
        /// The user data.
        /// </param>
        public void AddItem(string content, bool on, Action<object> func, object userData)
        {
            string itemName, itemPath;
            string[] jumpItem, jumpItemPath;
            this.SplitItem(content, out itemName, out itemPath, out jumpItem, out jumpItemPath);
            var popMenuWindowItem2 = new PopMenuWindowItem2(itemName, @on, func, userData, this);
            this.AddMenuItem(popMenuWindowItem2, itemPath, jumpItem, jumpItemPath);
        }

        private void AddMenuItem(PopMenuWindowItem item, string itemPath, string[] jumpItems, string[] jumpItemPaths)
        {
            if (!this.Gradable)
            {
                itemPath = this.MenuName;
                jumpItems = null;
            }

            if (!this.Items.ContainsKey(itemPath))
            {
                this.Items.Add(itemPath, new List<PopMenuWindowItem>());
            }

            this.Items[itemPath].Add(item);
            this.ItemList.Add(item);
            if (jumpItems != null)
            {
                //                var format = string.Format("{0}/{1}", jumpItemPath, jumpItem);
                for (int i = 0; i < jumpItems.Length; i++)
                {
                    var jumpItem = jumpItems[i];
                    var jumpItemPath = jumpItemPaths[i];
                    var format = string.Format("{0}/{1}", jumpItemPath, jumpItem);

                    PopMenuWindowItemJump popMenuWindowItemJump;
                    if (!this.JumpItems.TryGetValue(format, out popMenuWindowItemJump))
                    {
                        if (!this.Items.ContainsKey(jumpItemPath))
                        {
                            this.Items.Add(jumpItemPath, new List<PopMenuWindowItem>());
                        }
                        popMenuWindowItemJump = new PopMenuWindowItemJump(jumpItem, this, this);
                        this.JumpItems.Add(format, popMenuWindowItemJump);
                        this.Items[jumpItemPath].Add(popMenuWindowItemJump);
                    }
                    else
                    {
                        popMenuWindowItemJump.UpdateShortcutKey();
                    }
                }
            }
                
        }

        public void SplitItem(string content, out string itemName, out string itemPath, out string[] jumpItem, out string[] jumpItemPath)
        {
            var strings = content.Split('/');
            if (strings.Length >= 2)
            {
                //TODO 修复
                //this.jumpItemBuilder.Clear();
                this.jumpItemBuilder.Append(this.MenuName);
                jumpItem = new string[strings.Length - 1];
                jumpItemPath = new string[strings.Length - 1];
                for (int i = 0; i < strings.Length - 1; i++)
                {
                    var s = strings[i];
                    jumpItem[i] = s;
                    jumpItemPath[i] = this.jumpItemBuilder.ToString();
                    this.jumpItemBuilder.Append("/");
                    this.jumpItemBuilder.Append(s);
                }
                itemName = strings[strings.Length - 1];
                itemPath = this.jumpItemBuilder.ToString();
            }
            else
            {
                itemName = content;
                itemPath = this.MenuName;
                jumpItem = null;
                jumpItemPath = null;
            }
        }

        public void PageTo(string levelName)
        {
            menuNameStack.Add(levelName);
        }

        /// <summary>
        /// The get item count.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int GetItemCount()
        {
            int count = 0;
            if (this.ItemList != null)
            {
                count = this.ItemList.Count(e => e.IsContent);
            }
            return count;
        }

        #endregion

        #region Methods

        protected override void OnOpen()
        {
            this.menuNameStack.Clear();
            this.menuNameStack.Add(this.MenuName);
        }

        /// <summary>
        /// The draw on gui.
        /// </summary>
        protected override void DrawOnGUI()
        {
            if (this.Items.Count == 0)
            {
                return;
            }
            var levelName = this.menuNameStack[this.menuNameStack.Count - 1];
            var levelPath = this.menuNameStack.Joint("/");

            List<PopMenuWindowItem> items = this.Items[levelPath];
            if (this.HasSearchBar)
            {
                if (this.searchBar == null)
                {
                    this.searchBar = new GUISearchBar<PopMenuWindowItem>();
                }

                var searchItems = this.searchBar.Draw(this.ItemList, item => item.GetItemName());
                if (this.searchBar.SearchContent.IsNOTNullOrEmpty())
                {
                    items = searchItems;
                    if (this.MenuName.IsNOTNullOrEmpty())
                    {
                        levelName = SearchLevelName;
                    }
                }
            }
            if (levelName.IsNOTNullOrEmpty())
            {
                GUILayout.BeginHorizontal(GUITool.GetAreaGUIStyle(new Color(0, 0, 0, 0.2f)));
                if (this.menuNameStack.Count >= 2 && levelName != SearchLevelName)
                {
                    if (GUITool.Button("◀", Color.clear, GUILayout.Width(30)) || GUITool.Button(levelName, Color.clear) || GUITool.Button(" ", Color.clear, GUILayout.Width(30)))
                    {
                        this.menuNameStack.RemoveAt(this.menuNameStack.Count - 1);
                    }
                }
                else
                {
                    GUITool.Button(levelName, Color.clear);
                }
                GUILayout.EndHorizontal();
            }
            
            if (!this.AutoAdjustSize)
            {
                this.scroll = GUILayout.BeginScrollView(this.scroll);
            }

            if (this.AutoSortItem)
            {
                items.Sort(
                (l, r) =>
                {
                    if (l is PopMenuWindowItemJump)
                    {
                        return 1;
                    }
                    if (r is PopMenuWindowItemJump)
                    {
                        return -1;
                    }
                    return StringComparer.CurrentCulture.Compare(l.GetItemName(), r.GetItemName());
                });
            }
            
            foreach (var popMenuWindowItem in items)
            {
                if (popMenuWindowItem.Show())
                {
                    popMenuWindowItem.Select();
                }
            }
            if (!this.AutoAdjustSize)
            {
                GUILayout.EndScrollView();
            }
            if (Event.current.type == EventType.KeyUp && !Event.current.alt && !Event.current.command && Event.current.shift && !Event.current.control)
            {
                if (this.searchBar == null || string.IsNullOrEmpty(this.searchBar.SearchContent))
                {
                    if (Event.current.keyCode == KeyCode.Backspace)
                    {
                        
                    }
                    else if (Event.current.keyCode == KeyCode.Escape)
                    {
                        if (this.menuNameStack.Count >= 2 && levelName != SearchLevelName)
                        {
                            this.menuNameStack.RemoveAt(this.menuNameStack.Count - 1);
                            Event.current.Use();
                        }
                    }
                    else
                    {
                        foreach (var menuWindowItem in items)
                        {
                            if (menuWindowItem.ShortcutKey == Event.current.keyCode)
                            {
                                menuWindowItem.Select();
                                Event.current.Use();
                                break;
                            }
                        }
//                        var popMenuWindowItem = items[UnityEngine.Random.Range(0, items.Count - 1)];
//                        popMenuWindowItem.Select();
                    }
                    //                this.PageTo();
                    
                }
            }
        }

        #endregion

        /// <summary>
        /// The pop menu window content item.
        /// </summary>
        protected abstract class PopMenuWindowContentItem : PopMenuWindowItem
        {
            #region Static Fields

            /// <summary>
            /// The color 1.
            /// </summary>
            protected static Color color1 = new Color(1, 1, 1, 0.25f);

            #endregion

            #region Fields

            /// <summary>
            /// The name.
            /// </summary>
            public string Name;

            /// <summary>
            /// The on.
            /// </summary>
            public bool On;

            /// <summary>
            /// The pop menu window
            /// </summary>
            protected PopMenuWindow PopMenuWindow;

            #endregion

            #region Public Properties

            /// <summary>
            /// Gets the height.
            /// </summary>
            public int Height
            {
                get
                {
                    return 18;
                }
            }

            #endregion

            #region Public Methods and Operators

            public PopMenuWindowContentItem(PopMenuWindow popMenu)
            {
                this.PopMenuWindow = popMenu;
            }

            /// <summary>
            /// The show.
            /// </summary>
            /// <returns>
            /// The <see cref="bool"/>.
            /// </returns>
            public override bool Show()
            {
                GUILayout.BeginVertical(GUILayout.Height(this.Height));
                GUILayout.BeginHorizontal();
                bool button = false;
                if (this.PopMenuWindow.HasSelectTag)
                {
                    button = this.Button(this.On ? "√" : "  ",Color.clear,GUILayout.Width(15),GUILayout.Height(this.Height));
                }
                bool onShow = this.OnShow();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                return onShow || button;
            }
            public override string GetItemName()
            {
                return this.Name;
            }

            public void UpdateShortcutKey()
            {
                try
                {
                    if (this.ShortcutKey == KeyCode.None && this.Name != null && this.Name.Length >= 4
                        && this.Name[this.Name.Length - 1] == ')' && this.Name[this.Name.Length - 3] == '(')
                    {
                        var upper = this.Name[this.Name.Length - 2].ToString().ToUpper();
                        this.ShortcutKey = (KeyCode)Enum.Parse(typeof(KeyCode), upper);
                    }
                }
                catch
                {
                    
                }
            }

            #endregion

            #region Methods

            /// <summary>
            /// The button.
            /// </summary>
            /// <param name="text">
            /// The text.
            /// </param>
            /// <param name="backColor">
            /// The back color.
            /// </param>
            /// <param name="option">
            /// The option.
            /// </param>
            /// <returns>
            /// The <see cref="bool"/>.
            /// </returns>
            protected bool Button(string text, Color backColor, params GUILayoutOption[] option)
            {
                Color backgroundColor = GUI.backgroundColor;
                var textAnchor = GUI.skin.button.alignment;

                GUI.backgroundColor = backColor;
                GUI.skin.button.alignment = TextAnchor.MiddleLeft;
                bool button = GUILayout.Button(text, option);
                GUI.backgroundColor = backgroundColor;
                GUI.skin.button.alignment = textAnchor;

                return button;
            }

            /// <summary>
            /// The on show.
            /// </summary>
            /// <returns>
            /// The <see cref="bool"/>.
            /// </returns>
            protected abstract bool OnShow();

            #endregion
        }

        /// <summary>
        /// The pop menu window item.
        /// </summary>
        protected abstract class PopMenuWindowItem
        {
            #region Fields

            /// <summary>
            /// The is content.
            /// </summary>
            public bool IsContent;

            /// <summary>
            /// 快捷键
            /// </summary>
            public KeyCode ShortcutKey = KeyCode.None;

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// The select.
            /// </summary>
            public abstract void Select();

            /// <summary>
            /// The show.
            /// </summary>
            /// <returns>
            /// The <see cref="bool"/>.
            /// </returns>
            public abstract bool Show();

            /// <summary>
            /// Gets the name of the item.
            /// </summary>
            /// <returns>System.String.</returns>
            public abstract string GetItemName();


            #endregion
        }

        /// <summary>
        /// The pop menu window item 1.
        /// </summary>
        protected class PopMenuWindowItem1 : PopMenuWindowContentItem
        {
            #region Fields

            /// <summary>
            /// The func.
            /// </summary>
            private Action func;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="PopMenuWindowItem1" /> class.
            /// </summary>
            /// <param name="content">The content.</param>
            /// <param name="on">The on.</param>
            /// <param name="func">The func.</param>
            /// <param name="popMenu">The pop menu.</param>
            public PopMenuWindowItem1(string content, bool on, Action func, PopMenuWindow popMenu)
                : base(popMenu)
            {
                this.func = func;
                this.IsContent = true;
                this.Name = content;
                this.On = on;
                this.UpdateShortcutKey();
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// The select.
            /// </summary>
            public override void Select()
            {
                this.func();
                this.PopMenuWindow.CloseWindow();
            }

            #endregion

            #region Methods

            /// <summary>
            /// The on show.
            /// </summary>
            /// <returns>
            /// The <see cref="bool"/>.
            /// </returns>
            protected override bool OnShow()
            {
                return this.Button(this.Name, GUI.backgroundColor, GUILayout.Height(this.Height));
            }

            #endregion
        }

        /// <summary>
        /// The pop menu window item 2.
        /// </summary>
        protected class PopMenuWindowItem2 : PopMenuWindowContentItem
        {
            #region Fields

            /// <summary>
            /// The func.
            /// </summary>
            private Action<object> func;

            /// <summary>
            /// The user data.
            /// </summary>
            private object userData;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="PopMenuWindowItem2" /> class.
            /// </summary>
            /// <param name="content">The content.</param>
            /// <param name="on">The on.</param>
            /// <param name="func">The func.</param>
            /// <param name="userData">The user data.</param>
            /// <param name="popMenu">The pop menu.</param>
            public PopMenuWindowItem2(string content, bool on, Action<object> func, object userData, PopMenuWindow popMenu)
                : base(popMenu)
            {
                this.userData = userData;
                this.func = func;
                this.IsContent = true;
                this.Name = content;
                this.On = on;
                this.UpdateShortcutKey();
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// The select.
            /// </summary>
            public override void Select()
            {
                this.func(this.userData);
                this.PopMenuWindow.CloseWindow();
            }

            #endregion

            #region Methods

            /// <summary>
            /// The on show.
            /// </summary>
            /// <returns>
            /// The <see cref="bool"/>.
            /// </returns>
            protected override bool OnShow()
            {
                return this.Button(this.Name, GUI.backgroundColor, GUILayout.Height(this.Height));
            }

            #endregion
        }

        /// <summary>
        /// The pop menu window item disable.
        /// </summary>
        protected class PopMenuWindowItemDisable : PopMenuWindowContentItem
        {
            #region Static Fields

            /// <summary>
            /// The color disable.
            /// </summary>
            private static Color colorDisable = new Color(0.76f, 0.76f, 0.76f, 0.25f);

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="PopMenuWindowItemDisable" /> class.
            /// </summary>
            /// <param name="content">The content.</param>
            /// <param name="on">The on.</param>
            /// <param name="popMenu">The pop menu.</param>
            public PopMenuWindowItemDisable(string content, bool on, PopMenuWindow popMenu)
                : base(popMenu)
            {
                this.IsContent = true;
                this.Name = content;
                this.On = on;
            }

            #endregion

            #region Public Methods and Operators

            /// <summary>
            /// The select.
            /// </summary>
            /// <exception cref="NotImplementedException">
            /// </exception>
            public override void Select()
            {
                throw new NotImplementedException();
            }

            #endregion

            #region Methods

            /// <summary>
            /// The on show.
            /// </summary>
            /// <returns>
            /// The <see cref="bool"/>.
            /// </returns>
            protected override bool OnShow()
            {
                this.Button(this.Name.SetColor(Color.gray), colorDisable, GUILayout.Height(this.Height));
                return false;
            }

            #endregion
        }

        /// <summary>
        /// The pop menu window item separator.
        /// </summary>
        protected class PopMenuWindowItemJump : PopMenuWindowContentItem
        {
            #region Public Methods and Operators

            /// <summary>
            /// The menu window
            /// </summary>
            private PopMenuWindow menuWindow;

            /// <summary>
            /// The show name
            /// </summary>
            private string showName;

            /// <summary>
            /// Initializes a new instance of the <see cref="PopMenuWindowItemJump"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="popMenuWindow">The pop menu window.</param>
            /// <param name="popMenu">The pop menu.</param>
            public PopMenuWindowItemJump(string name, PopMenuWindow popMenuWindow, PopMenuWindow popMenu)
                : base(popMenu)
            {
                this.menuWindow = popMenuWindow;
                this.showName = name+ "▶";
                this.Name = name;
                this.UpdateShortcutKey();
            }

            public override void Select()
            {
                menuWindow.PageTo(this.Name);
            }

            protected override bool OnShow()
            {
                return this.Button(this.showName, new Color(0.3f, 0.5f, 0.6f), GUILayout.Height(this.Height));
            }

            public override string GetItemName()
            {
                return this.Name;
            }

            #endregion
        }
    }
}