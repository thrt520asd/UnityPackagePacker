using System.IO;

namespace Assets.Scripts.Platform.Editor.Plugins
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;

    public abstract class PluginImporter
    {

        /// <summary>
        /// 编译目标
        /// </summary>
        protected BuildTarget Target;

        /// <summary>
        /// 编译目标组
        /// </summary>
        protected BuildTargetGroup TargetGroup;


        /// <summary>
        /// 结束后的处理，某些公共接口处理通用的行为
        /// </summary>
        protected Action<string> BuildOver;

        /// <summary>
        /// 这个插件导入对其他插件的依赖
        /// </summary>
        public List<Type> Dependencies = new List<Type>();

        public void PreBuild(BuildTarget target)
        {
            this.Target = target;

            this.OnPreBuild();
        }

        public void EndBuild(string pathToBuiltProject)
        {
            this.OnEndBuild(pathToBuiltProject);
            if (null != BuildOver) BuildOver(pathToBuiltProject);
        }

        protected abstract void OnPreBuild();

        protected abstract void OnEndBuild(string pathToBuiltProject);


    }
}
