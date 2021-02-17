using System.Collections.Generic;

using UnityEngine;

using Object = UnityEngine.Object;

public class GUITool
{
    private static readonly Dictionary<Color,Texture2D> _colorTexture2Ds=new Dictionary<Color, Texture2D>();

    public static void Init()
    {
    }

    public static void Clear()
    {
        foreach (var value in _colorTexture2Ds.Values)
        {
            Object.DestroyImmediate(value);
        }
        _colorTexture2Ds.Clear();
    }

    public static Texture2D GetTexture2D(Color color)
    {
        foreach (var key in _colorTexture2Ds.Keys)
        {
            if (color.Equals(key) && _colorTexture2Ds[key] != null)
            {
                return _colorTexture2Ds[key];
            }
        }
        var texture2D = new Texture2D(4, 4);

        for (int i = 0; i < texture2D.width; i++)
        {
            for (int j = 0; j < texture2D.height; j++)
            {
                texture2D.SetPixel(i, j, color);
            }
        }
            
        texture2D.Apply();
        _colorTexture2Ds[color] = texture2D;
        return texture2D;
    }

    public static GUIStyle GetAreaGUIStyle()
    {
        return GetAreaGUIStyle(new Color(0.15f, 0.15f, 0.15f, 0.5f));
    }

    public static GUIStyle GetAreaGUIStyle(Color color)
    {
        GUIStyle guiStyle = new GUIStyle();
            
        guiStyle.normal.background = GetTexture2D(color);
//            guiStyle.fixedHeight = 18;
            
//            guiStyle.stretchHeight = true;
//            guiStyle.padding = new RectOffset(1, 1, 1, 1);
//            guiStyle.overflow = new RectOffset(1, 1, 1, 1);
//            guiStyle.margin = new RectOffset(1, 1, 1, 1);
//            guiStyle.border = new RectOffset(1, 1, 1, 1);

        return guiStyle;
    }

    public static void Line(int pixel)
    {
        GUILayout.BeginVertical(GetAreaGUIStyle());
        GUILayout.Space(pixel);
        GUILayout.EndVertical();
    }

    public static bool Button(string text, TextAnchor anchor)
    {
        TextAnchor textAnchor = GUI.skin.button.alignment;
        GUI.skin.button.alignment = anchor;
        if (GUILayout.Button(text))
        {
            GUI.skin.button.alignment = textAnchor;
            return true;
        }
        GUI.skin.button.alignment = textAnchor;
        return false;
    }

    public static bool Button(string text, Color backColor)
    {
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = backColor;
        if (GUILayout.Button(text))
        {
            GUI.backgroundColor = backgroundColor;
            return true;
        }
        GUI.backgroundColor = backgroundColor;
        return false;
    }

    public static bool Button(string text, Color backColor, params GUILayoutOption[] options)
    {
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = backColor;
        if (GUILayout.Button(text, options))
        {
            GUI.backgroundColor = backgroundColor;
            return true;
        }
        GUI.backgroundColor = backgroundColor;
        return false;
    }

    public static bool Button(string text, Color backColor, GUIStyle style, params GUILayoutOption[] options)
    {
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = backColor;
        if (GUILayout.Button(text, style, options))
        {
            GUI.backgroundColor = backgroundColor;
            return true;
        }
        GUI.backgroundColor = backgroundColor;
        return false;
    }

    public static bool Button(string text, Color backColor, TextAnchor anchor)
    {
        TextAnchor textAnchor = GUI.skin.button.alignment;
        GUI.skin.button.alignment = anchor;
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = backColor;
        if (GUILayout.Button(text))
        {
            GUI.skin.button.alignment = textAnchor;
            GUI.backgroundColor = backgroundColor;
            return true;
        }
        GUI.skin.button.alignment = textAnchor;
        GUI.backgroundColor = backgroundColor;
        return false;
    }

    public static bool LabelButton(string text, params GUILayoutOption[] options)
    {
        return GUILayout.Button(text, "ControlLabel", options);
    }

    public static void TextFieldComment(string check, string comment)
    {
        TextFieldComment(check, comment, GUILayoutUtility.GetLastRect());
    }
    public static void TextFieldComment(string check, string comment,Rect rect)
    {
        if (string.IsNullOrEmpty(check))
        {
            Color color = GUI.color;
            GUI.color = new Color(1, 1, 1, 0.3f);
            GUI.Label(rect, " <i>" + comment + "</i>");
            GUI.color = color;
        }
    }
}
