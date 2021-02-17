namespace Assets.Tools.Script.Editor.Tool
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Assets.Tools.Script.Attributes;
    using Assets.Tools.Script.Editor.Inspector.Field;
    using Assets.Tools.Script.Editor.Window;
    using Assets.Tools.Script.Reflec;

    using UnityEditor;

    using UnityEngine;

    public class GUITable<T> where T:class
    {
        public bool IsDirty = true;

        public GUITable(int tableWidth, int tableHeight)
        {
            this.tableHeight = tableHeight;
            this.tableWidth = tableWidth;
            Type type = typeof(T);
            this.fieldRealWidth = new Dictionary<string, Rect>();
            fieldWantWidth = new Dictionary<string, int>();
            fieldEnable = new Dictionary<FieldInfo, bool>();
            this.fieldToNameDic = new Dictionary<FieldInfo, string>();
            nameToFieldDic=new Dictionary<string, FieldInfo>();
            fieldList = new List<FieldInfo>();
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (field.GetCustomAttributes(typeof(HideInInspector), true).FirstOrDefault() != null)
                {
                    //跳过不显示的内容
                    continue;
                }
                string fieldName = GetFieldName(field);
                this.fieldRealWidth.Add(fieldName, new Rect());
                fieldEnable.Add(field, true);
                fieldWantWidth.Add(fieldName, 100);
                this.fieldToNameDic.Add(field, fieldName);
                nameToFieldDic.Add(fieldName, field);
                fieldList.Add(field);
                if (currSearchFieldName.IsNullOrEmpty())
                {
                    currSearchFieldName = fieldName;
                }
            }
        }

        private Dictionary<string, Rect> fieldRealWidth;
        private Dictionary<string, int> fieldWantWidth;
        private Dictionary<FieldInfo, bool> fieldEnable;
        private Dictionary<FieldInfo, string> fieldToNameDic;
        private Dictionary<string, FieldInfo> nameToFieldDic;
        private List<FieldInfo> fieldList;

        private List<T> objects = new List<T>();

        private string search=string.Empty;

        private string currSearchFieldName = "";

        private string currSortFieldName = "";

        public void BeginView()
        {
            if (IsDirty)
            {
                objects.Clear();
            }
            
            List<string> list = this.fieldRealWidth.Keys.ToList();
            foreach (var key in list)
            {
                this.fieldRealWidth[key] = new Rect();
            }
        }

        public void EndView()
        {
            IsDirty = false;
            Show();
        }

        public void ShowItem(T o)
        {
            if (IsDirty)
            {
                objects.Add(o);
            }
        }

        private Rect tableRect = new Rect(0,0,0,0);

        private void Show()
        {
//            GUILayout.BeginHorizontal();
//            GUILayout.Button("xx222xx");
//            GUILayout.Button("xx222xx");
//            GUILayout.BeginVertical();
//            GUILayout.Button("xx222xx");
//            GUILayout.Button("xx222xx");

            //---------------------------------------------//
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
            Rect lastRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.Repaint)
            {
                tableRect = new Rect(lastRect.position, new Vector2(tableWidth, tableHeight));
            }
            GUILayout.BeginArea(tableRect, "", (GUIStyle)"flow overlay box");
            ShowTable();
            GUILayout.EndArea();
            GUILayout.BeginHorizontal();
            GUILayout.Space(tableWidth);
            GUILayout.BeginVertical();
            GUILayout.Space(tableHeight);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            //---------------------------------------------//

