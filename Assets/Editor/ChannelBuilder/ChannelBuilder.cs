using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using ChannelBuilder.Builder;
using ChannelBuilder.Importer;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Callbacks;
using System.Text;
using System.Text.RegularExpressions;
using LitJson;

namespace ChannelBuilder
{
    public class Annotation : Attribute {
        public string content;
        public Annotation(string str)
        {
            content = str;
        }
    }

    public class ConstIgnore : Attribute
    {
        
    }
    
    public class BuildCfg {
        public bool debug;
        public bool useIL2CPP;
        public string prefix;
        public bool msdkisofficial;
        public bool importAssets = true;
        public bool obb = false;
        public override string ToString()
        {
            return JsonMapper.ToJson(this);
        }
    }

    [InitializeOnLoad]
    public class ChannelBuilder :EditorWindow
    {
        [MenuItem("构建工具/渠道打包")]
        public static void OpenWin()
        {
            var win = EditorWindow.GetWindow<ChannelBuilder>();
            win.titleContent = new GUIContent("渠道打包");
            win.Show();

        }
        Dictionary<string, string[]> configDic = new Dictionary<string, string[]>();
        

        Dictionary<string, ChannelConfig> channelCfgDic = new Dictionary<string, ChannelConfig>();
        Dictionary<string, int> configIndexDic = new Dictionary<string, int>();
        Dictionary<string, string> annotationDic = new Dictionary<string, string>();
        string[] chPopUpKeys;
        public static string keystoreFolder { get => Application.dataPath + _keyStoreDir; }
        public static string CbCfgDir
        {
            get => Application.dataPath + _cbCfgDir;
        }  
        private static string _channelCfgDir = "Assets/Editor/ChannelBuilder/ChannelConfig/";
        private static string _cbCfgDir = "/../cbCfg";
        private static string _keyStoreDir = _cbCfgDir + "/keyStorePath";
        
        private static IBuilder _builder;
        
        private void OnEnable()
        {
            debug = EditorUserBuildSettings.development;
            Refresh();
        }

        void Refresh()
        {
            InitPlugins();
            ReadChannelCfg();
            ParseTxtCfg();
            ReflectBuilder();
            var lastSelectIndex = EditorPrefs.GetInt(selectCacheKey, 0);
            _selecChIndex = Mathf.Clamp(lastSelectIndex, 0, chPopUpKeys.Length - 1);
        }

        private void ReadChannelCfg()
        {
            string sysfolder = Application.dataPath + "/Editor/ChannelBuilder/ChannelConfig";
            DirectoryInfo dir = new DirectoryInfo(sysfolder);
            var files = dir.GetFiles("*.asset");
            for (int i = 0; i < files.Length; i++)
            {
                string configName = Path.GetFileNameWithoutExtension(files[i].Name);
                var config = AssetDatabase.LoadAssetAtPath<ChannelConfig>(_channelCfgDir + files[i].Name);
                channelCfgDic[configName] = config;
            }
            var cfgKeys = channelCfgDic.Keys.ToArray();
            List<string> list = new List<string>(cfgKeys);
            list.Add("Plugins");
            chPopUpKeys = list.ToArray();
        }
        public static ChannelConfig LoadChannelCfg(string name)
        {
            return AssetDatabase.LoadAssetAtPath<ChannelConfig>(_channelCfgDir + name+".asset");
        }
        

