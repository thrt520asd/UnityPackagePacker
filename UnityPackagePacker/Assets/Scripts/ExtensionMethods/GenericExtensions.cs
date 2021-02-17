using System.Collections.Generic;
/* *****************************************************************************
     * File:    GenericExtensions.cs
     * Author:  Philip Pierce - Thursday, September 18, 2014
     * Description:
     *  Generic (T) Extensions
     *  
     * History:
     *  Thursday, September 18, 2014 - Created
     * ****************************************************************************/

/// <summary>
/// Generic (T) Extensions
/// </summary>
public static class GenericExtensions
{
    #region AsList

    /// <summary>
    /// Wraps the given object into a List{T} and returns the list.
    /// </summary>
    /// <param name="tobject">The object to be wrapped.</param>
    /// <typeparam name="T">Refers the object to be returned as List{T}.</typeparam>
    /// <returns>Returns List{T}.</returns>
    public static List<T> AsList<T>(this T tobject)
    {
        return new List<T> {tobject};
    }

    // AsList

    #endregion

    #region IsTNull

    /// <summary>
    /// Returns true if the generic T is null or default. 
    /// This will match: null for classes; null (empty) for Nullable&lt;T&gt;; zero/false/etc for other structs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tObj"></param>
    /// <returns></returns>
    public static bool IsTNull<T>(this T tObj)
    {
        return EqualityComparer<T>.Default.Equals(tObj, default(T));
    }

    // IsTNull

    #endregion
}
