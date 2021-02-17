//using UnityEngine;
//using UnityEditor;
//using ChannelBuilder.Importer;
//using UnityEditor.Callbacks;
//using System;
//using System.IO;
//using System.Reflection;
//using System.Text;


//namespace ChannelBuilder.Builder
//{
//    public class DefaultBuilder : IBuilder
//    {
//        public string Name => "Default";
//#if UNITY_EDITOR_OSX
//	static string BUILD_OUT_ANDROID = "Bin/android";
//#else
//        static string BUILD_OUT_ANDROID = "Bin\\android";
//#endif
//        static string BUILD_OUT_IOS = "Bin/ios/";
//        private static PluginGroup _pluginImporterGroup;
//        private static ChannelConfig _config;
//        static string[] BUILD_SCENES =
//        {
//            "Assets/Scenes/Main.unity",
//        };

//        static string[] BUILD_SCENES_RUNTIMECONSOLE =
//        {
//            "Assets/Scenes/Main_console.unity",
//        };

//        public void Build(ChannelConfig config,BuildTarget buildTarget, BuildCfg cfg)
//        {
//            Debug.Log("DefaultBuilder.Build");
//            PlayerSettings.SplashScreen.showUnityLogo = false;
            
//            SetConfig_App(config, buildTarget, cfg);
//            var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
//            _pluginImporterGroup = new PluginGroup();
//            string[] plugins = config.plugins.Split(';');
//            EditorPrefs.SetString("ChannelCfg", config.name);
//            for (int i = 0; i < plugins.Length; i++)
//            {
//                if (!string.IsNullOrEmpty(plugins[i]))
//                {
//                    Debug.Log("usePlugin" + plugins[i]);
//                    _pluginImporterGroup.Use(plugins[i]);
//                }
//            }
//            string outPutPath = Application.dataPath;
//            outPutPath = outPutPath.Replace("Assets", "");
//            string filename="";
//            bool isbatchmode = Application.isBatchMode;
//            bool runtimeConsole = cfg.runtimeConsole;
//            BuildOptions buildOption = BuildOptions.None;
//            if (cfg.debug)
//            {
//                buildOption |= BuildOptions.Development;
//                buildOption |= BuildOptions.AllowDebugging;
//                buildOption |= BuildOptions.ConnectWithProfiler;
//            }
//            if (BuildTarget.iOS == buildTarget)
//            {
//                var versionstr = PlayerSettings.bundleVersion;
//                PlayerSettings.bundleVersion = versionstr.Substring(0, versionstr.Length - 2);
//                outPutPath = Path.Combine(outPutPath, BUILD_OUT_IOS);
//                DateTime time = DateTime.Now;
//                if (!isbatchmode)
//                    filename = time.Year.ToString("D2") + time.Month.ToString("D2") + time.Day.ToString("D2") + "_" +
//                               time.Hour.ToString("D2") + time.Minute.ToString("D2");
//                else
//                {
//                    filename = time.Year.ToString("D2") + time.Month.ToString("D2") + time.Day.ToString("D2") + "_" +
//                               time.Hour.ToString("D2") + time.Minute.ToString("D2");


//                }

//                outPutPath += filename;
//                if (!Directory.Exists(outPutPath))
//                {
//                    Directory.CreateDirectory(outPutPath);
//                }
//                PlayerSettings.iOS.appleEnableAutomaticSigning = false;
//                if (cfg.runtimeConsole)
//                {
//                    BuildPipeline.BuildPlayer(BUILD_SCENES_RUNTIMECONSOLE, outPutPath, buildTarget, buildOption);
//                }
//                else
//                {
//                    BuildPipeline.BuildPlayer(BUILD_SCENES, outPutPath, buildTarget, buildOption);
//                }
//                var path1 = new System.IO.DirectoryInfo(Application.dataPath + "/../Bin/ios/").FullName;
//            }
//            else if (BuildTarget.Android == buildTarget)
//            {
//                outPutPath = Path.Combine(outPutPath, BUILD_OUT_ANDROID);

//                if (!Directory.Exists(outPutPath))
//                {
//                    Directory.CreateDirectory(outPutPath);
//                }


//                if (cfg.useIL2CPP)
//                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
//                else
//                    PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);


