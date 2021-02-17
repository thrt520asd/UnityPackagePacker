using Assets.Tools.Script.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

public class EditorTools
{
    public static void AddScriptingDefineSymbol(BuildTargetGroup group, string symbol)
    {
        string scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        var strings = scriptingDefineSymbols.Split(';').ToList();
        if (!strings.Contains(symbol))
        {
            strings.Add(symbol);
        }
        var joint = strings.Joint(";");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, joint);
    }

    public static void RemoveScriptingDefineSymbol(BuildTargetGroup group, string symbol)
    {
        string scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        var strings = scriptingDefineSymbols.Split(';').ToList();
        strings.Remove(symbol);
        var joint = strings.Joint(";");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, joint);
    }
}

