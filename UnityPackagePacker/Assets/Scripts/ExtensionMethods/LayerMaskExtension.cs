using System.Collections.Generic;
using UnityEngine;


public static class LayerMaskExtension
{
    public static Dictionary<uint, int> ValueToLayer = new Dictionary<uint, int>()
                                                          {
                                                                {1u,0 },
                                                                {2,1 },
                                                                {4,2 },
                                                                {8,3 },
                                                                {16,4 },
                                                                {32,5 },
                                                                {64,6 },
                                                                {128,7 },
                                                                {256,8 },
                                                                {512,9 },
                                                                {1024,10 },
                                                                {2048,11 },
                                                                {4096,12 },
                                                                {8192,13 },
                                                                {16384,14 },
                                                                {32768,15 },
                                                                {65536,16 },
                                                                {131072,17 },
                                                                {262144,18 },
                                                                {524288,19 },
                                                                {1048576,20 },
                                                                {2097152,21 },
                                                                {4194304,22 },
                                                                {8388608,23},
                                                                {16777216,24 },
                                                                {33554432,25 },
                                                                {67108864,26 },
                                                                {134217728,27 },
                                                                {268435456,28 },
                                                                {536870912,29 },
                                                                {1073741824,30 },
                                                                {2147483648,31 }
                                                          };

    public static LayerMask Create(params string[] layerNames)
    {
        return NamesToMask(layerNames);
    }

    public static LayerMask Create(params int[] layerNumbers)
    {
        return LayerNumbersToMask(layerNumbers);
    }

    public static LayerMask NamesToMask(params string[] layerNames)
    {
        LayerMask ret = (LayerMask)0;
        foreach (var name in layerNames)
        {
            ret |= (1 << LayerMask.NameToLayer(name));
        }
        return ret;
    }

    public static LayerMask LayerNumbersToMask(params int[] layerNumbers)
    {
        LayerMask ret = (LayerMask)0;
        foreach (var layer in layerNumbers)
        {
            ret |= (1 << layer);
        }
        return ret;
    }

    public static LayerMask Inverse(this LayerMask original)
    {
        return ~original;
    }

    public static LayerMask AddToMask(this LayerMask original, params string[] layerNames)
    {
        return original | NamesToMask(layerNames);
    }

    public static LayerMask RemoveFromMask(this LayerMask original, params string[] layerNames)
    {
        LayerMask invertedOriginal = ~original;
        return ~(invertedOriginal | NamesToMask(layerNames));
    }

    public static LayerMask RemoveFromMask(this LayerMask original, int layer)
    {
        LayerMask invertedOriginal = ~original;
        return ~(invertedOriginal | layer);
    }

    public static LayerMask RemoveFromMask(int original, int layer)
    {
        LayerMask invertedOriginal = ~original;
        return ~(invertedOriginal | layer);
    }

    public static string[] MaskToNames(this LayerMask original)
    {
        var output = new List<string>();

        for (int i = 0; i < 32; ++i)
        {
            int shifted = 1 << i;
            if ((original & shifted) == shifted)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    output.Add(layerName);
                }
            }
        }
        return output.ToArray();
    }

    public static string MaskToString(this LayerMask original)
    {
        return MaskToString(original, ", ");
    }

    public static string MaskToString(this LayerMask original, string delimiter)
    {
        return string.Join(delimiter, MaskToNames(original));
    }

    public static bool IsInLayerMask(this GameObject obj, LayerMask mask)
    {
        return ((mask.value & (1 << obj.layer)) > 0);
    }
}
