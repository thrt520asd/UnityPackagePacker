namespace Assets.Tools.Script.Editor.Window
{
    using System;

    using Assets.Tools.Script.Editor.Tool;
    using Assets.Tools.Script.Reflec;

    using UnityEditor;

    using UnityEngine;

    public abstract class PopEditorWindow : EditorWindow
    {
        public EditorWindow FromWindow { get; protected set; }

        protected bool AutoAdjustSize = true;

        public Vector2 DefaultSize;

        public Vector2 ScrollViewPosition;

        /// <summary>
        /// 关闭回调
        /// </summary>
        public Action OnCloseHandler = null;

        public void PopWindow()
        {
            if (DefaultSize == Vector2.zero)
            {
                AutoAdjustSize = true;
                DefaultSize = new Vector2(100, 100);
            }
            else
            {
                AutoAdjustSize = false;
            }

            if (EditorWindow.focusedWindow != this)
            {
                this.FromWindow = EditorWindow.focusedWindow;
            }
            
            Vector2 popPosition = Vector2.zero;
            if (Event.current != null)
            {
                popPosition = Event.current.mousePosition;
            }
            else
            {
                popPosition = EditorWindow.focusedWindow.position.position;
            }
            Vector2 guiToScreenPoint = GUIUtility.GUIToScreenPoint(popPosition);
            this.ShowAsDropDown(new Rect(guiToScreenPoint.x, guiToScreenPoint.y, 0, 0), DefaultSize);

            this.OnOpen();
        }

        private void OnGUI()
        {
            GUI.skin.label.richText = true;
            GUI.skin.button.richText = true;
            GUI.skin.box.richText = true;
            GUI.skin.textArea.richText = true;
            GUI.skin.textField.richText = true;
            GUI.skin.toggle.richText = true;
            GUI.skin.window.richText = true;

            if (AutoAdjustSize)
            {
                GUILayout.BeginVertical(GUITool.GetAreaGUIStyle(new Color(0.3f, 0.3f, 0.3f)),GUILayout.Width(10), GUILayout.Height(10));
                DrawOnGUI();
                GUILayout.EndVertical();
                Rect lastRect = GUILayoutUtility.GetLastRect();

                if (lastRect.size.x > 5 && lastRect.size.y > 5)
                {
                    this.maxSize = lastRect.size;
                    this.minSize = lastRect.size;
                }
            }
            else
            {
                this.ScrollViewPosition = GUILayout.BeginScrollView(this.ScrollViewPosition, GUITool.GetAreaGUIStyle(new Color(0.3f, 0.3f, 0.3f)));
                DrawOnGUI();
                GUILayout.EndScrollView();
            }

        }

        protected virtual void PreClose()
        {
            
        }

        protected virtual void OnOpen()
        {
            
        }

        private void OnDestroy()
        {
            if (this.OnCloseHandler != null)
            {
                this.OnCloseHandler();
            }
            this.PreClose();
            if (EditorWindow.focusedWindow == null || EditorWindow.focusedWindow == this)
            {
                this.FromWindow.Focus();
            }
        }

        public void CloseWindow()
        {
            this.Close();
        }

        protected abstract void DrawOnGUI();
    }

    public abstract class PopEditorWindow<T> : PopEditorWindow
        where T : PopEditorWindow
    {
        public static T LastPopWindow;

        public static T ShowPopWindow()
        {
            T window = ReflecTool.Instantiate<T>();
            window.PopWindow();
            LastPopWindow = window;
            return window;
        }

        public static T ShowPopWindow(Vector2 size)
        {
            T window = ReflecTool.Instantiate<T>();
            window.DefaultSize = size;
            window.PopWindow();
            
            LastPopWindow = window;
            return window;
        }

        protected override void PreClose()
        {
            LastPopWindow = null;
        }
    }
}