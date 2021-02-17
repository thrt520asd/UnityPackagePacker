using UnityEngine;

/// <summary>
/// Extension methods for common functions
/// </summary>
public static class UnityExtensions
{
    #region ToV2String

    /// <summary>
    /// Converts a Vector3 to a string in X, Y, Z format
    /// </summary>
    /// <param name="v3"></param>
    /// <returns></returns>
    public static string ToV2String(this Vector2 v2)
    {
        return string.Format("{0}, {1}", v2.x, v2.y);
    }

    // ToV3String

    #endregion

    #region ToV3String

    /// <summary>
    /// Converts a Vector3 to a string in X, Y, Z format
    /// </summary>
    /// <param name="v3"></param>
    /// <returns></returns>
    public static string ToV3String(this Vector3 v3)
    {
        return string.Format("{0}, {1}, {2}", v3.x, v3.y, v3.z);
    }

    // ToV3String

    #endregion
}
