using System.Reflection;
/// <summary>
/// 渠道ID规则如下
/// 分发商ID：(渠道ID % 100)
/// 运营商ID：(渠道ID - 分发商ID)
/// </summary>
public class PlatformType
{
    /// <summary>
    /// 内部使用
    /// 999999999
    /// </summary>
    public const int Internal = OperatorEnum.Internal + 9999999;
        

    /// <summary>
    /// 编辑器下的默认值
    /// 990000000
    /// </summary>
    public const int UnityEditor = OperatorEnum.Internal + 0;


    public static string GetName(int platform)
    {
        var type = typeof(PlatformType);
        var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var fieldInfo in fieldInfos)
        {
            if ((int)fieldInfo.GetValue(null) == platform)
            {
                return fieldInfo.Name;
            }
        }
        return "unknow";
    }

    /// <summary>
    /// 获取运营商ID
    /// </summary>
    /// <param name="platform">The platform.</param>
    /// <returns>System.Int32.</returns>
    public static int GetOperator(int platform)
    {
        return platform - (platform % 1000000);
    }

    /// <summary>
    /// 获取分发商ID
    /// </summary>
    /// <param name="platform">The platform.</param>
    /// <returns>System.Int32.</returns>
    public static int GetDistributor(int platform)
    {
        return platform % 1000000;
    }
}

