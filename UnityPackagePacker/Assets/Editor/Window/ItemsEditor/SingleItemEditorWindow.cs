namespace Assets.Tools.Script.Editor.Window
{
    using System;
    using System.Collections.Generic;

    using Assets.Tools.Script.Editor.Tool;
    using Assets.Tools.Script.Reflec;

    using UnityEditor;

    using UnityEngine;

    public abstract class SingleItemEditorWindow<T> : EditorWindow where T : class
    {
        public abstract T Data { get; set; }

        protected List<ItemDetailPartInspector<T>> itemDetailPartInspectors;
        private Vector3 detailScrollView;

        private object inited;
        protected virtual void ShowDetail()
        {
            detailScrollView = GUILayout.BeginScrollView(detailScrollView);
            foreach (var itemDetailPartInspector in itemDetailPartInspectors)
            {
                itemDetailPartInspector.Show(Data);
            }
            GUILayout.EndScrollView();
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

            this.PreGUI();
            if (this.Data == null)
            {
                this.InitializeData();
                GUILayout.Label("等待数据...".SetSize(30));
            }
            else
            {
                this.Init();
                this.ShowMenu();
                this.ShowDetail();

                this.OnGUIEnd();
            }
        }

        protected virtual void InitializeData()
        {
            
        }

        protected virtual void PreGUI()
        {
            
        }
        protected virtual void OnGUIEnd()
        {
        }

        private void Init()
        {
            if (this.inited == null)
            {
                this.OnInit();
                this.itemDetailPartInspectors = this.CreatePartInspector();
                this.itemDetailPartInspectors.Sort((l, r) => l.Order - r.Order);
                this.inited = new object();
            }
        }

        protected virtual List<ItemDetailPartInspector<T>> CreatePartInspector()
        {
            List<ItemDetailPartInspector<T>> partInspectors = new List<ItemDetailPartInspector<T>>();
            List<Type> partInspectorTypeSet = AssemblyTool.FindTypesInCurrentDomainWhereExtend<ItemDetailPartInspector<T>>();
            foreach (var partInspector in partInspectorTypeSet)
            {
                ItemDetailPartInspector<T> itemDetailPartInspector = ReflecTool.Instantiate(partInspector) as ItemDetailPartInspector<T>;
                partInspectors.Add(itemDetailPartInspector);
            }
            return partInspectors;
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void ShowMenu()
        {

        }
    }
}