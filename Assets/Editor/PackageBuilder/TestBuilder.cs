using UnityEngine;
using UnityEditor;

public class TestBuilder:BasePackageBuilder
{
    [MenuItem("PackageBuilder/Test")]
    public static void ShowWin()
    {
        BasePackageBuilder.OpenWindow<TestBuilder>();
    }

    protected override void ImportAsset()
    {
        Debug.Log("ImportAsset");
    }

    protected override void OnStartBuild()
    {
        Debug.Log("OnStartBuild");
    }

    protected override void OnEndBuild()
    {
        Debug.Log("OnEndBuild");
    }

    

    protected override void RemoveAsset()
    {
        Debug.Log("RemoveAsset");
    }

    protected override void OnBuild()
    {
        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes , buildPath+"/cow.exe" , BuildTarget.StandaloneWindows64 , BuildOptions.None);
    }
}