//            GUILayout.Button("xx222xx");
//            GUILayout.EndVertical();
//            GUILayout.Button("xx222xx");
//            GUILayout.EndHorizontal();
        }

        private void ShowTable()
        {
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
            if (Event.current.type == EventType.Repaint)
            {
                this.startRect = GUILayoutUtility.GetLastRect();
            }

            GUILayout.BeginVertical();
            DrawToolBar();
            GUILayout.Space(22);
            DrawTable();
            GUILayout.EndVertical();

            DrawTitle(this.startRect);
//            GUILayout.EndVertical();
        }

        private void DrawBackground()
        {
            if (lastTableRect.size.x > 1)
            {
                Debug.Log(lastTableRect.size.ToV2String());
                GUI.Box(new Rect(10, 10, 100, 150), string.Empty);
            }
        }

        private void DrawToolBar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("字段", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                PopCheckboxWindow checkbox = new PopCheckboxWindow();
                checkbox.OnSelectChange += (name, on) =>
                    {
                        fieldEnable[nameToFieldDic[name]] = on;
                        checkbox.FromWindow.Repaint();
                    };

                foreach (var field in fieldEnable)
                {
                    checkbox.AddItem(fieldToNameDic[field.Key], field.Value);
                }
                checkbox.PopWindow();
            }

            if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                IsDirty = true;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("查找 " + currSearchFieldName, EditorStyles.toolbarDropDown))
            {
                GenericMenu menu = new GenericMenu();
                GenericMenu.MenuFunction2 onSelect = (fieldName) =>
                    { currSearchFieldName = fieldName as string; };
                foreach (var key in fieldEnable.Keys)
                {
                    string fieldName = fieldToNameDic[key];
                    if (!fieldEnable[key])
                    {
                        continue;
                    }
                    menu.AddItem(new GUIContent(fieldName), fieldName == currSearchFieldName, onSelect, fieldName);
                }
                menu.ShowAsContext();
            }
            search = EditorGUILayout.TextField(search, (GUIStyle)"ToolbarSeachTextField");
            
            if (GUILayout.Button("", (GUIStyle)"ToolbarSeachCancelButton"))
            {
                search = string.Empty;
                GUIUtility.keyboardControl = 0;
            }

            GUILayout.EndHorizontal();
        }

        private void DrawTitle(Rect startRect)
        {
            GUILayout.BeginHorizontal();
            foreach (var key in this.fieldRealWidth.Keys)
            {
                Rect rect = this.fieldRealWidth[key];
                if (rect.size.x > 2)
                {
                    Rect buttonRect = new Rect(rect.x + startRect.x - scrollPosition.x, startRect.y + 20, rect.size.x, 20);
                    if (this.TitleButton(key, buttonRect))
                    {
                        if (currSortFieldName == key)
                        {
                            currSortFieldName = "";
                            this.objects.Sort(
                            (l, r) =>
                                {
                                    int compare = GetComparerValue(l, r, key);
                                    return compare * -1;
                                });
                            GUI.FocusControl("");
                        }
                        else
                        {
                            currSortFieldName = key;
                            this.objects.Sort(
                            (l, r) =>
                                { return GetComparerValue(l, r, key); });
                            GUI.FocusControl("");
                        }
                    }

                    int resizeButton = this.ResizeButton(key, new Rect(buttonRect.x + buttonRect.size.x, buttonRect.y, 3, buttonRect.size.y));
                    fieldWantWidth[key] += resizeButton;
                }
            }
            DrawMovedFieldTitle();
            GUILayout.EndHorizontal();
        }

        private Vector2 scrollPosition;

        private Rect startRect;

        private Rect lastTableRect;

        private int GetComparerValue(T l, T r, string fieldName)
        {
            FieldInfo fieldInfo = typeof(T).GetField(fieldName);
            if (fieldInfo == null)
            {
                FieldInfo[] fieldInfos = typeof(T).GetFields();

                foreach (var info in fieldInfos)
                {
                    InspectorStyle inspectorStyle = info.GetAttribute<InspectorStyle>();
                    if (inspectorStyle != null && inspectorStyle.Name == fieldName)
                    {
                        fieldInfo = info;
                        break;
                    }
                }
            }

            object valueL = fieldInfo.GetValue(l);
            object valueR = fieldInfo.GetValue(r);
            return Comparer.Default.Compare(valueL, valueR);
        }

        private void DrawTable()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.BeginVertical();

            List<string> unsupportField = new List<string>();

            for (int i = 0; i < this.objects.Count; i++)
            {
                var o = this.objects[i];
                if (search.IsNOTNullOrEmpty())
                {
                    bool finded = false;
                    foreach (var field in fieldList)
                    {
                        string fieldName = GetFieldName(field);
                        if (search.IsNOTNullOrEmpty() && fieldName == currSearchFieldName)
                        {
                            object value = field.GetValue(o);
                            if (value.ToString().Contains(search))
                            {
                                finded = true;
                                break;
                            }
                        }
                    }
                    if (!finded)
                    {
                        continue;
                    }
                }
                
                if (i % 2 == 0)
                {
                    GUILayout.BeginVertical(GUITool.GetAreaGUIStyle(new Color(0f, 0f, 0f, 0.35f)));
                }
                else
                {
                    GUILayout.BeginVertical(GUITool.GetAreaGUIStyle(new Color(1f, 1f, 1f, 0.1f)));
                }

                GUILayout.BeginHorizontal();
                foreach (var field in fieldList)
                {
                    if (field.GetCustomAttributes(typeof(HideInInspector), true).FirstOrDefault() != null)
                    {
                        //跳过不显示的内容
                        continue;
                    }
                    string fieldName = GetFieldName(field);

                    if (!fieldEnable[field])
                    {
                        //跳过不显示的内容
                        continue;
                    }

                    GUILayout.BeginHorizontal(GUILayout.Width(fieldWantWidth[fieldName]));
                    field.SetValue(
                        o,
                        FieldInspectorTool.GenericField(field.Name, field.GetValue(o), field.FieldType, field, o, false));
                    GUILayout.EndHorizontal();

                    Rect lastRect = GUILayoutUtility.GetLastRect();

                    if (lastRect.size.x > 2 && lastRect.size.y > 2)
                    {
                        if (this.fieldRealWidth[fieldName].position.x < lastRect.position.x)
                        {
                            this.fieldRealWidth[fieldName] = lastRect;
                        }
                    }
                    if (lastRect.size.y == 0)
                    {
                        //可能没被支持的字段
                        unsupportField.Add(fieldName);
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            foreach (var fieldName in unsupportField)
            {
                fieldEnable[nameToFieldDic[fieldName]] = false;
            }
        }

        private string GetFieldName(FieldInfo fieldInfo)
        {
            InspectorStyle inspectorStyle = fieldInfo.GetAttribute<InspectorStyle>();
            string name = inspectorStyle == null ? fieldInfo.Name : inspectorStyle.Name;
            return name;
        }

        private int lastpickPosition;
        private string pickResizeName;
        private int ResizeButton(string name, Rect rect)
        {
            var e = Event.current;
//            GUI.Box(rect,"");

            if (pickResizeName == name)
            {
                EditorGUIUtility.AddCursorRect(
                    new Rect(0, 0, Screen.width, Screen.height),
                    MouseCursor.ResizeHorizontal);
                if (e.type == EventType.MouseUp)
                {
                    pickResizeName = null;
                }
            }
            else if (rect.Contains(e.mousePosition) && e.type == EventType.MouseDown)
            {
                pickResizeName = name;
                lastpickPosition = (int)e.mousePosition.x;
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
            }
            else
            {
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
            }
            
            if (pickResizeName == name)
            {
                int d = (int)e.mousePosition.x - lastpickPosition;
                lastpickPosition = (int)e.mousePosition.x;
                EditorWindow.focusedWindow.Repaint();
                return d;
            }
            return 0;
        }

        private string pickIndexName;

        private bool startMoveField;

        private int tableWidth;

        private int tableHeight;

        private void DrawMovedFieldTitle()
        {
            if (startMoveField)
            {
                Rect rect = fieldRealWidth[pickIndexName];
                var currEvent = Event.current;
                GUI.Button(new Rect(currEvent.mousePosition, rect.size), pickIndexName, (GUIStyle)"LargeButtonMid");
            }
        }

        private bool TitleButton(string name, Rect rect)
        {
            var currEvent = Event.current;
            EventType eventType = currEvent.type;
            Vector2 mousePosition = currEvent.mousePosition;
            bool clickTitle = GUI.Button(rect, name, (GUIStyle)"LargeButtonMid");

            if (pickIndexName == name)
            {
                if (Mathf.Abs(lastpickPosition - (int)mousePosition.x) > 5)
                {
                    startMoveField = true;
                }
                if (startMoveField)
                {
                    clickTitle = false;
                }
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.MoveArrow);
                if (eventType == EventType.MouseUp)
                {
                    //移动字段位置结束
                    pickIndexName = null;
                    startMoveField = false;
                    float mouseX = mousePosition.x;
                    int moveToIndex = -1;

                    for (int i = 0; i < this.fieldList.Count; i++)
                    {
                        if (!fieldEnable[fieldList[i]])
                        {
                            continue;
                        }
                        Rect fieldRect = new Rect();
                        Rect nextFieldRect = new Rect(9999, 9999, 9999, 9999);
                        for (int j = i; j >= 0; j--)
                        {
                            FieldInfo fieldInfo = fieldList[j];
                            if (fieldEnable[fieldInfo])
                            {
                                fieldRect = fieldRealWidth[fieldToNameDic[fieldInfo]];
                                break;
                            }
                        }
                        for (int j = i + 1; j < fieldList.Count; j++)
                        {
                            FieldInfo fieldInfo = fieldList[j];
                            if (fieldEnable[fieldInfo])
                            {
                                nextFieldRect = fieldRealWidth[fieldToNameDic[fieldInfo]];
                                break;
                            }
                        }

                        if (mouseX > fieldRect.x + startRect.x - scrollPosition.x && mouseX < nextFieldRect.x + startRect.x - scrollPosition.x)
                        {
                            moveToIndex = i;
                            break;
                        }
                    }
                    if (moveToIndex >= 0)
                    {
                        int findIndex = fieldList.FindIndex(element => fieldToNameDic[element] == name);
                        if (moveToIndex > findIndex)
                        {
                            FieldInfo fieldInfo = fieldList[findIndex];
                            fieldList.RemoveAt(findIndex);
                            fieldList.Insert(moveToIndex, fieldInfo);
                        }
                        else if (moveToIndex < findIndex)
                        {
                            FieldInfo fieldInfo = fieldList[findIndex];
                            fieldList.RemoveAt(findIndex);
                            fieldList.Insert(moveToIndex, fieldInfo);
                        }
                    }
                }
            }
            else if (rect.Contains(mousePosition) && eventType == EventType.MouseDown)
            {
                pickIndexName = name;
                lastpickPosition = (int)mousePosition.x;
            }
            return clickTitle;
        }
    }
}