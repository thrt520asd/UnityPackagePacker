using System;
using System.Collections.Generic;

namespace Assets.Tools.Script.Reflec
{
    /// <summary>
    /// 类加载工具
    /// </summary>
    public static class AssemblyTool
    {
        /// <summary>
        /// 循环一遍CurrentDomain里的所有类
        /// </summary>
        /// <param name="execute"></param>
        public static void ForeachCurrentDomainType(Action<Type> execute)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    execute(type);
                }
            }
        }
        /// <summary>
        /// CurrentDomain里寻找满足指定条件的类
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static List<Type> FindTypesInCurrentDomainWhere(Func<Type, bool> condition)
        {
            List<Type> list = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (condition(type))
                    {
                        list.Add(type);
                    }
                }
            }
            return list;
        }

        public static Type FindTypesInCurrentDomainByName(string ClassName)
        {
            List<Type> extendType = AssemblyTool.FindTypesInCurrentDomainWhere(
                (e) =>
                {
                    return e.Name == ClassName;
                });
            if (extendType.Count > 0) return extendType[0];
            else return null;
        }
        /// <summary>
        /// CurrentDomain里寻找带有指定Attribute的类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> FindTypesInCurrentDomainWhereAttributeIs<T>()
        {
            Type withType = typeof(T);
            List<Type> list = FindTypesInCurrentDomainWhere((type) =>
            {
                return type.GetCustomAttributes(withType, true).Length > 0;
            });
            return list;
        }
        /// <summary>
        /// CurrentDomain里寻找继承与指定类的类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> FindTypesInCurrentDomainWhereExtend<T>()
        {
            Type withType = typeof(T);
            List<Type> list = FindTypesInCurrentDomainWhere((type) =>
            {
                return typeof(T).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract;
            });
            return list;
        }

        /// <summary>
        /// CurrentDomain里寻找继承与指定类的类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> FindTypesInCurrentDomainWhereExtend(Type withType)
        {
            List<Type> list = FindTypesInCurrentDomainWhere((type) =>
            {
                return withType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract;
            });
            return list;
        }
    }
}