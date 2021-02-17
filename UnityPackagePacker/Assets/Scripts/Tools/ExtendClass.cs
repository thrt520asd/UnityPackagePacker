using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtendClass  {

	public static bool IsNullOrEmpty(this string str)
    {
        return str.IsNullOrEmpty();
    }

    public static bool IsNOTNullOrEmpty(this string str)
    {
        return !str.IsNullOrEmpty();
    }
}
