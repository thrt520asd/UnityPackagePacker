namespace Assets.Tools.Script.Editor.Window
{
    using System;

    using UnityEditor;

    using UnityEngine;

    public class PopInputWindow : PopEditorWindow<PopInputWindow>
    {
        public Action<string> OnInput;

        public string InputString;

        protected override void DrawOnGUI()
        {
            this.InputString = EditorGUILayout.TextArea(this.InputString);
            if (GUILayout.Button("确定",GUILayout.Width(300)))
            {
                if (OnInput != null)
                {
                    OnInput(this.InputString);
                }
                this.CloseWindow();
            }
        }
    }
}