namespace Assets.Tools.Script.Editor.Inspector.Field.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Assets.Tools.Script.Attributes;
    using Assets.Tools.Script.Reflec;

    using UnityEditor;

    using UnityEngine;

    public class IntEnumInspectorParser : FieldInspectorParser
    {
        public override string Name
        {
            get
            {
                return "IntEnum";
            }
        }

        public override object ParserFiled(
            InspectorStyle style,
            object value,
            Type t,
            FieldInfo fieldInfo,
            object instance,
            bool withName = true)
        {
            var type = style.ParserAgrs[0] as Type;
            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            int currValue = value is int ? (int)value : 0;

            List<FieldInfo> enums = new List<FieldInfo>();
            List<GUIContent> enumNames = new List<GUIContent>();
            int currIndex = -1;
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                var info = fieldInfos[i];
                enums.Add(info);
                var inspectorStyle = info.GetAttribute<InspectorStyle>();
                enumNames.Add(new GUIContent(inspectorStyle == null ? info.Name : inspectorStyle.Name));
                var o = info.GetValue(null) is int ? (int)info.GetValue(null) : 0;
                if (o == currValue)
                {
                    currIndex = i;
                }
            }
            if (currIndex < 0)
            {
                currIndex = 0;
            }

            if (withName)
            {
                currIndex = EditorGUILayout.Popup(new GUIContent(style.Name), currIndex, enumNames.ToArray());
            }
            else
            {
                currIndex = EditorGUILayout.Popup(currIndex, enumNames.ToArray());
            }
            currValue = enums[currIndex].GetValue(null) is int ? (int)enums[currIndex].GetValue(null) : 0;
            return currValue;
        }
    }
}