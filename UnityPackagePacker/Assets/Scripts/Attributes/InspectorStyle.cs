namespace Assets.Tools.Script.Attributes
{
    #region

    using System;

    #endregion

    /// <summary>
    /// The inspector style.
    /// </summary>
    public class InspectorStyle : Attribute
    {
        #region Fields

        /// <summary>
        /// 显示的字段名字
        /// </summary>
        public string Name;

        /// <summary>
        /// 如果有对应的解析器，则会使用该解析器解析，否则按照类型，使用默认
        /// </summary>
        public string ParserName;

        /// <summary>
        /// 解析用参数
        /// </summary>
        public object[] ParserAgrs;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InspectorStyle"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public InspectorStyle(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InspectorStyle"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="parserName">
        /// The parser name.
        /// </param>
        public InspectorStyle(string name, string parserName)
        {
            this.Name = name;
            this.ParserName = parserName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InspectorStyle"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="parserName">
        /// The parser name.
        /// </param>
        /// <param name="parserAgrs">
        /// The parser agrs.
        /// </param>
        public InspectorStyle(string name, string parserName, params object[] parserAgrs)
        {
            this.ParserAgrs = parserAgrs;
            this.Name = name;
            this.ParserName = parserName;
        }

        #endregion
    }
}