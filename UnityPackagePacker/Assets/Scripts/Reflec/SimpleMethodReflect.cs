using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Assets.Tools.Script.Reflec
{
    /// <summary>
    /// 反射简单的方法
    /// </summary>
    public class SimpleMethodReflect
    {
        /// <summary>
        /// 方法是简单方法
        /// </summary>
        /// <param name="info"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsSimpleMethod(MethodInfo info, object obj)
        {
            ParameterInfo[] parameterInfos = info.GetParameters();
            foreach (var parameterInfo in parameterInfos)
            {
                Type parameterType = parameterInfo.ParameterType;
                if (!parameterType.IsValueType && parameterType != typeof(string) && parameterType != typeof(String) &&
                    !parameterType.IsEnum)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 将字符串格式化成指定的数据类型
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Format(string str, Type type)
        {
            if (String.IsNullOrEmpty(str))
                return null;
            if (type == null)
                return str;
            if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                String[] strs = str.Split(new char[] { ';' });
                Array array = Array.CreateInstance(elementType, strs.Length);
                for (int i = 0, c = strs.Length; i < c; ++i)
                {
                    array.SetValue(ConvertSimpleType(strs[i], elementType), i);
                }
                return array;
            }
            if (type == typeof(Type))
            {
                return Type.GetType(str);
            }
            return ConvertSimpleType(str, type);
        }

        private static object ConvertSimpleType(object value, Type destinationType)
        {
            object returnValue;
            if ((value == null) || destinationType.IsInstanceOfType(value))
            {
                return value;
            }
            string str = value as string;
            if ((str != null) && (str.Length == 0))
            {
                return null;
            }
            TypeConverter converter = TypeDescriptor.GetConverter(destinationType);
            bool flag = converter.CanConvertFrom(value.GetType());
            if (!flag)
            {
                converter = TypeDescriptor.GetConverter(value.GetType());
            }
            if (!flag && !converter.CanConvertTo(destinationType))
            {
                throw new InvalidOperationException("无法转换成类型：" + value + "==>" + destinationType);
            }
            try
            {
                returnValue = flag ? converter.ConvertFrom(null, null, value) : converter.ConvertTo(null, null, value, destinationType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("类型转换出错：" + value + "==>" + destinationType, e);
            }
            return returnValue;
        }

        //--------------------------------------------------------------------------------------------------------------//
        /// <summary>
        /// 方法
        /// </summary>
        public MethodInfo method { get; private set; }
        /// <summary>
        /// 调用方法的对象
        /// </summary>
        public object methodObject { get; private set; }
        /// <summary>
        /// 参数
        /// </summary>
        public List<string> parameters = new List<string>();
        /// <summary>
        /// 设置调用参数
        /// </summary>
        /// <param name="str">参数（会被转换为对应类型）</param>
        /// <param name="parameterIndex">参数位序，0开始</param>
        public void SetParameter(string str, int parameterIndex)
        {
            parameters[parameterIndex] = str;
        }
        /// <summary>
        /// 简单的方法调用反射
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="methodObject">调用方法的对象</param>
        public SimpleMethodReflect(MethodInfo method, object methodObject)
        {
            this.methodObject = methodObject;
            this.method = method;
            ParameterInfo[] parameterInfos = this.method.GetParameters();
            foreach (var genericArgument in method.GetGenericArguments())
            {
                parameters.Add("template " + genericArgument.Name);
            }
            foreach (var parameterInfo in parameterInfos)
            {
                parameters.Add(parameterInfo.ParameterType.Name + " " + parameterInfo.Name);
            }
        }
        /// <summary>
        /// 执行调用
        /// </summary>
        /// <returns></returns>
        public object Call()
        {
            try
            {
                ParameterInfo[] parameterInfos = method.GetParameters();
                Type[] genericArguments = method.GetGenericArguments();

                MethodInfo methodInfo = null;
                if (method.IsGenericMethod)
                {
                    Type[] types = new Type[genericArguments.Length];
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        types[i] = Format(parameters[i], typeof(Type)) as Type;
                    }
                    methodInfo = method.MakeGenericMethod(types);
                }
                else
                {
                    methodInfo = method;
                }

                object[] objs = new object[parameterInfos.Length]; //参数
                for (int i = 0; i < parameterInfos.Length; i++)
                {
                    ParameterInfo parameterInfo = parameterInfos[i];
                    try
                    {
                        objs[i] = Format(parameters[i + genericArguments.Length], parameterInfo.ParameterType);
                    }
                    catch (Exception)
                    {
                        objs[i] = parameterInfo.DefaultValue;
                    }

                }

                object invoke = methodInfo.Invoke(methodObject, objs);
                return invoke;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}