        private void ParseTxtCfg()
        {
            DirectoryInfo configDir = new DirectoryInfo(CbCfgDir);
            if (configDir.Exists)
            {
                var configChildDir = configDir.GetDirectories();
                for (int i = 0; i < configChildDir.Length; i++)
                {
                    var childDir = configChildDir[i];
                    var cfgFile = childDir.FullName + "/cfg" ;
                    string searchParttern = "*.*";
                    if (File.Exists(cfgFile))
                    {
                        var content = File.ReadAllText(cfgFile);
                        var match = Regex.Match(content, @"searchPattern[\s ]*:[\s ]*([.*\w]+)[\s ]*");
                        if (match.Success)
                        {
                            searchParttern = match.Groups[1].Value;
                        }
                    }

                    var fileInfos = childDir.GetFiles(searchParttern);
                    string[] fileNameArray = new string[fileInfos.Length];
                    for (int j = 0; j < fileInfos.Length; j++)
                    {
                        fileNameArray[j] = fileInfos[j].Name;
                    }
                    configDic[childDir.Name] = fileNameArray;
                    
                }
                
            }
            
            string configFilePath = CbCfgDir + "/config.txt";
            if (File.Exists(configFilePath))
            {
                string[] content = File.ReadAllLines(configFilePath);
                List<string> strList = new List<string>();
                
                string cfgName = "";
                for (int i = 0; i < content.Length; i++)
                {
                    string str = content[i];
                    str.TrimEnd();
                    str.TrimStart();
                    
                    if (string.IsNullOrEmpty(str))
                    {
                        continue;
                    }
                    if (str.Contains("---"))
                    {
                        if (!string.IsNullOrEmpty(cfgName))
                        {
                            configDic[cfgName] = strList.ToArray();
                            strList.Clear();
                        }
                        cfgName = str.Replace("-", "");
                        continue;
                    }
                    if (!strList.Contains(str))
                    {
                        strList.Add(str);
                    }
                }
                if (!string.IsNullOrEmpty(cfgName))
                {
                    configDic[cfgName] = strList.ToArray();
                    strList.Clear();
                }
            }
        }

        private void ReflectBuilder()
        {
            configDic["builder"] = Tools.GetAllBuilder();
        }

        private void InitViewIndex(ChannelConfig config)
        {
            Type t = config.GetType();
            foreach (var item in configDic)
            {
                var key = item.Key;
                configIndexDic[key] = 0;
                var field = t.GetField(key);
                if (field == null)
                {
                    Debug.Log("没有指定字段"+key);
                    continue;
                }
                var val = field.GetValue(config).ToString();
                for (int i = 0; i < item.Value.Length; i++)
                {
                    if(item.Value[i] == val)
                    {
                        configIndexDic[key] = i;
                    }
                }
            }
        }

        private int _selecChIndex;
        private bool il2cpp = true;
        private bool debug;
        private bool runtimeConsole = true;
        private string selectCacheKey = "ChannelBuilder_LastSelectIndex";
        private int hotUpdateVer = 0;
        private bool obb;
        private void OnGUI()
        {
            _selecChIndex = EditorGUILayout.Popup(_selecChIndex, chPopUpKeys);
            string name = chPopUpKeys[_selecChIndex];
            if (GUI.changed)
            {
                EditorPrefs.SetInt(selectCacheKey,_selecChIndex);
                var curCfg = channelCfgDic[name];
            }
            
            
            
            if (channelCfgDic.ContainsKey(name))
            {
                DrawChannels(name);
            }
            else
            {
                if(name == "Plugins")
                {
                    DrawPlugins();
                }
            }
            
            
        }

