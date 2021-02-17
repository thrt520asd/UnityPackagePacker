namespace Assets.Tools.Script.Editor.Inspector.Type.TypeParser
{
    using System;
    using System.Reflection;

    using Assets.Tools.Script.Editor.Window;

    using UnityEditor;

    using UnityEngine;

    public class DatetimeTypeInspector : DefaultTypeInspector
    {
        public override Type GetInspectorType()
        {
            return typeof(DateTime);
        }

        public override object Show(string name, object value, Type t, FieldInfo fieldInfo, object instance, bool withName = true)
        {
            string textField = "";

            if (withName)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(name,GUILayout.Width(146));
            }
            var dateTime = value is DateTime ? (DateTime)value : new DateTime();
            
            if (GUILayout.Button(dateTime.ToString("yyyy-MM-dd HH:mm:ss")))
            {
                string str = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                var popCustomWindow = new PopCustomWindow();
                popCustomWindow.DrawGUI = () =>
                    {
                        str = EditorGUILayout.TextField(str,GUILayout.Width(200));
                        try
                        {
                            value = Convert.ToDateTime(str);
                            fieldInfo.SetValue(instance, value);
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e);
                        }
                    };
                popCustomWindow.PopWindow();
            }

            if (withName)
            {
                GUILayout.EndHorizontal();
            }

//            if (withName)
//            {
//                textField = EditorGUILayout.TextField(name,value.ToString());
//            }
//            else
//            {
//                textField = EditorGUILayout.TextField(value.ToString());
//            }
//            try
//            {
//                value = Convert.ToDateTime(textField);
//            }
//            catch (Exception e)
//            {
//                
//            }
            

            return value;
        }
    }
}