//                string symbolFileName = filename;
//                if (runtimeConsole)
//                {
//                    filename += "[log].apk";
//                    symbolFileName += "[log]";
//                }
//                else
//                {
//                    filename += ".apk";
//                }

//                EditorUserBuildSettings.androidCreateSymbolsZip = !runtimeConsole;
                
//                var exportFilePath = outPutPath + ("/"+ filename);
//                if (runtimeConsole)
//                {
//                    BuildPipeline.BuildPlayer(BUILD_SCENES_RUNTIMECONSOLE, exportFilePath, buildTarget, buildOption);
//                }
//                else
//                {
//                    BuildPipeline.BuildPlayer(BUILD_SCENES, exportFilePath, buildTarget, buildOption);
//                }

//                var path = new System.IO.DirectoryInfo(Application.dataPath + "/../Bin/android/").FullName;
//                }
//                //obb重新命名
//                if (PlayerSettings.Android.useAPKExpansionFiles)
//                {
//                    string obbFilePath = exportFilePath.Replace("apk", "main.obb");
//                    string newObbFilePath = obbFilePath.Replace("obb", string.Format("{0}.{1}.obb", PlayerSettings.Android.bundleVersionCode, PlayerSettings.GetApplicationIdentifier(targetGroup)));
//                    try
//                    {
//                        if (File.Exists(obbFilePath))
//                        {
//                            File.Move(obbFilePath, newObbFilePath);
//                        }
//                    }
//                    catch (Exception e)
//                    {
//                        Debug.Log(e.ToString());
//                    }
//                }
//            }
            
//        }

//        private void ResetChannelConfig()
//        {
//            if(_config == null)
//            {
//                string channelName = EditorPrefs.GetString("ChannelCfg", "");
//                if (!string.IsNullOrEmpty(channelName))
//                {
//                    _config = ChannelBuilder.LoadChannelCfg(channelName);
//                }
//            }
//        }

//        private void SetConfig_File(ChannelConfig config, BuildTarget target, BuildCfg cfg)
//        {

            
//        }
        
//        private void SetConfig_Compile(ChannelConfig config, BuildTarget target, BuildCfg cfg)
//        {
//            Debug.Log("SetConfig_Compile");
//            if (!string.IsNullOrEmpty(cfg.frontSrvUrl))
//            {
//                config.frontSrvUrl = cfg.frontSrvUrl;
//            }
//            Tools.WriteChannelConst(config);
//            Tools.AddScriptingDefineSymbols(config.DefineSymbol);
//        }

//        private void SetConfig_App(ChannelConfig config,BuildTarget target,BuildCfg cfg)
//        {
//            Debug.Log("SetConfig_App");
//            _config = config;
//            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
//            Debug.Log("applicationIdentifier" + config.applicationIdentifier);
//            PlayerSettings.applicationIdentifier = config.applicationIdentifier;
//            PlayerSettings.productName = config.productName;
//            if(targetGroup == BuildTargetGroup.iOS)
//            {
//                if (!string.IsNullOrEmpty(config.iosApplicationIdentifier) && config.iosApplicationIdentifier != "_")
//                {
//                    PlayerSettings.SetApplicationIdentifier(targetGroup, config.iosApplicationIdentifier);
//                }
//                PlayerSettings.iOS.appleDeveloperTeamID = config.appleDeveloperTeamID;
//            }
//            else if(target == BuildTarget.Android)
//            {
//                PlayerSettings.Android.useAPKExpansionFiles = cfg.obb;
//                Debug.Log("useAPKExpansionFiles" + cfg.obb);
//                PlayerSettings.Android.keystoreName = new FileInfo(Path.Combine( ChannelBuilder.keystoreFolder , config.keyStorePath)).FullName.Replace("\\", "/");
//                string[] keystorePass = Tools.LoadKeyStorePass(config.keyStorePath);
//                Debug.Log("密钥信息");
//                foreach (var item in keystorePass)
//                {
//                    Debug.Log("密钥信息" + item);
//                }
//                PlayerSettings.Android.keystorePass = keystorePass[0];
//                PlayerSettings.Android.keyaliasName = keystorePass[1];
//                PlayerSettings.Android.keyaliasPass = keystorePass[2];
//            }
//            Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AppIcon/menheraicon_nil.png");
            
