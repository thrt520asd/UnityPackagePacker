namespace Assets.Scripts.Platform.Editor.Tool
{
    using System;
    using System.Reflection;

    using Assets.Tools.Script.Attributes;
    using Assets.Tools.Script.Editor.Inspector.Field;

    using UnityEditor;

    using UnityEngine;

    public class PathFieldSelector : FieldInspectorParser
    {
        public override string Name
        {
            get
            {
                return "PathFieldSelector";
            }
        }

        public override object ParserFiled(InspectorStyle style,object value,Type t,FieldInfo fieldInfo, object instance,bool withName = true)
        {
            EditorGUILayout.BeginHorizontal();
            string path = value as string;
            path = EditorGUILayout.TextField(new GUIContent("导出路径"), path);
            if (GUILayout.Button("选择", GUILayout.Width(40)))
            {
                string savePath = null;
                if (style.ParserAgrs == null || (string)style.ParserAgrs[0] == "Folder")
                {
                    savePath = EditorUtility.SaveFolderPanel("路径", path, "");
                }
                else
                {
                    savePath = EditorUtility.SaveFilePanel("路径", path, "", (string)style.ParserAgrs[0]);
                }
                
                if (savePath.IsNOTNullOrEmpty())
                {
                    path = savePath;
                }
            }
            fieldInfo.SetValue(instance, path);
            EditorGUILayout.EndHorizontal();
            return path;
        }
    }
}