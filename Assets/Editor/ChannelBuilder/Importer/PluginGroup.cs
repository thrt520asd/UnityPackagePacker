using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEditor;

namespace ChannelBuilder.Importer
{
    public class PluginGroup
    {
        private Dictionary<Type, IPlugin> importers = new Dictionary<Type, IPlugin>();

        private List<IPlugin> importerList = new List<IPlugin>();

        public PluginGroup Use(string importerName)
        {
            var type = Tools.GetPluginImporterType(importerName);
            if (type != null)
            {
                Use(type);
            }
            else
            {
                Debug.LogError("找不到Importer" + importerName);
            }

            return this;
        }

        public PluginGroup Use(Type importType)
        {
            if (!importers.ContainsKey(importType))
            {
                importers.Add(importType, null);
                var pluginImporter = ReflectTool.Instantiate(importType) as IPlugin;

                //引用导入插件的依赖插件
                foreach (var dependency in pluginImporter.Dependencies)
                {
                    this.Use(dependency);
                }

                importers[importType] = pluginImporter;
                importerList.Add(pluginImporter);
            }
            return this;
        }

        public void PreBuild(ChannelConfig channelConfig, BuildTarget target,BuildCfg cfg)
        {
            foreach (var pluginImporter in importerList)
            {
                try
                {
                    pluginImporter.PreBuild(channelConfig,target,cfg);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }

        public void Prepare(ChannelConfig channelConfig, BuildTarget target, BuildCfg cfg)
        {
            if (cfg.importAssets)
            {
                ImportAsset();
            }
            
            PreBuild(channelConfig,target,cfg);
            
        }

        public void Clean(string path)
        {
            EndBuild(path);
            RemoveAsset();
        }

        public void ImportAsset()
        {
            foreach (var pluginImporter in importerList)
            {
                try
                {
                    pluginImporter.ImportAsset();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }

        public void CopyToAsset()
        {
            foreach (var pluginImporter in importerList)
            {
                try
                {
                    pluginImporter.RemoveAsset();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }

        public void RemoveAsset()
        {
            foreach (var pluginImporter in importerList)
            {
                try
                {
                    pluginImporter.RemoveAsset();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }

        public void EndBuild(string pathToBuiltProject)
        {
            foreach (var pluginImporter in importerList)
            {
                try
                {
                    pluginImporter.EndBuild(pathToBuiltProject);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }
    }
    
}