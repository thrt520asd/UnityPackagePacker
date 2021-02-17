using System;
using System.Collections.Generic;
using UnityEditor;

namespace ChannelBuilder.Importer
{
    public interface IPlugin
    {
        string Name { get; }
        List<Type> Dependencies { get; }
        void PreBuild(ChannelConfig channelConfig, BuildTarget target, BuildCfg cfg);
        void EndBuild(string pathToProject);
        void ImportAsset();
        void RemoveAsset();

        void CopyToChannelAsset();
    }
}