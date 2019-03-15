using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ReflectTool
{
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
