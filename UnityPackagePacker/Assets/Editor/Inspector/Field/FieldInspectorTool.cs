namespace Assets.Tools.Script.Editor.Inspector.Field
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using Assets.Tools.Script.Attributes;
    using Assets.Tools.Script.Editor.Inspector.Type;
    //using Assets.Tools.Script.Editor.Tool;
    using Assets.Tools.Script.Editor.Window;
    using Assets.Tools.Script.Reflec;

    using UnityEditor;

    using UnityEngine;

    public class FieldInspectorTool
    {
        public static Dictionary<string, FieldInspectorParser> inspectorFieldParsers;
        public static Dictionary<Type, DefaultTypeInspector> inspectorTypeParsers;

        private static void Init()
        {
            if (inspectorFieldParsers != null && inspectorTypeParsers != null)
            {
                return;
            }

            inspectorFieldParsers = new Dictionary<string, FieldInspectorParser>();
            List<Type> parserTypes = AssemblyTool.FindTypesInCurrentDomainWhereExtend<FieldInspectorParser>();
            foreach (var parserType in parserTypes)
            {
                var obj = ReflecTool.Instantiate(parserType);
                Debug.Log(parserType.ToString() + "   " + (obj == null));
                FieldInspectorParser fieldInspectorParser = obj as FieldInspectorParser;
                Debug.Log(parserType.ToString()+"   " + (fieldInspectorParser == null));
                inspectorFieldParsers.Add(fieldInspectorParser.Name, fieldInspectorParser);
            }

            inspectorTypeParsers = new Dictionary<Type, DefaultTypeInspector>();
            parserTypes = AssemblyTool.FindTypesInCurrentDomainWhereExtend<DefaultTypeInspector>();
            foreach (var parserType in parserTypes)
            {
                DefaultTypeInspector defaultTypeInspector = ReflecTool.Instantiate(parserType) as DefaultTypeInspector;
                inspectorTypeParsers.Add(defaultTypeInspector.GetInspectorType(), defaultTypeInspector);
            }
        }

        /// <summary>
        /// 显示object的showFileds内指定的public字段和[SerializeField]字段，除外
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="showFileds">The show fileds.</param>
        /// <param name="showNames">The show names.</param>
        public static void ShowObjectWith(object o, Dictionary<string, bool> showFileds, Dictionary<string, string> showNames = null)
        {
            List<FieldInfo> fields = new List<FieldInfo>();
            List<string> fieldNames = null;
            if (showNames != null)
            {
                fieldNames = new List<string>();
            }
            foreach (var field in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!showFileds.ContainsKey(field.Name))
                {
                    continue;
                }
                fields.Add(field);
                if (fieldNames != null)
                {
                    fieldNames.Add(showNames.ContainsKey(field.Name) ? showNames[field.Name] : field.Name);
                }
            }

            List<Type> types = o.GetType().GetBaseClassTypes(true);
            foreach (var interfaceType in types)
            {
                foreach (var field in interfaceType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (!showFileds.ContainsKey(field.Name))
                    {
                        continue;
                    }
                    if (field.GetAttribute<SerializeField>() == null)
                    {
                        continue;
                    }
                    fields.Add(field);
                    if (fieldNames != null)
                    {
                        fieldNames.Add(showNames.ContainsKey(field.Name) ? showNames[field.Name] : field.Name);
                    }
                }
            }
            ShowObjectFields(o, fields, fieldNames);
        }

        /// <summary>
        /// 显示object的所有public字段和[SerializeField]字段，dontShowFileds除外
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="dontShowFileds">The dont show fileds.</param>
        /// <param name="showNames">The show names.</param>
        public static void ShowObjectWithOut(object o, Dictionary<string, bool> dontShowFileds, Dictionary<string, string> showNames = null)
        {
            List<FieldInfo> fields = new List<FieldInfo>();
            List<string> fieldNames = null;
            if (showNames != null)
            {
                fieldNames = new List<string>();
            }

            foreach (var field in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (dontShowFileds.ContainsKey(field.Name))
                {
                    continue;
                }
                fields.Add(field);
                if (fieldNames != null)
                {
                    fieldNames.Add(showNames.ContainsKey(field.Name) ? showNames[field.Name] : field.Name);
                }
            }

            List<Type> types = o.GetType().GetBaseClassTypes(true);
            foreach (var interfaceType in types)
            {
                foreach (var field in interfaceType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (dontShowFileds.ContainsKey(field.Name))
                    {
                        continue;
                    }
                    if (field.GetAttribute<SerializeField>() == null)
                    {
                        continue;
                    }
                    fields.Add(field);
                    if (fieldNames != null)
                    {
                        fieldNames.Add(showNames.ContainsKey(field.Name) ? showNames[field.Name] : field.Name);
                    }
                }
            }
            ShowObjectFields(o, fields, fieldNames);
        }

        /// <summary>
        /// 显示object的所有public字段和[SerializeField]字段
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="showNames">The show names.</param>
        public static void ShowObject(object o, Dictionary<string, string> showNames = null)
        {
            List<FieldInfo> fields = new List<FieldInfo>();
            List<string> fieldNames = null;
            if (showNames != null)
            {
                fieldNames = new List<string>();
            }

            foreach (var field in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                fields.Add(field);
                if (fieldNames != null)
                {
                    fieldNames.Add(showNames.ContainsKey(field.Name) ? showNames[field.Name] : field.Name);
                }
            }

            List<Type> types = o.GetType().GetBaseClassTypes(true);
            foreach (var interfaceType in types)
            {
                foreach (var field in interfaceType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (field.GetAttribute<SerializeField>() == null)
                    {
                        continue;
                    }
                    fields.Add(field);
                    if (fieldNames != null)
                    {
                        fieldNames.Add(showNames.ContainsKey(field.Name) ? showNames[field.Name] : field.Name);
                    }
                }
            }

            ShowObjectFields(o, fields, fieldNames);
        }

        /// <summary>
        /// 显示object的字段
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="fieldNames">The field names.</param>
        public static void ShowObjectFields(object o, List<FieldInfo> fields, List<string> fieldNames = null)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (field.GetCustomAttributes(typeof(HideInInspector), true).FirstOrDefault() != null)
                {
                    //跳过不显示的内容
                    continue;
                }
                object value = field.GetValue(o);

                string name = fieldNames != null ? fieldNames[i] : field.Name;
                field.SetValue(
                    o,
                    GenericField(name, value, value == null ? field.FieldType : value.GetType(), field, o));
            }
        }

        public static List<FieldInfo> GetAllFields(Type o)
        {
            List<FieldInfo> fields = new List<FieldInfo>();
            foreach (var field in o.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                fields.Add(field);
            }

            List<Type> types = o.GetBaseClassTypes(true);
            foreach (var interfaceType in types)
            {
                foreach (var field in interfaceType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (field.GetAttribute<SerializeField>() == null)
                    {
                        continue;
                    }
                    fields.Add(field);
                }
            }
            return fields;
        }

        public static object GenericField(object instance, string fieldName, bool withName = true)
        {
            var fieldInfo = instance.GetType().GetField(fieldName,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
            return GenericField(
                fieldInfo.GetBestName(),
                fieldInfo.GetValue(instance),
                fieldInfo.FieldType,
                fieldInfo,
                instance,
                withName);
        }

        public static object GenericField(
            string name,
            object value,
            Type t,
            FieldInfo fieldInfo,
            object instance,
            bool withName = true)
        {
            if (t == null)
            {
                return value;
            }

            Init();

            if (fieldInfo != null)
            {
                //检查是否有字段解析支持
                InspectorStyle inspectorStyle = fieldInfo.GetAttribute<InspectorStyle>();
                FieldInspectorParser parser = null;

                if (inspectorStyle != null)
                {
                    name = inspectorStyle.Name;
                    if (inspectorStyle.ParserName != null)
                    {
                        inspectorFieldParsers.TryGetValue(inspectorStyle.ParserName, out parser);
                    }
                }

                if (parser != null)
                {
                    return parser.ParserFiled(inspectorStyle, value, t, fieldInfo, instance, withName);
                }
            }

            //检查是否有默认类型解析
            if (inspectorTypeParsers.ContainsKey(t))
            {
                return inspectorTypeParsers[t].Show(name, value, t, fieldInfo, instance, withName);
            }

            //默认显示
            if (t == typeof(string))
            {
                if (withName)
                    return EditorGUILayout.TextField(name, (string)value);
                return EditorGUILayout.TextField((string)value);
            }

            if (t == typeof(bool))
            {
                if (withName)
                    return EditorGUILayout.Toggle(name, (bool)value);
                return EditorGUILayout.Toggle((bool)value);
            }

            if (t == typeof(int))
            {
                if (withName)
                    return EditorGUILayout.IntField(name, (int)value);
                return EditorGUILayout.IntField((int)value);
            }

            if (t == typeof(uint))
            {
                if (withName)
                    return (uint)EditorGUILayout.IntField(name, Convert.ToInt32((uint)value));
                return (uint)EditorGUILayout.IntField(Convert.ToInt32((uint)value));
            }

            if (t == typeof(float))
            {
                if (withName)
                    return EditorGUILayout.FloatField(name, (float)value);
                return EditorGUILayout.FloatField((float)value);
            }

            if (t == typeof(Vector2))
            {
                if (withName)
                    return EditorGUILayout.Vector2Field(name, (Vector2)value);
                return EditorGUILayout.Vector2Field("", (Vector2)value);
            }

            if (t == typeof(Vector3))
            {
                if (withName)
                    return EditorGUILayout.Vector3Field(name, (Vector3)value);
                return EditorGUILayout.Vector3Field("", (Vector3)value);
            }

            if (t == typeof(Vector4))
            {
                if (withName)
                    return EditorGUILayout.Vector4Field(name, (Vector4)value);
                return EditorGUILayout.Vector4Field("", (Vector4)value);
            }


            if (t == typeof(Quaternion))
            {
                var quat = (Quaternion)value;
                var vec4 = new Vector4(quat.x, quat.y, quat.z, quat.w);
                if (withName)
                {
                    vec4 = EditorGUILayout.Vector4Field(name, vec4);
                }
                else
                {
                    vec4 = EditorGUILayout.Vector4Field("", vec4);
                }

                return new Quaternion(vec4.x, vec4.y, vec4.z, vec4.w);
            }

            if (t == typeof(Color))
            {
                if (withName)
                    return EditorGUILayout.ColorField(name, (Color)value);
                return EditorGUILayout.ColorField((Color)value);
            }

            if (t == typeof(Rect))
            {
                if (withName)
                    return EditorGUILayout.RectField(name, (Rect)value);
                return EditorGUILayout.RectField((Rect)value);
            }

            if (t == typeof(AnimationCurve))
            {
                if (withName)
                    return EditorGUILayout.CurveField(name, (AnimationCurve)value);
                return EditorGUILayout.CurveField((AnimationCurve)value);
            }

            if (t == typeof(Bounds))
            {
                if (withName)
                    return EditorGUILayout.BoundsField(name, (Bounds)value);
                return EditorGUILayout.BoundsField((Bounds)value);
            }

            if (typeof(IList).IsAssignableFrom(t))
                return ListEditor(name, (IList)value, t);

            if (t.IsSubclassOf(typeof(System.Enum)))
            {
                bool hasAttribute = t.HasAttribute<FlagsAttribute>();
                if (hasAttribute)
                {
                    if (withName)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(name, GUILayout.Width(150));
                        FlagsEnumEditor(value, fieldInfo, instance);
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        FlagsEnumEditor(value, fieldInfo, instance);
                    }
                    return fieldInfo.GetValue(instance);
                }
                else
                {
                    if (withName)
                        return EditorGUILayout.EnumPopup(name, (System.Enum)value);
                    return EditorGUILayout.EnumPopup((System.Enum)value);
                }
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(t))
            {
                if (withName) return EditorGUILayout.ObjectField(name, (UnityEngine.Object)value, t, true);
                return EditorGUILayout.ObjectField((UnityEngine.Object)value, t, true);
            }

            if (value == null)
            {
                try
                {
                    value = ReflecTool.Instantiate(fieldInfo.FieldType);
                    fieldInfo.SetValue(instance, value);
                }
                catch (Exception e)
                {
                    // ignored
                }
            }

            if (value != null)
            {
                GUILayout.BeginVertical();
                if (withName)
                {
                    GUILayout.Label(name);
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical();
                ShowObject(value);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            return value;
        }

        public static FieldInspectorParser GetFieldInspectorParser(string inspectorName)
        {
            Init();
            FieldInspectorParser parser;
            inspectorFieldParsers.TryGetValue(inspectorName, out parser);
            return parser;
        }

        public static object FlagsEnumEditor(object enumValue, FieldInfo field, object instance)
        {
            Type type = enumValue.GetType();
            int value = (int)enumValue;
            string[] strings = Enum.GetNames(type);
            StringBuilder builder = new StringBuilder();
            foreach (var s in strings)
            {
                object o = Enum.Parse(type, s);
                int currValue = (int)o;
                bool selected = (currValue & value) > 0;
                if (selected)
                {
                    builder.Append(s);
                    builder.Append("|");
                }
            }
            if (GUILayout.Button(builder.ToString()))
            {
                PopCustomWindow popCustomWindow = PopCustomWindow.ShowPopWindow();
                popCustomWindow.DrawGUI = () =>
                    {
                        foreach (var s in strings)
                        {
                            object o = Enum.Parse(type, s);
                            int currValue = (int)o;
                            bool selected = (currValue & value) > 0;
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(s);
                            bool endSelect = EditorGUILayout.Toggle(selected);
                            GUILayout.EndHorizontal();
                            if (selected != endSelect)
                            {
                                if (endSelect)
                                {
                                    value += currValue;

                                }
                                else
                                {
                                    value -= currValue;
                                }
                            }
                        }
                        field.SetValue(instance, value);
                    };
            }
            return value;
        }

        private static readonly Dictionary<object, bool> registeredEditorFoldouts = new Dictionary<object, bool>();
        public static IList ListEditor(string prefix, IList list, Type listType)
        {
            if (list == null)
            {
                list = ReflecTool.Instantiate(listType) as IList;
            }
            var argType = listType.IsArray ? listType.GetElementType() : listType.GetGenericArguments()[0];

            //register foldout
            if (!registeredEditorFoldouts.ContainsKey(list))
                registeredEditorFoldouts[list] = false;

            GUILayout.BeginVertical();

            var foldout = registeredEditorFoldouts[list];
            foldout = EditorGUILayout.Foldout(foldout, prefix);
            registeredEditorFoldouts[list] = foldout;

            if (!foldout)
            {
                GUILayout.EndVertical();
                return list;
            }

            if (list.Equals(null))
            {
                GUILayout.Label("Null List");
                GUILayout.EndVertical();
                return list;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();
            if (GUILayout.Button("Add Element"))
            {

                if (listType.IsArray)
                {

                    list = ResizeArray((Array)list, list.Count + 1);
                    registeredEditorFoldouts[list] = true;

                }
                else
                {
                    object newElement = null;
                    if (argType.IsValueType)
                    {
                        newElement = Activator.CreateInstance(argType);
                    }
                    if (!argType.IsInterface && !argType.IsAbstract && AssemblyTool.FindTypesInCurrentDomainWhereExtend(argType).Count == 1)
                    {
                        try
                        {
                            newElement = Activator.CreateInstance(argType);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    list.Add(newElement);
                }
                GUI.FocusControl("");
            }

            //            EditorGUI.indentLevel++;

            for (var i = 0; i < list.Count; i++)
            {


                GUILayout.BeginVertical(GUITool.GetAreaGUIStyle(new Color(Color.grey.r, Color.grey.g, Color.grey.b, 0.3f)));
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                ShowObject(list[i]);
                GUILayout.EndVertical();
                //                list[i] = GenericField("Element " + i, list[i], argType, null, null, false);
                if (GUILayout.Button("X", GUILayout.Width(18)))
                {

                    if (listType.IsArray)
                    {

                        list = ResizeArray((Array)list, list.Count - 1);
                        registeredEditorFoldouts[list] = true;

                    }
                    else
                    {

                        list.RemoveAt(i);
                    }
                    GUI.FocusControl("");
                }
                GUILayout.EndHorizontal();
                GUITool.Line(2);
                GUILayout.EndVertical();

            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            //            EditorGUI.indentLevel--;
            Separator();

            GUILayout.EndVertical();
            return list;
        }

        static System.Array ResizeArray(System.Array oldArray, int newSize)
        {
            int oldSize = oldArray.Length;
            System.Type elementType = oldArray.GetType().GetElementType();
            System.Array newArray = System.Array.CreateInstance(elementType, newSize);
            int preserveLength = System.Math.Min(oldSize, newSize);
            if (preserveLength > 0)
                System.Array.Copy(oldArray, newArray, preserveLength);
            return newArray;
        }

        public static void Separator()
        {
            GUI.backgroundColor = Color.black;
            GUILayout.Box("", GUILayout.MaxWidth(Screen.width), GUILayout.Height(2));
            GUI.backgroundColor = Color.white;
        }
    }
}