using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;


//TODO Editor tools
//TODO upload package

/// <summary>
/// Unity打包管理器基类
/// 执行顺序
/// ImportAsset
/// OnStartBuild
/// OnBuild
/// OnEndBuild
/// RemoveAsset
/// </summary>
public abstract class BasePackageBuilder : EditorWindow  
{

    public static void OpenWindow<T>() where T : BasePackageBuilder
    {
        var type = typeof(T);
        BasePackageBuilder editorWindow = EditorWindow.GetWindow(type) as BasePackageBuilder;
        var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        Debug.Log(fieldInfos.Length);
        foreach (var fieldInfo in fieldInfos)
        {
            var cacheVal = editorWindow.GetCacheValue(fieldInfo);
            Debug.Log(cacheVal);
            fieldInfo.SetValue(editorWindow, cacheVal);
        }
    }

    
    private static BasePackageBuilder _curBuilder;
    private static string bfProductName;
    private static string bfBundleId;

    protected virtual void ImportAsset(){}
    protected abstract void OnStartBuild();
    protected abstract void OnEndBuild();
    protected virtual void RemoveAsset(){}
    protected abstract void OnBuild();
    protected virtual void CustomGUI(){}
    public string productName="";
    public string overrideBundleId="";
    public string buildPath;

    private GUIStyle titleStyle;
    void OnEnable()
    {
        titleStyle = new GUIStyle();
        titleStyle.fontSize = 20;
        titleStyle.fontStyle = FontStyle.Bold;

    }

    void OnGUI()
    {
      
        GUILayout.Label(this.GetType().ToString() , titleStyle);
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("产品名称");
            this.productName = GUILayout.TextField(productName);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        {
            if(GUILayout.Button("选择", GUILayout.Width(60)))
            {
                buildPath = EditorUtility.OpenFolderPanel("选择目标路径", Application.dataPath, "defaultName");
            }
            GUILayout.Label(buildPath);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("导入插件"))
            {
                this.ImportAsset();
            }
            if (GUILayout.Button("移除插件"))
            {
                this.RemoveAsset();
            }
        }
        GUILayout.EndHorizontal();
        var fieldInfos = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var fieldInfo in fieldInfos)
        {
            this.SaveCacheValue(fieldInfo);
        }
        if (GUILayout.Button("Build"))
        {
            try
            {
                this.Build();
            }
            catch (Exception e)
            {
                Debug.Log(e);
                PlayerSettings.productName = bfProductName;
            }
            
        }
        this.CustomGUI();
    }

    protected virtual void Build()
    {
        bfBundleId = PlayerSettings.applicationIdentifier;
        if (this.overrideBundleId.Trim().IsNOTNullOrEmpty())
        {
            PlayerSettings.applicationIdentifier = this.overrideBundleId;
        }
        bfProductName = PlayerSettings.productName;
        if (this.productName.Trim().IsNOTNullOrEmpty())
        {
            PlayerSettings.productName = productName.Trim();
        }
        if (string.IsNullOrEmpty(buildPath))
        {
            this.ShowNotification(new GUIContent("打包路径错误 : (" + buildPath+")"));
            return;
        }

        try
        {
            this.ImportAsset();
            this.OnStartBuild();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            _curBuilder = this;
            this.OnBuild();
        }
        catch (Exception e)
        {
            PlayerSettings.productName = bfProductName;
            PlayerSettings.applicationIdentifier = bfBundleId;
            Debug.Log(e);
        }
        
    }


    [PostProcessBuild(100)]
    private static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log("OnPostprocessBuild");
        if (_curBuilder !=null)
        {
            try
            {
                _curBuilder.OnEndBuild();
                _curBuilder.RemoveAsset();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                AssetDatabase.Refresh();
                PlayerSettings.productName = bfProductName;
                PlayerSettings.applicationIdentifier = bfBundleId;
            }
            
#if UNITY_EDITOR_WIN
            var path =  _curBuilder.buildPath.Replace("/"  ,  "\\");
            Debug.Log(path);
            System.Diagnostics.Process.Start("explorer.exe", path);
#endif
        }

        
    }

    private void SaveCacheValue(FieldInfo fieldInfo)
    {
        string cacheName = string.Format("{0}_{1}_{2}", Application.dataPath, this.GetType().FullName, fieldInfo.Name);
        if (fieldInfo.FieldType == typeof(string))
        {
            EditorPrefs.SetString(cacheName, fieldInfo.GetValue(this) as string);
        }
        if (fieldInfo.FieldType == typeof(int))
        {
            EditorPrefs.SetInt(cacheName, (int)fieldInfo.GetValue(this));
        }
        if (fieldInfo.FieldType == typeof(float))
        {
            EditorPrefs.SetFloat(cacheName, (float)fieldInfo.GetValue(this));
        }
        if (fieldInfo.FieldType == typeof(bool))
        {
            EditorPrefs.SetBool(cacheName, (bool)fieldInfo.GetValue(this));
        }
    }

    private object GetCacheValue(FieldInfo fieldInfo)
    {
        string cacheName = string.Format("{0}_{1}_{2}", Application.dataPath, this.GetType().FullName, fieldInfo.Name);
        if (fieldInfo.FieldType == typeof(string))
        {
            return EditorPrefs.GetString(cacheName);
        }
        if (fieldInfo.FieldType == typeof(int))
        {
            return EditorPrefs.GetInt(cacheName);
        }
        if (fieldInfo.FieldType == typeof(float))
        {
            return EditorPrefs.GetFloat(cacheName);
        }
        if (fieldInfo.FieldType == typeof(bool))
        {
            return EditorPrefs.GetBool(cacheName);
        }
        return null;
    }
}

public class PackTools
{
    public static void AddScriptingDefineSymbol(string symbol)
    {
       AddScriptingDefineSymbol(GetCurBuildTargetGroup() , symbol);
        
    }

    public static void RemoveScriptingDefineSymbol(string symbol)
    {
        RemoveScriptingDefineSymbol(GetCurBuildTargetGroup(), symbol);
    }

    public static void AddScriptingDefineSymbol(BuildTargetGroup buildTargetGroup , string symbol)
    {
        string scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        var strings = scriptingDefineSymbols.Split(';').ToList();
        if (!strings.Contains(symbol))
        {
            strings.Add(symbol);
        }
        string joint = "";
        for (int i = 0; i < strings.Count; i++)
        {
            joint += strings[i];
            joint += ";";
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, joint);
    }

    public static void RemoveScriptingDefineSymbol(BuildTargetGroup buildTargetGroup, string symbol)
    {
        string scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        var strings = scriptingDefineSymbols.Split(';').ToList();    
        strings.Remove(symbol);
        string joint = "";
        for (int i = 0; i < strings.Count; i++)
        {
            joint += strings[i];
            joint += ";";
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, joint);
    }

    public static BuildTargetGroup GetCurBuildTargetGroup()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        var group = BuildPipeline.GetBuildTargetGroup(target);
        return group;
    }
}