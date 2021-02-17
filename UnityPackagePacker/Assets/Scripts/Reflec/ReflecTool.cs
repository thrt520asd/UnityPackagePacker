using System;
using System.Reflection;

namespace Assets.Tools.Script.Reflec
{
    using System.Collections.Generic;
    using System.Linq;

    using Assets.Tools.Script.Reflec;
    using Assets.Tools.Script.Attributes;

    using UnityEngine;

    public static class ReflecTool
    {
        public static List<FieldInfo> GetPublicFields(this Type type, Type withAttribute = null)
        {
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            if (withAttribute != null)
            {
                List<FieldInfo> fields = new List<FieldInfo>();
                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.HasAttribute(withAttribute))
                    {
                        fields.Add(fieldInfo);
                    }
                }
                return fields;
            }
            else
            {
                return fieldInfos.ToList();
            }
        }

        public static List<FieldInfo> GetPrivateFields(this Type type, Type withAttribute = null)
        {
            List<Type> baseClassTypes = GetBaseClassTypes(type,true);
            List<FieldInfo> fields = new List<FieldInfo>();

            foreach (var baseClassType in baseClassTypes)
            {
                FieldInfo[] fieldInfos = baseClassType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                if (withAttribute != null)
                {
                    foreach (var fieldInfo in fieldInfos)
                    {
                        if (fieldInfo.HasAttribute(withAttribute))
                        {
                            fields.Add(fieldInfo);
                        }
                    }
                }
                else
                {
                    fields.AddRange(fieldInfos.ToList());
                }
            }
            return fields;
        }

        public static List<Type> GetBaseClassTypes(this Type type, bool includeSelf = false)
        {
            List<Type> types = new List<Type>();
            if (includeSelf)
            {
                types.Add(type);
            }
            type = type.BaseType;
            while (type != typeof(object))
            {
                types.Add(type);
                type = type.BaseType;
            }
            return types;
        }

        public static bool HasAttribute<T>(this FieldInfo field) where T : class
        {
            object[] customAttributes = field.GetCustomAttributes(typeof(T), true);
            return customAttributes.Length > 0;
        }

        public static T GetAttribute<T>(this FieldInfo field) where T : class
        {
            object[] customAttributes = field.GetCustomAttributes(typeof(T), true);
            return customAttributes.Length > 0 ? customAttributes[0] as T : null;
        }

        public static bool HasAttribute<T>(this Type type) where T : class
        {
            object[] customAttributes = type.GetCustomAttributes(typeof(T), true);
            return customAttributes.Length > 0;
        }

        public static T GetAttribute<T>(this MemberInfo field) where T : class
        {
            object[] customAttributes = field.GetCustomAttributes(typeof(T), true);
            return customAttributes.Length > 0 ? customAttributes[0] as T : null;
        }

        public static T GetAttribute<T>(this Type type) where T : class
        {
            object[] customAttributes = type.GetCustomAttributes(typeof(T), true);
            return customAttributes.Length > 0 ? customAttributes[0] as T : null;
        }

        public static bool HasAttribute(this FieldInfo field, Type attribute)
        {
            object[] customAttributes = field.GetCustomAttributes(attribute, true);
            return customAttributes.Length > 0;
        }

        public static Attribute GetAttribute(this FieldInfo field, Type attribute)
        {
            object[] customAttributes = field.GetCustomAttributes(attribute, true);
            return customAttributes.Length > 0 ? customAttributes[0] as Attribute : null;
        }

        public static bool HasAttribute(this Type type, Type attribute)
        {
            object[] customAttributes = type.GetCustomAttributes(attribute, true);
            return customAttributes.Length > 0;
        }

        public static Attribute GetAttribute(this Type type, Type attribute)
        {
            object[] customAttributes = type.GetCustomAttributes(attribute, true);
            return customAttributes.Length > 0 ? customAttributes[0] as Attribute : null;
        }

        public static T Instantiate<T>() where T : class
        {
            Type type = typeof(T);
            ConstructorInfo constructorInfo = type.GetConstructor(Type.EmptyTypes);
            return constructorInfo.Invoke(null) as T;
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
            catch (Exception e )
            {
                Debug.Log(e.Message);
                Debug.Log(type);
            }
            return null;
        }

        public static object Instantiate(Type type,Type[] parameterTypes, object[] parameters)
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

        public static string GetBestName(this FieldInfo field)
        {
            InspectorStyle inspectorStyle = field.GetAttribute<InspectorStyle>();
            if (inspectorStyle != null)
            {
                return inspectorStyle.Name;
            }
            return field.Name;
        }

        public static string GetBestName(this Type type)
        {
            InspectorStyle inspectorStyle = type.GetAttribute<InspectorStyle>();
            if (inspectorStyle != null)
            {
                return inspectorStyle.Name;
            }
            return type.Name;
        }

        public static MemberInfo GetFieldInfoOrPropertyInfo(object targetObj, string field)
        {
            Type targetType = targetObj.GetType();
            FieldInfo fieldInfo = targetType.GetField(field);
            if (fieldInfo != null)
            {
                return fieldInfo;
            }
            PropertyInfo propertyInfo = targetType.GetProperty(field);
            if (propertyInfo != null)
            {
                return propertyInfo;
            }
            propertyInfo = targetType.GetProperty(field, BindingFlags.Instance | BindingFlags.NonPublic);
            if (propertyInfo != null)
            {
                return propertyInfo;
            }
            fieldInfo = targetType.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo != null)
            {
                return fieldInfo;
            }
            throw new Exception("can't find field");
        }

        public static object GetFieldValue(object targetObj, object fieldInfoOrPropertyInfo)
        {
            if (fieldInfoOrPropertyInfo is FieldInfo)
            {
                FieldInfo fieldInfo = fieldInfoOrPropertyInfo as FieldInfo;
                return fieldInfo.GetValue(targetObj);
            }
            else
            {
                PropertyInfo fieldInfo = fieldInfoOrPropertyInfo as PropertyInfo;
                return fieldInfo.GetValue(targetObj, null);
            }
        }

        public static Type GetFieldType(object fieldInfoOrPropertyInfo)
        {
            if (fieldInfoOrPropertyInfo is FieldInfo)
            {
                FieldInfo fieldInfo = fieldInfoOrPropertyInfo as FieldInfo;
                return fieldInfo.FieldType;
            }
            else
            {
                PropertyInfo fieldInfo = fieldInfoOrPropertyInfo as PropertyInfo;
                return fieldInfo.PropertyType;
            }
        }

        public static string GetFieldName(object fieldInfoOrPropertyInfo)
        {
            if (fieldInfoOrPropertyInfo is FieldInfo)
            {
                FieldInfo fieldInfo = fieldInfoOrPropertyInfo as FieldInfo;
                return fieldInfo.Name;
            }
            else
            {
                PropertyInfo fieldInfo = fieldInfoOrPropertyInfo as PropertyInfo;
                return fieldInfo.Name;
            }
        }

        public static void SetFieldValue(object targetObj, object fieldInfoOrPropertyInfo, object toValue)
        {
            if (fieldInfoOrPropertyInfo is FieldInfo)
            {
                FieldInfo fieldInfo = fieldInfoOrPropertyInfo as FieldInfo;
                fieldInfo.SetValue(targetObj, toValue);
            }
            else
            {
                PropertyInfo fieldInfo = fieldInfoOrPropertyInfo as PropertyInfo;
                fieldInfo.SetValue(targetObj, toValue, null);
            }
        }

        /// <summary>
        /// 获得私有属性
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propname"></param>
        /// <returns></returns>
        public static object GetPrivateProperty(object instance, string propname)
        {
            Type type = instance.GetType();
            PropertyInfo property = type.GetProperty(propname);
            return property.GetValue(instance, null);
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="methodName"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static object InvokeMethod(object instance, string methodName, object[] param, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        {
            MethodInfo methdInfo = instance.GetType().GetMethod(methodName, flags);
            if (methdInfo == null)
            {
                return null;
            }
            return methdInfo.Invoke(instance, param);//DropInfo 
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="methodName"></param>
        /// <param name="paramTypes"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static object InvokeMethod(object instance, string methodName,Type[] paramTypes, object[] param)
        {
            MethodInfo methdInfo = instance.GetType().GetMethod(methodName, paramTypes);
            if (methdInfo == null)
            {
                return null;
            }
            return methdInfo.Invoke(instance, param);//DropInfo 
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>System.Object.</returns>
        public static object InvokeMethod(Type type, object instance, string methodName, object[] param, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
        {
            MethodInfo methdInfo = type.GetMethod(methodName, flags);
            if (methdInfo == null)
            {
                return null;
            }
            return methdInfo.Invoke(instance, param);//DropInfo 
        }

        /// <summary>
        /// 获得私有字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="fieldname"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static T GetPrivateField<T>(object instance, string fieldname, BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            Type type = instance.GetType();
            FieldInfo field = type.GetField(fieldname, flags);
            if (field == null) return default(T);
            return (T)field.GetValue(instance);
        }

        /// <summary>
        /// 设置私有字段
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="fieldname"></param>
        /// <param name="value"></param>
        /// <param name="flag"></param>
        public static void SetPrivateFiled(object instance, string fieldname, object value, BindingFlags flag = BindingFlags.Default)
        {
            Type type = instance.GetType();
            FieldInfo field = type.GetField(fieldname, flag);
            field.SetValue(instance, value);
        }

        public static List<MemberInfo> GetAllFieldOrProperty<T>(object instance, BindingFlags flag = BindingFlags.Instance)
        {
            List<MemberInfo> memberInfos = new List<MemberInfo>();
            Type t = instance.GetType();
            PropertyInfo[] propertyInfos = t.GetProperties(flag);
            FieldInfo[] fieldInfos = t.GetFields(flag);
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                object obj = propertyInfo.GetValue(instance, null);
                if (obj is T)
                {
                    memberInfos.Add(propertyInfo);
                }
            }
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                object obj = fieldInfo.GetValue(instance);
                if (obj is T)
                {
                    memberInfos.Add(fieldInfo);
                }
            }
            return memberInfos;
        }
        /// <summary>
        /// 有条件的获得字段或者属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="where"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static List<MemberInfo> GetAllFieldOrPropertyWhere<T>(object instance, Func<MemberInfo, bool> where, BindingFlags flag = BindingFlags.Instance)
        {
            List<MemberInfo> memberInfos = new List<MemberInfo>();
            Type t = instance.GetType();
            PropertyInfo[] propertyInfos = t.GetProperties(flag);
            FieldInfo[] fieldInfos = t.GetFields(flag);
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                object obj = propertyInfo.GetValue(instance, null);
                if ((obj is T) && where(propertyInfo))
                {
                    memberInfos.Add(propertyInfo);
                }
            }
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                object obj = fieldInfo.GetValue(instance);
                if ((obj is T) && where(fieldInfo))
                {
                    memberInfos.Add(fieldInfo);
                }
            }
            return memberInfos;
        }
        /// <summary>
        /// 检查是否有空构造
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool CheckEmptyConstructor(Type t)
        {
            ConstructorInfo[] infos = t.GetConstructors();
            foreach (ConstructorInfo info in infos)
            {
                if (info.GetParameters().Length == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}