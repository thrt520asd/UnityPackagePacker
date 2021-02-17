namespace Assets.Tools.Script.Editor.Inspector.Field.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Assets.Tools.Script.Attributes;
    using Assets.Tools.Script.Editor.Tool;
    using Assets.Tools.Script.Editor.Window;

    using UnityEditor;

    using UnityEngine;

    public class ImgNameSelectorInspectorParser : FieldInspectorParser
    {
        public override string Name { get
        {
            return "ImgNameSelector";
        } }
        GUISearchBar<Texture2D> searchBar2 = new GUISearchBar<Texture2D>();
        public override object ParserFiled(
            InspectorStyle style,
            object value,
            Type t,
            FieldInfo fieldInfo,
            object instance,
            bool withName = true)
        {

      

            string[] imgFolderPaths = new string[style.ParserAgrs.Length];
            for (int i = 0; i < style.ParserAgrs.Length; i++)
            {
                var parserAgr = style.ParserAgrs[i];
                imgFolderPaths[i] = parserAgr as string;
            }
            string currName = value as string ?? "";

            
            Texture2D currTexture2D = null;
            try
            {
                var databasePath = this.FieldValueToAssetDatabasePath(currName);
                currTexture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(String.Format("{0}.png", databasePath));
                if (currTexture2D == null)
                {
                    currTexture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(String.Format("{0}.jpg", databasePath));
                }
            }
            catch (Exception)
            {
                
            }

            GUILayout.BeginHorizontal();
            if (withName)
            {
                GUILayout.Label(style.Name, GUILayout.Width(145));
            }
            bool open = false;

            int width = 25, height = 25;

            if (currTexture2D == null)
            {
                open = GUILayout.Button("选择图片");
            }
            else
            {
                if (currTexture2D.width > currTexture2D.height)
                {
                    width = 100;
                    height = currTexture2D.height / (currTexture2D.width / 100);
                }
                else
                {
                    height = 100;
                    width = currTexture2D.width / (currTexture2D.height / 100);
                }
                open = GUILayout.Button(new GUIContent(currTexture2D),GUILayout.Width(width),GUILayout.Height(height));
            }
            if (open)
            {
                OpenSelectWindow(fieldInfo, instance, imgFolderPaths);
            }

            GUILayout.BeginVertical();
            GUILayout.Space(height-20);
            currName = EditorGUILayout.TextField(currName);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            
            return currName;
        }

        private void OpenSelectWindow(FieldInfo fieldInfo, object instance,string[] imgFolderPaths)
        {
            List<Texture2D> texture2Ds = new List<Texture2D>();
            List<Texture2D> texture2Dss = new List<Texture2D>();
            List<Texture2D> texture2Dsss = new List<Texture2D>();
            var findAssets = AssetDatabase.FindAssets("", imgFolderPaths);
            Dictionary<string, string> loaded = new Dictionary<string, string>();
            Dictionary<Texture2D, string> loadedTextureName = new Dictionary<Texture2D, string>();
            Dictionary<Texture2D, string> loadedTexturePath = new Dictionary<Texture2D, string>();
            foreach (var asset in findAssets)
            {
                var guidToAssetPath = AssetDatabase.GUIDToAssetPath(asset);
                if (!loaded.ContainsKey(guidToAssetPath))
                {
                    var loadAssetAtPath = AssetDatabase.LoadAssetAtPath<Texture2D>(guidToAssetPath);
                    var pathSplit = guidToAssetPath.Split('/');
                    var imgName = pathSplit[pathSplit.Length - 1];

                    loaded[guidToAssetPath] = imgName;
                    if (loadAssetAtPath != null)
                    {
                        loadedTextureName[loadAssetAtPath] = imgName;
                        loadedTexturePath[loadAssetAtPath] = guidToAssetPath;
                        texture2Ds.Add(loadAssetAtPath);
                    }
                }
            }

            var popCustomWindow = new PopCustomWindow();
            popCustomWindow.DefaultSize = new Vector2(600, 400);

            popCustomWindow.DrawGUI = () =>
                {
                    texture2Dsss = searchBar2.Draw(texture2Ds, item => item.name);
                    if (searchBar2.SearchContent.IsNOTNullOrEmpty())
                    {
                        texture2Dss = texture2Dsss;

                    }
                    else
                    {
                        texture2Dss = texture2Ds;
                    }
                    string selectPath = null;
                    int i = 0;
                    while (i< texture2Dss.Count)
                    {
                        GUILayout.BeginHorizontal();
                        for (int lineCount = 0; lineCount < 5 && i < texture2Dss.Count; lineCount++)
                        {
                            bool select = false;
                            var texture2D = texture2Dss[i];
                            GUILayout.BeginVertical();
                            select = GUILayout.Button(new GUIContent(texture2D), GUILayout.Width(100), GUILayout.Height(100));
                            GUITool.Button(loadedTextureName[texture2D], Color.clear, GUILayout.MaxWidth(100));
                            GUILayout.EndVertical();
                            i++;

                            if (select)
                            {
                                selectPath = loadedTexturePath[texture2D];
                            }
                        }
                        GUILayout.EndHorizontal();
                    }

                    if (selectPath.IsNOTNullOrEmpty())
                    {
                        var assetDatabasePathToFieldValue = this.AssetDatabasePathToFieldValue(selectPath);
//                        Debug.Log(assetDatabasePathToFieldValue);
                        fieldInfo.SetValue(instance, this.AssetDatabasePathToFieldValue(selectPath));
                        popCustomWindow.CloseWindow();
                    }
                };
            popCustomWindow.PopWindow();
        }

        protected virtual string FieldValueToAssetDatabasePath(string fieldValue)
        {
            return fieldValue;
        }

        protected virtual string AssetDatabasePathToFieldValue(string databasePath)
        {
            return databasePath.Substring(0, databasePath.Length - 4);
        }
    }
}