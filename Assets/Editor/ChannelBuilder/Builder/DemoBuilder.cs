using ChannelBuilder.Importer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ChannelBuilder.Builder
{
    public class DemoBuilder : IBuilder
    {
        public string Name => "Demo";
        static string[] BUILD_SCENES ={"Assets/Scenes/Main.unity",};
        private static PluginGroup _pluginImporterGroup;
        private static ChannelConfig _config;
        public void Build(ChannelConfig config, BuildTarget target, BuildCfg cfg)
        {
            string outPutPath = Application.dataPath;
            outPutPath = outPutPath.Replace("Assets", "");
            outPutPath += "/bin/";
            BuildOptions buildOption = BuildOptions.None;
            if (cfg.debug)
            {
                buildOption |= BuildOptions.Development;
                buildOption |= BuildOptions.AllowDebugging;
                buildOption |= BuildOptions.ConnectWithProfiler;
            }
            string filename="";
            string exportFilePath = "";
            DateTime time = DateTime.Now;
            string timeStamp = time.Year.ToString("D2") + time.Month.ToString("D2") + time.Day.ToString("D2") + "_" +
                                               time.Hour.ToString("D2") + time.Minute.ToString("D2");
            if (cfg.useIL2CPP)
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            else
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            if (target == BuildTarget.Android)
            {
                outPutPath = Path.Combine(outPutPath, "Andorid");
                if (!Directory.Exists(outPutPath))
                {
                    Directory.CreateDirectory(outPutPath);
                }
                
                filename =timeStamp + "demo.apk";

                exportFilePath = outPutPath + "/" + filename;
            }
            else
            {
                outPutPath = Path.Combine(outPutPath, "iOS");
                outPutPath += timeStamp;
                exportFilePath = outPutPath;
            }

            BuildPipeline.BuildPlayer(BUILD_SCENES, exportFilePath, target, buildOption);

        }

        public void OnEndBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (_pluginImporterGroup != null)
            {
                try
                {

                    _pluginImporterGroup.Clean(pathToBuiltProject);

                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
                finally
                {

                }
            }
        }

        public void OnFailed()
        {
            Debug.Log("onFailed");
            RevertConfig();
            if (_pluginImporterGroup != null)
            {
                _pluginImporterGroup.RemoveAsset();
            }
        }

        public void Prepare(ChannelConfig config, BuildTarget buildTarget, BuildCfg cfg)
        {
            Debug.Log("准备打包" + config.name);
            //todo 打包加速
            //CopyRes(config, buildTarget, cfg);
            PreparePlugin(config, buildTarget, cfg);
            SetConfig_Compile(config, buildTarget, cfg);
        }

        private void SetConfig_Compile(ChannelConfig config, BuildTarget target, BuildCfg cfg)
        {
            Debug.Log("SetConfig_Compile");
            Tools.WriteChannelConst(config);
            Tools.AddScriptingDefineSymbols(config.DefineSymbol);
        }

        public void RevertSimulate(ChannelConfig config, BuildTarget target, BuildCfg cfg)
        {
            _config = config;
            RevertConfig();
            RemovePlugin(config, target, cfg);
        }
        private void RemovePlugin(ChannelConfig config, BuildTarget target, BuildCfg cfg)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            _pluginImporterGroup = new PluginGroup();
            string[] plugins = config.plugins.Split(';');

            for (int i = 0; i < plugins.Length; i++)
            {
                if (!string.IsNullOrEmpty(plugins[i]))
                {
                    Debug.Log("usePlugin" + plugins[i]);
                    _pluginImporterGroup.Use(plugins[i]);
                }
            }
            _pluginImporterGroup.RemoveAsset();
        }


        private static void RevertConfig()
        {
            if (_config != null)
            {
                Tools.RemoveScritpingDefineSymbols(_config.DefineSymbol);
            }
            else
            {
                Debug.Log("_config null");
            }
        }

        private void PreparePlugin(ChannelConfig config, BuildTarget target, BuildCfg cfg)
        {
            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            _pluginImporterGroup = new PluginGroup();
            string[] plugins = config.plugins.Split(';');

            for (int i = 0; i < plugins.Length; i++)
            {
                if (!string.IsNullOrEmpty(plugins[i]))
                {
                    Debug.Log("usePlugin" + plugins[i]);
                    _pluginImporterGroup.Use(plugins[i]);
                }
            }
            _pluginImporterGroup.Prepare(config, target, cfg);
        }
        public void Simulate(ChannelConfig config, BuildTarget target, BuildCfg cfg)
        {
            SetConfig_Compile(config, target, cfg);
            PreparePlugin(config, target, cfg);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}