//            if (t)
//            {
//                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { t }, IconKind.Any);
//                PlayerSettings.SetIconsForTargetGroup(targetGroup, new Texture2D[] { t }, IconKind.Any);
//            }
//        }

//        private void PreparePlugin(ChannelConfig config,BuildTarget target,BuildCfg cfg)
//        {
//            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
//            _pluginImporterGroup = new PluginGroup();
//            string[] plugins = config.plugins.Split(';');
            
//            for (int i = 0; i < plugins.Length; i++)
//            {
//                if (!string.IsNullOrEmpty(plugins[i]))
//                {
//                    Debug.Log("usePlugin" + plugins[i]);
//                    _pluginImporterGroup.Use(plugins[i]);
//                }
//            }
//            _pluginImporterGroup.Prepare(config,target,cfg);
//        }

//        private void RemovePlugin(ChannelConfig config, BuildTarget target, BuildCfg cfg)
//        {
//            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
//            _pluginImporterGroup = new PluginGroup();
//            string[] plugins = config.plugins.Split(';');

//            for (int i = 0; i < plugins.Length; i++)
//            {
//                if (!string.IsNullOrEmpty(plugins[i]))
//                {
//                    Debug.Log("usePlugin" + plugins[i]);
//                    _pluginImporterGroup.Use(plugins[i]);
//                }
//            }
//            _pluginImporterGroup.RemoveAsset();
//        }
        

//        private static void RevertConfig()
//        {
//            if(_config != null)
//            {
//                Tools.RemoveScritpingDefineSymbols(_config.DefineSymbol);
//            }
//            else
//            {
//                Debug.Log("_config null");
//            }
//        }

//        public void OnEndBuild(BuildTarget target, string pathToBuiltProject)
//        {
//            //ResetChannelConfig();
//            //RevertConfig();
//            if (_pluginImporterGroup != null)
//            {
//                try
//                {
                    
//                    //_pluginImporterGroup.Clean(pathToBuiltProject);

//                }
//                catch (Exception e)
//                {
//                    Debug.LogError(e.ToString());
//                }
//                finally
//                {
                    
//                }
//            }
//        }

        
//        public void OnFailed()
//        {
//            Debug.Log("onFailed");
//            RevertConfig();
//            if(_pluginImporterGroup != null)
//            {
//                _pluginImporterGroup.RemoveAsset();
//            }
//        }

//        public void Simulate(ChannelConfig config, BuildTarget target, BuildCfg cfg)
//        {
//            SetConfig_Compile(config, target,cfg);
//            SetConfig_File(config,target,cfg);
//            PreparePlugin(config, target, cfg);
//            AssetDatabase.Refresh();
//            AssetDatabase.SaveAssets();
//        }

        

//        public void RevertSimulate( ChannelConfig config, BuildTarget target, BuildCfg cfg)
//        {
//            _config = config;
//            RevertConfig();
//            RemovePlugin(config,target,cfg);
//        }

//        private static void CopyRes(ChannelConfig config,BuildTarget target,BuildCfg cfg)
//        {
//#if BUILD_TURBO
//            if (Directory.Exists(Application.streamingAssetsPath))
//            {
//                FileOpHelper.DeleteFilesAndFolders(Application.streamingAssetsPath, "WelcomeAV", "pcfg", "resversion");
//            }

//            if (!Directory.Exists(Application.streamingAssetsPath))
//                Directory.CreateDirectory(Application.streamingAssetsPath);

//            if (cfg.obb && target == BuildTarget.Android)
//            {
//                FileOpHelper.CopyDirectoryAllChildren(ResLoaderCore.GetEditorStreamingAssetsPath(), VersionManager.INITIAL_PATH,
//                    is_cover: true);
//            }
//#endif
//        }

//        public void Prepare(ChannelConfig config, BuildTarget buildTarget, BuildCfg cfg)
//        {
//            Debug.Log("准备打包" + config.name);
//            CopyRes(config,buildTarget,cfg);
//            SetConfig_File(config,buildTarget,cfg);
//            PreparePlugin(config, buildTarget, cfg);
//            SetConfig_Compile(config, buildTarget, cfg);
//        }

//    }
//}