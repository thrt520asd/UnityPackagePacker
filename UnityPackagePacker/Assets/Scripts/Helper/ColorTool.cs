using UnityEngine;

namespace Assets.Tools.Script.Helper
{
    public static class ColorTool
    {
        /// <summary>
        /// 金色
        /// </summary>
        public static Color Golden { get; private set; }
        /// <summary>
        /// 金色"f4f07f"
        /// </summary>
        public static string GoldenStr { get; private set; }

        static ColorTool()
        {
            GoldenStr = "f4f07f";
            Golden = GetColorFromRGBHexadecimal(GoldenStr);

        }

        /// <summary>
        /// 获得带颜色标签的BBCode。e.g. Hello world -> [f7f7f7]Hello world[-]
        /// </summary>
        /// <param name="src"></param>
        /// <param name="color">e.g.f7f7f7</param>
        /// <returns></returns>
        public static string SetBBCodeColor(string src,string color)
        {
            return string.Format("[{0}]{1}[-]", color, src);
        }
        /// <summary>
        /// 获得带颜色标签的BBCode。e.g. Hello world -> [f7f7f7]Hello world[-]
        /// </summary>
        /// <param name="src"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string SetBBCodeColor(string src, Color color)
        {
            return SetBBCodeColor(src, color.GetRGBHexadecimal());
        }
        /// <summary>
        /// 格式化Color到16进制字符串
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string GetRGBHexadecimal(this Color color)
        {
            return string.Format("{0}{1}{2}", ((int)(color.r * 255)).ToString("X2"), ((int)(color.g * 255)).ToString("X2"), ((int)(color.b * 255)).ToString("X2"));
        }

        /// <summary>
        /// Gets the RGB hexadecimal.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="a">a.</param>
        /// <returns>System.String.</returns>
        public static string GetRGBHexadecimal(float r,float g,float b,float a)
        {
            return string.Format("{0}{1}{2}{3}", ((int)(r * 255)).ToString("X2"), ((int)(g * 255)).ToString("X2"), ((int)(b * 255)).ToString("X2"), ((int)(a * 255)).ToString("X2"));
        }
        /// <summary>
        /// 格式化16进制字符串到Color
        /// </summary>
        /// <param name="colorStr"></param>
        /// <returns></returns>
        public static Color GetColorFromRGBHexadecimal(string colorStr)
        {
            string r = colorStr.Substring(0, 2);
            string g = colorStr.Substring(2, 2);
            string b = colorStr.Substring(4, 2);
            string a = null;
            if (colorStr.Length == 8)
            {
                a = colorStr.Substring(6, 2);
            }
            uint nr = uint.Parse(r, System.Globalization.NumberStyles.AllowHexSpecifier);
            uint ng = uint.Parse(g, System.Globalization.NumberStyles.AllowHexSpecifier);
            uint nb = uint.Parse(b, System.Globalization.NumberStyles.AllowHexSpecifier);
            uint na = uint.Parse(a ?? "FF", System.Globalization.NumberStyles.AllowHexSpecifier);
            
            Color color=new Color(nr / 255f, ng / 255f, nb / 255f, na / 255f);
            return color;
        }
    }
}