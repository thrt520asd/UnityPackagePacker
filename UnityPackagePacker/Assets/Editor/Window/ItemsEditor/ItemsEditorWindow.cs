namespace Assets.Tools.Script.Editor.Window
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Assets.Tools.Script.Editor.Tool;
    //using Assets.Tools.Script.Editortool;
    using Assets.Tools.Script.Helper;
    using Assets.Tools.Script.Reflec;

    using UnityEditor;

    using UnityEngine;

    public abstract class ItemsEditorWindow<T> : EditorWindow where T : class
    {
        public T CurrSelect
        {
            get
            {
                return currSelect;
            }
            set
            {
                GUI.FocusControl("");
                currSelect = value;
                this.OnSelect(currSelect);
            }
        }

        private T currSelect;

        private List<ItemDetailPartInspector<T>> itemDetailPartInspectors = new List<ItemDetailPartInspector<T>>();

        private object inited;

        private Vector2 listScrollView;
        private Vector2 detailScrollView;

        protected abstract string GetItemName(T item);

        protected abstract bool GetSource(out List<T> source);

        protected abstract void ShowSourceView();

        protected abstract void ShowMenu();

        protected virtual void ShowDetail()
        {
            if (CurrSelect == null)
            {
                return;
            }
            detailScrollView = GUILayout.BeginScrollView(detailScrollView, GUIStyle.none);
            foreach (var itemDetailPartInspector in itemDetailPartInspectors)
            {
                itemDetailPartInspector.Show(CurrSelect);
            }

            GUILayout.Space(400);
            GUILayout.EndScrollView();
        }

        protected virtual void OnSelect(T selectData) { }

        protected virtual void OnInit() { }

        protected virtual void ShowListView()
        {
            List<T> source;
            bool b = GetSource(out source);
            if (b)
            {
                CurrSelect = source.FirstOrDefault(e => e == CurrSelect);
            }

            listScrollView = GUILayout.BeginScrollView(listScrollView);
            foreach (var item in source)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);

                if (item == CurrSelect)
                {
                    GUITool.Button(this.GetItemName(item).SetSize(16, true).SetColor(Color.white), new Color(0, 0, 0, 0.3f));
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    if (GUITool.Button(this.GetItemName(item), Color.clear, TextAnchor.MiddleLeft))
                    {
                        CurrSelect = item;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }


        protected virtual void OnGUIEnd() { }

        private void OnGUI()
        {
            try
            {
                Init();
                GUI.skin.label.richText = true;
                GUI.skin.button.richText = true;
                GUI.skin.box.richText = true;
                GUI.skin.textArea.richText = true;
                GUI.skin.textField.richText = true;
                GUI.skin.toggle.richText = true;
                GUI.skin.window.richText = true;

                GUILayout.BeginVertical();
                ShowMenu();
                GUILayout.BeginHorizontal();

                GUI.Box(new Rect(0, 17, 298, Screen.height), "");
                GUI.Box(new Rect(300, 17, Screen.width, Screen.height), "");
                GUILayout.BeginVertical(GUILayout.Width(295));
                ShowSourceView();
                ShowListView();
                GUILayout.EndVertical();

                GUILayout.Space(7);
                ShowDetail();
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                OnGUIEnd();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
        }

        private void Init()
        {
            if (inited == null)
            {
                List<Type> partInspectors = AssemblyTool.FindTypesInCurrentDomainWhereExtend<ItemDetailPartInspector<T>>();
                foreach (var partInspector in partInspectors)
                {
                    ItemDetailPartInspector<T> itemDetailPartInspector = ReflecTool.Instantiate(partInspector) as ItemDetailPartInspector<T>;
                    itemDetailPartInspectors.Add(itemDetailPartInspector);
                }
                itemDetailPartInspectors.Sort(
                    (l, r) =>
                        { return l.Order - r.Order; });
                OnInit();
                inited = new object();
            }
        }
    }
}