        private void DrawChannels(string name)
        {
            var curConfig = channelCfgDic[name];
            InitViewIndex(curConfig);
            var t = curConfig.GetType();
            var fields = t.GetFields();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            foreach (var field in fields)
            {
                GUILayout.Space(10);
                Annotation a = field.GetCustomAttribute(typeof(Annotation)) as Annotation;
                string annotation = "";
                if (a != null)
                {
                    annotation = "[" + a.content + "]";
                }
                if (field.FieldType == typeof(string))
                {
                    GUILayout.Label(field.Name + annotation);
                }
                else if (field.FieldType == typeof(bool))
                {
                    field.SetValue(curConfig, GUILayout.Toggle((bool)field.GetValue(curConfig), field.Name));
                }
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            foreach (var field in fields)
            {
                GUILayout.Space(10);
                if (configDic.ContainsKey(field.Name))
                {
                    string[] array = configDic[field.Name];
                    configIndexDic[field.Name] = EditorGUILayout.Popup(configIndexDic[field.Name], array);
                    if (GUI.changed)
                    {
                        field.SetValue(curConfig, array[configIndexDic[field.Name]]);
                    }
                    continue;
                }
                if (field.FieldType == typeof(string))
                {
                    field.SetValue(curConfig, GUILayout.TextField(field.GetValue(curConfig).ToString()));
                }
                else
                {
                    GUILayout.Space(10);
                }

            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                il2cpp = GUILayout.Toggle(il2cpp, "Il2Cpp");
                obb = GUILayout.Toggle(obb, "obb");
                runtimeConsole = GUILayout.Toggle(runtimeConsole, "runtimeConsole");
                debug = GUILayout.Toggle(debug, "debug");
                hotUpdateVer = EditorGUILayout.IntField("热更版本",hotUpdateVer);
                if (GUILayout.Button("WriteConst"))
                {
                    Tools.WriteChannelConst(name);
                    AssetDatabase.Refresh();
                    
                }
                if (GUILayout.Button("Save"))
                {
                    EditorUtility.SetDirty(curConfig);
                    AssetDatabase.SaveAssets();
                }
                if (GUILayout.Button("TestKeystore"))
                {
                    PlayerSettings.Android.keystoreName = new FileInfo(ChannelBuilder.keystoreFolder + curConfig.keyStorePath).FullName.Replace("\\", "/");
                    string[] keystorePass = Tools.LoadKeyStorePass(curConfig.keyStorePath);
                    foreach (var item in keystorePass)
                    {
                        Debug.Log("密钥信息" + item);
                    }
                    PlayerSettings.Android.keystorePass = keystorePass[0];
                    PlayerSettings.Android.keyaliasName = keystorePass[1];
                    PlayerSettings.Android.keyaliasPass = keystorePass[2];
                    
                }
                if (GUILayout.Button("Simulate"))
                {
                    LoadBuilder(name, (b, cfg) => b.Simulate(cfg, EditorUserBuildSettings.activeBuildTarget, new BuildCfg() { useIL2CPP = il2cpp, debug = debug}));
                }
                if (GUILayout.Button("RevertSimulate"))
                {
                    LoadBuilder(name, (b, cfg) => b.RevertSimulate(cfg, EditorUserBuildSettings.activeBuildTarget, new BuildCfg() { useIL2CPP = il2cpp, debug = debug}));
                }
                if (GUILayout.Button("刷新配置"))
                {
                    Refresh();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Build"))
            {
                EditorUtility.SetDirty(curConfig);
                AssetDatabase.SaveAssets();
                Run(name, EditorUserBuildSettings.activeBuildTarget, new BuildCfg() { useIL2CPP = il2cpp, debug = debug });
            }
            if (GUILayout.Button("Build(不导入"))
            {
                EditorUtility.SetDirty(curConfig);
                AssetDatabase.SaveAssets();
                Run(name, EditorUserBuildSettings.activeBuildTarget, new BuildCfg() { useIL2CPP = il2cpp, debug = debug});
            }
            GUILayout.EndHorizontal();

        }
        
        private static void LoadBuilder(string channel, Action<IBuilder,ChannelConfig> call)
        {
            var config = LoadChannelCfg(channel);
            if (config != null)
            {
                _builder = Tools.GetBuilder(config.builder);
                try { call(_builder,config); }
                catch (Exception e)
                {
                    Debug.Log("LoadBuilder " + e.ToString());

                }
            }
            else
            {
                Debug.LogError("找不到渠道" + channel);
            }
        }

        public static void Run(string channel,BuildTarget target,BuildCfg cfg)
        {
            var config = LoadChannelCfg(channel);
            Run(config, target, cfg);
        }

        private static void Run(ChannelConfig config,BuildTarget target,BuildCfg cfg)
        {
            Debug.Log("Build channle config::" + config.ToString());
            _builder = Tools.GetBuilder(config.builder);
            
            try {
                _builder.Prepare(config, target, cfg);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["channelCfg"] = JsonMapper.ToJson(config);
                dic["buildTarget"] = ((int)target).ToString();
                dic["buildCfg"] = JsonMapper.ToJson(cfg);
                if (File.Exists(getCacheCfgPath()))
                {
                    Debug.Log("delete cacheBuildCfg");
                    File.Delete(getCacheCfgPath());
                }
                string content = JsonMapper.ToJson(dic);
                Debug.Log("write cacheBuildCfg" + content);
                File.WriteAllText(getCacheCfgPath(), content);
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            }
            catch(Exception e)
            {
                Debug.Log("打包失败结果" + e.ToString());
                _builder.OnFailed();
            }
            
        }

        private static string getCacheCfgPath()
        {
            return Application.dataPath + "/../cbCfg/cacheBuilCfg";
        }
        public static void CacheBuild()
        {
            string path = getCacheCfgPath();
            //Debug.Log("cacheCfgPath" + path);
            if (File.Exists(path))
            {
                string channelCache="";
                int targetCache;
                string cfgCache = "";
                string content = File.ReadAllText(path);
                Debug.Log(content);
                Dictionary<string, string> dic = JsonMapper.ToObject<Dictionary<string,string>>(content);
                channelCache = dic["channelCfg"];
                targetCache = int.Parse(dic["buildTarget"]);
                cfgCache = dic["buildCfg"];
                Debug.Log("cacheChannelCfgString " + channelCache);
                Debug.Log("cacheBuildCfgString " + cfgCache);
                Debug.Log("targetCache " + targetCache);
                File.Delete(getCacheCfgPath());
                if (string.IsNullOrEmpty(channelCache) || string.IsNullOrEmpty(cfgCache) || targetCache == 0)
                {
                    Debug.LogError("数据错误");
                    return;
                }
                ChannelConfig config = JsonMapper.ToObject<ChannelConfig>(channelCache);
                BuildCfg cfg = JsonMapper.ToObject<BuildCfg>(cfgCache);
                BuildTarget target = (BuildTarget)targetCache;
                Debug.Log("cacheChannelCfg" + config.ToString());
                Debug.Log("cacheBuildCfg" + cfg.ToString());
                Debug.Log("cacheTarget" + target.ToString());
                EditorUpdater updator = new EditorUpdater();
                if (config == null)
                {
                    Debug.LogError("序列化错误");
                    return;
                }
                Debug.Log("start Build");
                _builder = Tools.GetBuilder(config.builder);
                _builder.Build(config, target, cfg);
                
            }
        }
        [DidReloadScripts]
        static void AllScriptsReloaded()
        {
            if (Application.isBatchMode)
            {
                return;
            }
            EditorUpdater updator = new EditorUpdater();
            updator.DelayedCall(0.1f, delegate () {
                CacheBuild();
            });
        }

        [PostProcessBuild(100)]
        private static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (_builder != null)
            {
                try
                {
                    _builder.OnEndBuild(target,pathToBuiltProject);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                    _builder.OnFailed();
                }
            }
        }
        private IPlugin[] pluginImporters;
        private void InitPlugins()
        {
            pluginImporters = Tools.GetAllPlugins();
        }

        private void DrawPlugins()
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < 4; i++)
            {
                GUILayout.BeginVertical();
                foreach (var item in pluginImporters)
                {
                    
                    if(i == 0)
                    {
                        GUILayout.Label(item.Name);
                    }else if(i == 1)
                    {
                        if (GUILayout.Button("ImportAssets"))
                        {
                            item.ImportAsset();
                        }
                    }else if (i == 2)
                    {
                        if (GUILayout.Button("RemoveAssets"))
                        {
                            item.RemoveAsset();
                        }
                    }else if (i == 3)
                    {
                        if (GUILayout.Button("CopyToAssets"))
                        {
                            item.CopyToChannelAsset();
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            
            GUILayout.EndHorizontal();
        }

    }


    public class ReflectTool
    {

        public static List<Type> GetInterfaces<T>(string assemblyName)
        {
            List<Type> list = new List<Type>();
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var item in assemblies)
            {
                if (item.FullName == assemblyName)
                {
                    var modules = item.GetModules();
                    var types = item.GetTypes();
                    foreach (var t in types)
                    {
                        if (t.GetInterface(typeof(T).ToString()) != null)
                        {
                            list.Add(t);
                        }
                    }
                }
                
            }
            return list;
        }

        public static T Instantiate<T>(Type[] parameterTypes, object[] parameters) where T : class
        {
            Type type = typeof(T);
            ConstructorInfo constructorInfo = type.GetConstructor(parameterTypes);
            return constructorInfo.Invoke(parameters) as T;
        }

        public static object Instantiate(Type type)
        {
            try
            {
                ConstructorInfo constructorInfo = type.GetConstructor(Type.EmptyTypes);
                return constructorInfo.Invoke(null);
            }
            catch (Exception)
            {
                Debug.Log(type);
            }
            return null;
        }

        public static T Instantiate<T>() where T : class
        {
            Type type = typeof(T);
            ConstructorInfo constructorInfo = type.GetConstructor(Type.EmptyTypes);
            return constructorInfo.Invoke(null) as T;
        }

        public static object Instantiate(Type type, Type[] parameterTypes, object[] parameters)
        {
            ConstructorInfo constructorInfo = type.GetConstructor(parameterTypes);
            return constructorInfo.Invoke(parameters);
        }

        public static List<T> Instantiate<T>(List<Type> types) where T : class
        {
            List<T> list = new List<T>();
            foreach (var type in types)
            {
                list.Add(Instantiate(type) as T);
            }
            return list;
        }
    }

    public class Tools {

        public static string LoadCustomCfg(string name)
        {
            string path = ChannelBuilder.CbCfgDir + "/CustomCfg/" + name;
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            else
            {
                Debug.LogError("file not exit "+path);
                return null;
            }
            
        }
        
        public static string[] LoadKeyStorePass(string keyStoreName)
        {
            if (string.IsNullOrEmpty(keyStoreName))
            {
                Debug.LogError("keyStoreName null ");
                return null;
            }
            Debug.Log(keyStoreName);
            string filePath = Path.Combine( ChannelBuilder.keystoreFolder , "keystorePass.txt");
            var contents = File.ReadAllLines(filePath);
            string[] keyStorePass = new string[3];
            for (int i = 0; i < contents.Length; i++)
            {
                string content = contents[i];
                if (content == keyStoreName)
                {
                    keyStorePass[0] = contents[i + 1].Replace("keystorePass ", "");
                    keyStorePass[1] = contents[i + 2].Replace("keyaliasName ", "");
                    keyStorePass[2] = contents[i + 3].Replace("keyaliasPass ", "");
                }
            }
            return keyStorePass;
        }

        public static void WriteChannelConst(ChannelConfig channelCfg)
        {
            var type = channelCfg.GetType();
            FieldInfo[] fieldInfos = type.GetFields();
            var path = Application.dataPath + @"/ChannelConst.cs";
            StringBuilder sb = new StringBuilder();
            sb.Append(
@"public class ChannelConst
{");
            sb.AppendLine();
            Func<FieldInfo, string> getFieldValDes = (fieldInfo) => {
                var val = fieldInfo.GetValue(channelCfg).ToString();

                if (fieldInfo.FieldType == typeof(string))
                {
                    string[] vals = val.Split('#');
                    val = vals[0];
                    val = "\"" + val + "\"";
                }
                else if (fieldInfo.FieldType == typeof(bool))
                {
                    val = val.ToLower();
                }
                return val;
            };
            foreach (var item in fieldInfos)
            {
                if(((item.GetCustomAttribute(typeof(ConstIgnore)) as ConstIgnore)  != null ))
                {
                    continue;
                }
                sb.AppendLine(string.Format("public const {0} {1} = {2};", item.FieldType.ToString(), item.Name, getFieldValDes(item)));
            }
            sb.AppendLine();
            sb.Append("}");
            sb.AppendLine(@"//Create " + System.DateTime.Now.ToString());
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.WriteAllText(path, sb.ToString());
        }
        public static void WriteChannelConst(string channel)
        {
            WriteChannelConst(ChannelBuilder.LoadChannelCfg(channel));
        }
        public static string[] GetAllBuilder()
        {
            List<Type> builderList = ReflectTool.GetInterfaces<IBuilder>("Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            List<string> list = new List<string>();
            for (int i = 0; i < builderList.Count; i++)
            {
                var builder = ReflectTool.Instantiate(builderList[i]) as IBuilder;
                list.Add(builder.Name);
            }
            return list.ToArray();
        }

        public static IPlugin[] GetAllPlugins()
        {
            List<Type> typeList = ReflectTool.GetInterfaces<IPlugin>("Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            List<IPlugin> list = new List<IPlugin>();
            for (int i = 0; i < typeList.Count; i++)
            {
                var builder = ReflectTool.Instantiate(typeList[i]) as IPlugin;
                list.Add(builder);
            }
            return list.ToArray();
        }
        public static IBuilder GetBuilder(string name)
        {
            List<Type> builderList = ReflectTool.GetInterfaces<IBuilder>("Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            for (int i = 0; i < builderList.Count; i++)
            {
                var builder = ReflectTool.Instantiate(builderList[i]) as IBuilder;
                if (builder.Name == name)
                {
                    return builder;
                }
            }
            Debug.LogError("找不到builder" + name);
            return null;
        }

        public static Type GetPluginImporterType(string name)
        {
            List<Type> builderList = ReflectTool.GetInterfaces<IPlugin>("Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            for (int i = 0; i < builderList.Count; i++)
            {
                var pluginImporter = ReflectTool.Instantiate(builderList[i]) as IPlugin;
                if (pluginImporter.Name == name)
                {
                    return builderList[i];
                }
            }
            Debug.LogError("找不到pluginImporter" + name);
            return null;
        }

        public static IPlugin GetPluginImporter(string name)
        {
            List<Type> builderList = ReflectTool.GetInterfaces<IPlugin>("Assembly-CSharp-Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            for (int i = 0; i < builderList.Count; i++)
            {
                var pluginImporter = ReflectTool.Instantiate(builderList[i]) as IPlugin;
                if (pluginImporter.Name == name)
                {
                    return pluginImporter;
                }
            }
            Debug.LogError("找不到pluginImporter" + name);
            return null;
        }
        public static BuildTargetGroup GetCurBuildTargetGroup()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var group = BuildPipeline.GetBuildTargetGroup(target);
            return group;
        }

        public static void AddScriptingDefineSymbols(string symbols)
        {
            string[] defines = symbols.Split(';');
            foreach (var item in defines)
            {
                Debug.Log(item);
                AddScriptingDefineSymbol(item);
            }
        }

        public static void RemoveScritpingDefineSymbols(string symbols)
        {
            string[] defines = symbols.Split(';');
            foreach (var item in defines)
            {
                RemoveScriptingDefineSymbol(item);
            }
        }

        public static void AddScriptingDefineSymbol(string symbol)
        {
            AddScriptingDefineSymbol(GetCurBuildTargetGroup(), symbol);

        }

        public static void RemoveScriptingDefineSymbol(string symbol)
        {
            RemoveScriptingDefineSymbol(GetCurBuildTargetGroup(), symbol);
        }

        public static void AddScriptingDefineSymbol(BuildTargetGroup buildTargetGroup, string symbol)
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

        

        private static FileSystemInfo[] filterFileSystemInfos(FileSystemInfo[] infos)
        {;
            List<FileSystemInfo> newList = new List<FileSystemInfo>();
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].FullName.EndsWith(""))
                {
                    continue;
                }
                newList.Add((infos[i]));
            }

            return newList.ToArray();
        }
        
        public static void CopyDir(string srcPath, string destPath,bool backup = false)
        {
            try
            {
                if (!Directory.Exists(destPath))
                {
                    Directory.CreateDirectory(destPath);
                }
                DirectoryInfo dir = new DirectoryInfo(srcPath); 
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
                fileinfo = filterFileSystemInfos(fileinfo);
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)     //判断是否文件夹
                    {
                        if (!Directory.Exists(destPath + "/" + i.Name))
                        {
                            Directory.CreateDirectory(destPath + "/" + i.Name);   //目标目录下不存在此文件夹即创建子文件夹
                        }
                        CopyDir(i.FullName, destPath + "/" + i.Name);    //递归调用复制子文件夹
                    }
                    else
                    {
                        var desFilePath = destPath + "/" + i.Name;
                        if (backup)
                        {
                            if (File.Exists((desFilePath)))
                            {
                                File.Move(desFilePath, desFilePath + ".backup");
                            }
                        }
                        
                        File.Copy(i.FullName, destPath + "/" + i.Name, true);      //不是文件夹即复制文件，true表示可以覆盖同名文件
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public static void RemoveDir(string srcPath,string destPath,bool isCopy = false)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
                fileinfo = filterFileSystemInfos(fileinfo);
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)     //判断是否文件夹
                    {
                        
                        string dstDir = destPath + "/" + i.Name;
                        if (Directory.Exists(dstDir))
                        {
                            RemoveDir(i.FullName, dstDir,isCopy);    //递归调用删除子文件夹
                            var dstDirInfo = new DirectoryInfo(dstDir);
                            
                            var files = dstDirInfo.GetFileSystemInfos();
                            bool otherFile = false;
                            for (int j = 0; j < files.Length; j++)
                            {
                                if (!files[j].FullName.EndsWith(".meta"))
                                {
                                    otherFile = true;
                                }
                            }
                            if (!otherFile)
                            {
                                Directory.Delete(dstDir,true);
                            }
                            else
                            {
                                
                            }
                        }
                    }
                    else
                    {
                        string targetPath = destPath + "/" + i.Name;
                        if (File.Exists(targetPath))
                        {
                            if (isCopy)
                            {
                                File.Copy(targetPath, i.FullName, true);
                            }

                            File.Delete(targetPath);
                            if (File.Exists(targetPath + ".backup"))
                            {
                                File.Move(targetPath+".backup",targetPath);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        
    }

}