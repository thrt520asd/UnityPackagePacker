using System;
using System.Collections.Generic;
using UnityEditor;

public abstract class PluginImporter
{
    public List<Type> Dependencies = new List<Type>();
    protected BuildTarget Target;
    public void PreBuild(BuildTarget target)
    {
        this.Target = target;
        this.OnPreBuild();
    }

    public void EndBuild(string pathToBuiltProject)
    {
        this.OnEndBuild(pathToBuiltProject);
    }

    protected abstract void OnPreBuild();
    public abstract void ImportAsset();
    protected abstract void OnEndBuild(string pathToBuiltProject);
    public abstract void RemoveAsset();
}
