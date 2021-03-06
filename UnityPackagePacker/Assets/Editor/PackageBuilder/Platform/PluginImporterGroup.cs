﻿namespace Assets.Scripts.Platform.Editor.Plugins
{
    using System;
    using System.Collections.Generic;

    using Assets.Tools.Script.Reflec;

    using UnityEditor;

    using UnityEngine;

    public class PluginImporterGroup
    {
        private Dictionary<Type, PluginImporter> importers = new Dictionary<Type, PluginImporter>();

        private List<PluginImporter> importerList = new List<PluginImporter>();

        public PluginImporterGroup Use(Type importType)
        {
            if (!this.importers.ContainsKey(importType))
            {
                this.importers.Add(importType, null);
                var pluginImporter = ReflecTool.Instantiate(importType) as PluginImporter;

                //引用导入插件的依赖插件
                foreach (var dependency in pluginImporter.Dependencies)
                {
                    this.Use(dependency);
                }

                this.importers[importType] = pluginImporter;
                this.importerList.Add(pluginImporter);
            }
            return this;
        }

        public void PreBuild(BuildTarget target)
        {
            foreach (var pluginImporter in this.importerList)
            {
                try
                {
                    pluginImporter.PreBuild(target);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }

        public void EndBuild(string pathToBuiltProject)
        {
            foreach (var pluginImporter in this.importerList)
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