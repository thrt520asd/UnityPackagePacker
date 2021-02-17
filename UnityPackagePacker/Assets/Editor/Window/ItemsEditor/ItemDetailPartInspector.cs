namespace Assets.Tools.Script.Editor.Window
{
    using Assets.Tools.Script.Editor.Tool;

    using UnityEditor;

    using UnityEngine;

    public abstract class ItemDetailPartInspector<T>
    {
        private static GUIStyle PartBackgroundStyle = new GUIStyle("WindowBackground");
        private static GUIStyle PartTitleStyle = new GUIStyle("SelectionRect") {richText = true};

        public abstract string Title { get; }

        protected virtual int PartWidth
        {
            get
            {
                return 10;
            } 
        }

        public void Show(T item)
        {
            if (!this.CheckPartEnable(item))
            {
                return;
            }
            GUILayout.BeginVertical(PartBackgroundStyle, GUILayout.Height(10));
            GUILayout.BeginVertical(GUILayout.Width(this.PartWidth));
            GUILayout.Label(Title.SetSize(28).SetColor("FFF68F"), PartTitleStyle);
            GUILayout.Space(5);
            OnShow(item);
//            GUITool.Line(2);
            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }
        
        public abstract int Order { get; }

        protected virtual bool CheckPartEnable(T item)
        {
            return true;
        }

        protected abstract void OnShow(T item);
    }
}