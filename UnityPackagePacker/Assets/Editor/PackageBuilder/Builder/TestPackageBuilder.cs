using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestPackageBuilder : PackageBuilder {

    [MenuItem("Builder/Test")]
    public static void Open()
    {
        OpenWindow<TestPackageBuilder>();
    }

    public override int Platform
    {
        get { return PlatformType.Internal; }
    }

    protected override void ImportPlatformAssets()
    {
        Debug.Log("import assets");
    }

    protected override void OnBuild()
    {
        
        Debug.Log("build  ");
    }

    protected override void OnEndBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log("end  build  ");
    }

    protected override void OnStartBuild()
    {
        Debug.Log("start  build  ");
    }

    protected override void RemovePlatformAssets()
    {
        Debug.Log("remove assets");
    }

    
}
