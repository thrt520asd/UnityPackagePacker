using Assets.Tools.Script.Helper;
using UnityEngine;

namespace Assets.Tools.Script.Editor.Tool
{
    public static class GUIEditorTool
    {
        public static string SetColor(this string str, string color)
        {
            return string.Format("<color=#{0}>{1}</color>", color, str);
        }

        public static string SetColor(this string str, Color color)
        {
            return string.Format("<color=#{0}>{1}</color>", ColorTool.GetRGBHexadecimal(color), str);
        }

        public static string SetSize(this string str, int size, bool bold = false)
        {
            if (bold)
            {
                return string.Format("<b><size={0}>{1}</size></b>", size, str);
            }
            
            return string.Format("<size={0}>{1}</size>", size, str);
        }

        public static string SetBold(this string str)
        {
            return string.Format("<b>{0}</b>", str);
        }
    }
}