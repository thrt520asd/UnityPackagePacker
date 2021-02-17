using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;
using System;

namespace ChannelBuilder
{
    [CreateAssetMenu]
    public class ChannelConfig : ScriptableObject
    {
        //包名
        [Annotation("包名")]
        public string applicationIdentifier="";
        [Annotation("iOs包名如果和默认包名一样不填")]
        public string iosApplicationIdentifier="";
        public string gameId;
        //app名称
        [Annotation("产品名")]
        public string productName="";
        //安卓签名文件

        public string keyStorePath="";
        
        //宏定义';'分割
        [Annotation("宏定义';'分割")]
        public string DefineSymbol="";
        //插件 使用;分割
        [Annotation("插件 使用;分割")]
        public string plugins="";
        //build 默认使用default
        [Annotation("打包器默认使用Demo")]
        public string builder = "Demo";
        
        [ConstIgnore]
        public string CustomCfg = "";
        public override string ToString()
        {
            return LitJson.JsonMapper.ToJson(this);
        }
    }
}