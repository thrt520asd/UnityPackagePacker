using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using Assets.Tools.Script.Attributes;
using UnityEditor.Callbacks;
using System.Text;
using Assets.Tools.Script.Editor.Inspector.Field;

public abstract class PackageBuilder : EditorWindow {

    private static PackageBuilder currBuilder;

    public static void OpenWindow<T>() where T : PackageBuilder
    {
        var type = typeof(T);
        PackageBuilder editorWindow = EditorWindow.GetWindow(type) as PackageBuilder;
        var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var fieldInfo in fieldInfos)
        {
            fieldInfo.SetValue(editorWindow, editorWindow.GetCacheValue(fieldInfo));
        }
    }

    public static PackageBuilder OpenWindow(Type type)
    {
        PackageBuilder editorWindow = EditorWindow.GetWindow(type) as PackageBuilder;
        var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var fieldInfo in fieldInfos)
        {
            fieldInfo.SetValue(editorWindow, editorWindow.GetCacheValue(fieldInfo));
        }
        return editorWindow;
    }

    /// <summary>
    /// Called before [OnStartBuild].
    /// </summary>
    protected abstract void ImportPlatformAssets();

    /// <summary>
    /// Called before [build].
    /// </summary>
    protected abstract void OnStartBuild();

    /// <summary>
    /// Called when [build].
    /// </summary>
    protected abstract void OnBuild();

    /// <summary>
    /// Called after [OnEndBuild].
    /// </summary>
    protected abstract void RemovePlatformAssets();

    /// <summary>
    /// Called when [end build].
    /// </summary>
    protected abstract void OnEndBuild(BuildTarget target, string pathToBuiltProject);

    public abstract int Platform { get; }

    public string OverrideBundleId = "";

    [InspectorStyle("产品名称（默认不填）", "")]
    public string productName;

    [InspectorStyle("图标[带扩展名]（默认不填）", "")]
    public string IconPath = "";

    [InspectorStyle("导出路径", "PathFieldSelector")]
    public string ExportPath;

    [InspectorStyle("上传父目录", "")]
    public string UploadFolder;
    /// <summary>
    /// The real export path
    /// </summary>
    protected string RealExportPath;

    /// <summary>
    /// 缓存改变包名，用于还原
    /// </summary>
    protected string bundleIdentifierCache = null;

    protected int buildProcess = 0;

    private BuildTarget endBuildTarget;

    private string endBuildProjectPath;

    private string bfProductName;



    private void OnGUI()
    {
        GUI.skin.label.richText = true;
        GUI.skin.button.richText = true;
        GUI.skin.box.richText = true;
        GUI.skin.textArea.richText = true;
        GUI.skin.textField.richText = true;
        GUI.skin.toggle.richText = true;
        GUI.skin.window.richText = true;

        //GUILayout.Label(PlatformType.GetName(this.Platform).SetSize(20).SetBold());
        GUILayout.Label(PlatformType.GetName(this.Platform));

        FieldInspectorTool.ShowObject(this);

        var type = this.GetType();
        var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var fieldInfo in fieldInfos)
        {
            this.SaveCacheValue(fieldInfo);
        }

        if (GUITool.Button("生成", Color.green))
        {
            bfProductName = PlayerSettings.productName;
            if (!string.IsNullOrEmpty( productName.Trim())) PlayerSettings.productName = productName.Trim();
            try
            {
                this.Build();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                PlayerSettings.productName = bfProductName;
            }
        }

        this.PlatformGUI();
    }

    protected virtual void Build()
    {
        if (string.IsNullOrEmpty(this.ExportPath))
        {
            this.ShowNotification(new GUIContent("缺少导出路径"));
            return;
        }
        
        currBuilder = this;
        if (!string.IsNullOrEmpty(this.OverrideBundleId))
        {
            this.bundleIdentifierCache = PlayerSettings.applicationIdentifier;
            PlayerSettings.applicationIdentifier = this.OverrideBundleId;
        }
        if (this.OverrideBundleId.IsNOTNullOrEmpty())
        {
            this.bundleIdentifierCache = PlayerSettings.applicationIdentifier;
            PlayerSettings.applicationIdentifier = this.OverrideBundleId;
        }
        //修改版本号
        //PlayerSettings.bundleVersion = ClientConfig.GetVerStr(this.Config.Version);
        //PlayerSettings.Android.bundleVersionCode = this.Config.Version;
        //PlayerSettings.iOS.buildNumber = this.Config.Version.ToString();
        this.ImportPlatformAssets();
        this.OnStartBuild();

        this.OnBuild();
    }


    [PostProcessBuild(100)]
    private static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        //if (currBuilder != null)
        //{
        //currBuilder.EndBuild(target, pathToBuiltProject);
        //    currBuilder.buildProcess = 98;
        //    currBuilder.endBuildTarget = target;
        //    currBuilder.endBuildProjectPath = pathToBuiltProject;
        //}
        currBuilder.EndBuild(target, pathToBuiltProject);
    }

    private void EndBuild(BuildTarget target, string pathToBuiltProject)
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        
        this.OnEndBuild(target, pathToBuiltProject);
        
        this.RemovePlatformAssets();
        //
        if (!string.IsNullOrEmpty(this.bundleIdentifierCache))
        {
            PlayerSettings.applicationIdentifier = this.bundleIdentifierCache;
            this.bundleIdentifierCache = null;
        }

        //如果可以，打开生成的文件夹
        try
        {
            var replace = this.ExportPath.Replace('/', '\\');
            System.Diagnostics.Process.Start("explorer.exe", replace);
        }
        catch
        {
            // ignored
        }

        AssetDatabase.Refresh();

        currBuilder = null;

        if (!string.IsNullOrEmpty(UploadFolder))
        {
            //TODO  upload package
            //UploadPackage(UploadFolder);
        }
        EditorUtility.DisplayDialog("build succeed", "build succeed", "ok");
    }

    /// <summary>
    /// 平台自定义GUI
    /// </summary>
    protected virtual void PlatformGUI()
    {

    }




    #region Cache
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
    #endregion
}
