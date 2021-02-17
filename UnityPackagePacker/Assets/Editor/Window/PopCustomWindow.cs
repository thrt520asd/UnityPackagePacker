namespace Assets.Tools.Script.Editor.Window
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Assets.Tools.Script.Editor.Tool;
    //using Assets.Tools.Script.Helper;

    using UnityEngine;

    public class PopCustomWindow : PopEditorWindow<PopCustomWindow>
    {
        public Action DrawGUI;

        public Action<object> DrawGUIWith;

        public Action OnClose; 

        public object CacheArg;
        public object CacheObject;

        protected override void DrawOnGUI()
        {
            if (CacheArg != null && DrawGUIWith != null)
            {
                DrawGUIWith(CacheArg);
            }
            else
            {
                if (DrawGUI != null)
                {
                    DrawGUI();
                }
            }
        }
        protected override void PreClose()
        {
            if (OnClose != null)
            {
                OnClose();
            }
            base.PreClose();
        }
    }
}