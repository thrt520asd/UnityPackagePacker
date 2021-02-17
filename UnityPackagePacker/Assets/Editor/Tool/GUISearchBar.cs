namespace Assets.Tools.Script.Editor.Tool
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;

    using Assets.Tools.Script.Attributes;
    using Assets.Tools.Script.Editor.Inspector.Field;
    using Assets.Tools.Script.Editor.Window;
    //using Assets.Tools.Script.Event;
    using Assets.Tools.Script.Helper;
    using Assets.Tools.Script.Reflec;

    using UnityEditor;

    using UnityEngine;

    public class GUISearchBar<T>
    {
        public GUISearchBar()
        {
            this.Init(null);
        }

        public GUISearchBar(string searchId)
        {
            this.Init(searchId);
        }

        /// <summary>
        /// 有字段排序功能
        /// </summary>
        public bool HasSort;

        /// <summary>
        /// 有筛选功能
        /// </summary>
        public bool HasFilter;

        /// <summary>
        /// 搜索内容
        /// </summary>
        public string SearchContent;

        /// <summary>
        /// 页个数上限
        /// </summary>
        public int PageDataCount = 500;

        /// <summary>
        /// 筛选器
        /// </summary>
        private List<GUISearchBarFilter> filters = new List<GUISearchBarFilter>();

        /// <summary>
        /// 数据等待重新搜索
        /// </summary>
        public  bool IsDirty { get; private set; }

        /// <summary>
        /// 需要重新搜索
        /// </summary>
        private bool research = false;

        /// <summary>
        /// 当前搜索到的数据
        /// </summary>
        private List<T> searchDataList;

        /// <summary>
        /// 最后一次搜索的搜索源数据个数
        /// </summary>
        private int lastSearchSourceCount;

        /// <summary>
        /// 当前页
        /// </summary>
        private int currPage = 0;

        private string searchId;

        private string sortField;

        private Type dataType;

        private void Init(string searchId)
        {
            this.dataType = typeof(T);
            if (searchId.IsNOTNullOrEmpty())
            {
                this.searchId = searchId;
                var fieldName = this.GetSaveState();
                if (fieldName.IsNOTNullOrEmpty())
                {
                    this.sortField = fieldName;
                    var fieldInfo = this.dataType.GetField(this.sortField, BindingFlags.Instance | BindingFlags.Public);
                    if (fieldInfo == null)
                    {
                        this.sortField = string.Empty;
                    }
                }
            }
            var fieldInfos = this.dataType.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.FieldType == typeof(string))
                {
                    filters.Add(new StringFiledFilter() { FieldName = fieldInfo.Name });
                }
                else if (fieldInfo.FieldType == typeof(int))
                {
                    filters.Add(new IntFiledFilter() { FieldName = fieldInfo.Name });
                    filters.Add(new RangeFieldFilter<int>() { FieldName = fieldInfo.Name });
                }
                else if (fieldInfo.FieldType == typeof(bool))
                {
                    filters.Add(new FieldFilter<bool>() { FieldName = fieldInfo.Name });
                }
                else if (fieldInfo.FieldType == typeof(uint))
                {
                    filters.Add(new FieldFilter<uint>() { FieldName = fieldInfo.Name });
                    filters.Add(new RangeFieldFilter<uint>() { FieldName = fieldInfo.Name });
                }
                else if (fieldInfo.FieldType == typeof(float))
                {
                    filters.Add(new FieldFilter<float>() { FieldName = fieldInfo.Name });
                    filters.Add(new RangeFieldFilter<float>() { FieldName = fieldInfo.Name });
                }
                else if (fieldInfo.FieldType == typeof(double))
                {
                    filters.Add(new FieldFilter<double>() { FieldName = fieldInfo.Name });
                    filters.Add(new RangeFieldFilter<double>() { FieldName = fieldInfo.Name });
                }
                else if (fieldInfo.FieldType.IsEnum)
                {
                    filters.Add(new EnumFieldFilter() { FieldName = fieldInfo.Name });
                }
            }
        }

        /// <summary>
        /// 显示数据项搜索条
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="getName">获得数据项名字</param>
        /// <returns>经过整理的数据List</returns>
        public List<T> Draw(List<T> source, Func<T, string> getName)
        {
            if (this.IsDirty)
            {
                this.research = true;
                this.IsDirty = false;
            }
            else
            {
                this.research = false;
            }

            if (this.lastSearchSourceCount != source.Count)
            {
                this.SetDirty();
                this.lastSearchSourceCount = source.Count;
            }

            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            //显示搜索条
            var searchText = EditorGUILayout.TextField(this.SearchContent, (GUIStyle)"ToolbarSeachTextField");
            if (this.SearchContent != searchText)
            {
                this.SetDirty();
            }
            this.SearchContent = searchText;
            if (GUILayout.Button("", (GUIStyle)"ToolbarSeachCancelButton"))
            {
                this.SearchContent = string.Empty;
                GUIUtility.keyboardControl = 0;
            }

            GUILayout.Space(5);

            var list = this.research ? source : this.searchDataList;
            
            //需要搜索则搜索
            if (this.SearchContent.IsNOTNullOrEmpty())
            {
                list = this.Search(list, getName);
            }

            //需要筛选则筛选
            if (this.HasFilter && this.filters.Count > 0)
            {
                list = this.Filter(list);
            }

            //需要排序则排序
            if (this.HasSort)
            {
                list = this.Sort(list);
            }
            
            //翻页
            if (list != null && list.Count > this.PageDataCount)
            {
                if (GUILayout.Button("◀", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    this.currPage--;
                }
                GUILayout.Label(this.currPage.ToString(), EditorStyles.toolbarTextField, GUILayout.Width(20));
                if (GUILayout.Button("▶", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    this.currPage++;
                }
            }

            //数据整理完毕
            this.searchDataList = list;

            GUILayout.EndHorizontal();

            //需要赛选时显示当前赛选内容
            if (this.HasFilter && this.filters.Count > 0)
            {
                this.ShowFilter();
            }

            if (this.searchDataList != null && this.searchDataList.Count > this.PageDataCount)
            {
                var currPageCount = Mathf.CeilToInt((float)this.searchDataList.Count/ this.PageDataCount);
                if (this.currPage > currPageCount - 1)
                {
                    this.currPage = currPageCount - 1;
                }
                if (this.currPage < 0)
                {
                    this.currPage = 0;
                }
                if (this.currPage >= currPageCount - 1)
                {
                    var range = this.searchDataList.GetRange(
                        this.currPage * this.PageDataCount,
                        this.searchDataList.Count % this.PageDataCount);
                    return range;
                }
                else
                {
                    var range = this.searchDataList.GetRange(
                        this.currPage * this.PageDataCount,
                        this.PageDataCount);
                    return range;
                }
            }
            else
            {
                return this.searchDataList;
            }
        }

        /// <summary>
        /// 设置数据为脏数据，等待下一次刷新数据
        /// </summary>
        public void SetDirty()
        {
            this.IsDirty = true;
            this.currPage = 0;
        }

        /// <summary>
        /// 添加一个筛选器
        /// </summary>
        /// <param name="filter">The filter.</param>
        public void AddFilter(GUISearchBarFilter filter)
        {
            this.filters.Add(filter);
        }

        /// <summary>
        /// 筛选列表
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>List&lt;T&gt;.</returns>
        private List<T> Filter(List<T> source)
        {
            if (GUILayout.Button("F", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                PopCustomWindow customWindow = new PopCustomWindow();
                customWindow.DrawGUI = () =>
                    {
                        GUILayout.BeginHorizontal("flow overlay box");
                        GUITool.Button("筛选", Color.clear);
                        GUILayout.EndHorizontal();
                        
                        GUILayout.BeginVertical();
                        foreach (var searchFilter in filters)
                        {
                            var show = searchFilter.Show();
                            if (show)
                            {
                                this.SetDirty();
                            }
                        }
                        GUILayout.EndVertical();
                    };
                customWindow.PopWindow();
            }

            if (research)
            {
                var list = new List<T>();

                //检索数据，必须满足所有的筛选器
                foreach (var data in source)
                {
                    bool enable = this.filters.All(filter => !filter.IsActive() || filter.RunFilter(data));
                    if (enable)
                    {
                        list.Add(data);
                    }
                }
                return list;
            }
            return source;
        }

        /// <summary>
        /// 搜索列表
        /// </summary>
        /// <param name="source">The list.</param>
        /// <param name="getName">Name of the get.</param>
        /// <returns>List&lt;T&gt;.</returns>
        private List<T> Search(List<T> source, Func<T, string> getName)
        {
            if (research)
            {
                List<T> list = new List<T>();
                for (int i = 0; i < source.Count; i++)
                {
                    var data = source[i];
                    var name = getName(data).ToLower();
                    var traditional = SearchTool.ToTraditional(name);
                    var simplified = SearchTool.ToSimplified(name);
                    var keyword = this.SearchContent.ToLower();
                    if (name.Contains(keyword) || simplified.Contains(keyword) || traditional.Contains(keyword))
                    {
                        list.Add(data);
                    }
                }
                return list;
            }
            return source;
        }

        /// <summary>
        /// 排序列表
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>List&lt;T&gt;.</returns>
        private List<T> Sort(List<T> list)
        {
            FieldInfo sortFieldInfo = null;
            if (this.sortField.IsNOTNullOrEmpty())
            {
                sortFieldInfo = this.dataType.GetField(this.sortField);
            }
            
            bool click = false;

            if (sortFieldInfo != null)
            {
                click = GUILayout.Button("Sort:" + this.dataType.GetField(this.sortField).GetBestName(), EditorStyles.toolbarButton);
            }
            else
            {
                click = GUILayout.Button("S", EditorStyles.toolbarButton, GUILayout.Width(20));
            }
            if (click)
            {
                var customWindow = PopCustomWindow.ShowPopWindow();
                customWindow.DrawGUI = () =>
                    {
                        GUILayout.BeginVertical();
                        GUILayout.BeginHorizontal("flow overlay box");
                        GUITool.Button("排序", Color.clear);
                        GUILayout.EndHorizontal();
                        var fieldInfos = this.dataType.GetFields(BindingFlags.Instance | BindingFlags.Public);
                        if (GUILayout.Button("默认"))
                        {
                            this.sortField = string.Empty;
                            this.SaveState();
                            customWindow.CloseWindow();
                            this.SetDirty();
                        }
                        foreach (var fieldInfo in fieldInfos)
                        {
                            if (fieldInfo.FieldType == typeof(int) || fieldInfo.FieldType == typeof(string)
                                || fieldInfo.FieldType == typeof(uint) || fieldInfo.FieldType == typeof(float))
                            {
                                if (GUILayout.Button(fieldInfo.Name))
                                {
                                    this.sortField = fieldInfo.Name;
                                    this.SaveState();
                                    customWindow.CloseWindow();
                                    this.SetDirty();
                                }
                            }
                        }
                        GUILayout.EndVertical();
                    };
            }
            if (research)
            {
                if (this.sortField.IsNOTNullOrEmpty())
                {
                    list.Sort(
                    (l, r) =>
                    {
                        var fieldInfo = this.dataType.GetField(this.sortField, BindingFlags.Instance | BindingFlags.Public);
                        var lvalue = fieldInfo.GetValue(l);
                        var rvalue = fieldInfo.GetValue(r);
                        if (fieldInfo.FieldType == typeof(string))
                        {
                            return StringComparer.CurrentCulture.Compare(lvalue, rvalue);
                        }
                        if (fieldInfo.FieldType == typeof(uint))
                        {
                            if ((uint)(lvalue) > (uint)(rvalue))
                            {
                                return 1;
                            }
                            if ((uint)(lvalue) < (uint)(rvalue))
                            {
                                return -1;
                            }
                            return 0;
                        }
                        if (fieldInfo.FieldType == typeof(int))
                        {
                            if ((int)(lvalue) > (int)(rvalue))
                            {
                                return 1;
                            }
                            if ((int)(lvalue) < (int)(rvalue))
                            {
                                return -1;
                            }
                            return 0;
                        }
                        if (fieldInfo.FieldType == typeof(float))
                        {
                            if ((float)(lvalue) > (float)(rvalue))
                            {
                                return 1;
                            }
                            if ((float)(lvalue) < (float)(rvalue))
                            {
                                return -1;
                            }
                            return 0;
                        }
                        return 0;
                    });
                }
            }

            return list;
        }

        /// <summary>
        /// 显示当前正在使用的筛选项
        /// </summary>
        private void ShowFilter()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Space(2);
            var searchFilters = this.filters.ToArray();
            foreach (var activeFilter in searchFilters)
            {
                if (!activeFilter.IsActive())
                {
                    continue;
                }
                GUILayout.BeginHorizontal(GUILayout.Width(10));
                if (GUILayout.Button(activeFilter.Name, "sv_label_1"))
                {
                    activeFilter.OnDisable();
                    this.SetDirty();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void SaveState()
        {
//            if (this.searchId.IsNOTNullOrEmpty())
//            {
//                EditorPrefs.SetString(string.Format("{0}_search_sort_field", this.searchId), this.sortField);
//            }
        }

        private string GetSaveState()
        {
//            return EditorPrefs.GetString(string.Format("{0}_search_sort_field", this.searchId));
            return null;
        }

        /// <summary>
        /// 筛选器
        /// </summary>
        public abstract class GUISearchBarFilter
        {
            public string Name;

            public abstract bool RunFilter(T item);

            public abstract bool Show();

            public abstract void OnDisable();

            public abstract bool IsActive();
        }

        /// <summary>
        /// 通用筛选器
        /// </summary>
        private class CommonFilter : GUISearchBarFilter
        {
            public string Name;

            public Func<T, bool> Filter;

            public override bool RunFilter(T item)
            {
                return Filter(item);
            }

            public override bool Show()
            {
                return false;
            }

            public override void OnDisable()
            {
                
            }

            public override bool IsActive()
            {
                return true;
            }
        }

        /// <summary>
        /// 字段筛选器
        /// </summary>
        /// <typeparam name="FT">The type of the field.</typeparam>
        private class FieldFilter<FT> : GUISearchBarFilter
        {
            public List<FT> hitValues = new List<FT>();

            public string FieldName = null;

            protected FT addHit;

            public override bool RunFilter(T item)
            {
                if (this.hitValues.Count == 0)
                {
                    return true;
                }
                var fieldInfo = typeof(T).GetField(this.FieldName, BindingFlags.Public | BindingFlags.Instance);
                var s = (FT)fieldInfo.GetValue(item);
                return this.hitValues.Contains(s);
            }

            public override bool Show()
            {
                bool changed = false;
                GUILayout.BeginHorizontal();
                var fieldInfo = typeof(T).GetField(this.FieldName, BindingFlags.Public | BindingFlags.Instance);
                var bestName = fieldInfo.GetBestName();
                GUILayout.Label(bestName, GUILayout.Width(146));

                for (int i = 0; i < this.hitValues.Count; i++)
                {
                    var hitString = this.hitValues[i];
                    if (GUILayout.Button(String.Format("{0} {1}", hitString, "X".SetColor(new Color(0, 0, 0, 0.2f)))))
                    {
                        this.hitValues.RemoveAt(i);
                        i--;
                        changed = true;
                    }
                }
                GUILayout.Space(20);
                GUILayout.FlexibleSpace();
                this.ShowAddNewHit(fieldInfo);
                if (GUILayout.Button("Add", GUILayout.Width(40)))
                {
                    this.hitValues.Add(this.addHit);
                    this.addHit = default(FT);
                    changed = true;
                }

                Name = this.GetName();
                GUILayout.EndHorizontal();

                return changed;
            }

            public override void OnDisable()
            {
                this.hitValues.Clear();
            }

            public override bool IsActive()
            {
                return this.hitValues.Count > 0;
            }

            protected virtual void ShowAddNewHit(FieldInfo fieldInfo)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(100));
                this.addHit = (FT)FieldInspectorTool.GenericField(
                    "addHit", this.addHit,
                    fieldInfo.FieldType,
                    this.GetType().GetField("addHit", BindingFlags.Public | BindingFlags.Instance),
                    this,
                    false);
                GUILayout.EndHorizontal();
            }

            protected virtual string GetName()
            {
                string[] names = new string[this.hitValues.Count];
                for (int i = 0; i < this.hitValues.Count; i++)
                {
                    var hitString = this.hitValues[i];
                    names[i] = hitString.ToString();
                }
                return String.Format("{0}:{1}", FieldName, names.Joint("."));
            }
        }

        private class EnumFieldFilter : FieldFilter<Enum>
        {
            protected override void ShowAddNewHit(FieldInfo fieldInfo)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(100));
                if (this.addHit == null)
                {
                    this.addHit = (Enum)Enum.GetValues(fieldInfo.FieldType).GetValue(0);
                }
                this.addHit = EditorGUILayout.EnumPopup(this.addHit);
                GUILayout.EndHorizontal();
            }
        }

        private class StringFiledFilter : FieldFilter<string>
        {
            protected override void ShowAddNewHit(FieldInfo fieldInfo)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(100));
                var inspectorStyle = fieldInfo.GetAttribute<InspectorStyle>();
                var inspectorFieldParsers = FieldInspectorTool.inspectorFieldParsers;
                if (inspectorStyle != null && inspectorStyle.ParserName == "StringEnum" && inspectorFieldParsers != null && inspectorFieldParsers.ContainsKey("StringEnum"))
                {
                    this.addHit = inspectorFieldParsers["StringEnum"].ParserFiled(
                        inspectorStyle, this.addHit,
                        typeof(string),
                        null,
                        null,
                        false) as string;
                }
                else
                {
                    this.addHit = EditorGUILayout.TextField(this.addHit);
                }
                GUILayout.EndHorizontal();
            }

            public override bool RunFilter(T item)
            {
                if (this.hitValues.Count == 0)
                {
                    return true;
                }
                var fieldInfo = typeof(T).GetField(this.FieldName, BindingFlags.Public | BindingFlags.Instance);
                var fieldValue = (string)fieldInfo.GetValue(item);
                var lowerFieldValue = fieldValue.ToLower();
                var traditionalFieldValue = SearchTool.ToTraditional(lowerFieldValue);
                var simplifiedFieldValue = SearchTool.ToSimplified(lowerFieldValue);

                foreach (var hitValue in this.hitValues)
                {
                    var lowerHitValue = hitValue.ToLower();
                    if (lowerFieldValue.Contains(lowerHitValue) || traditionalFieldValue.Contains(lowerHitValue)
                        || simplifiedFieldValue.Contains(lowerHitValue))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private class IntFiledFilter : FieldFilter<int>
        {
            protected override void ShowAddNewHit(FieldInfo fieldInfo)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(100));
                var inspectorStyle = fieldInfo.GetAttribute<InspectorStyle>();
                var inspectorFieldParsers = FieldInspectorTool.inspectorFieldParsers;
                if (inspectorStyle != null && inspectorStyle.ParserName == "IntEnum" && inspectorFieldParsers != null && inspectorFieldParsers.ContainsKey("IntEnum"))
                {
                    this.addHit = (int)inspectorFieldParsers["IntEnum"].ParserFiled(
                        inspectorStyle, this.addHit,
                        typeof(string),
                        null,
                        null,
                        false);
                }
                else
                {
                    this.addHit = EditorGUILayout.IntField(this.addHit);
                }
                GUILayout.EndHorizontal();
            }
        }

        private class RangeFieldFilter<FT> : GUISearchBarFilter
        {
            public List<FT> fromFilter = new List<FT>();
            public List<FT> toFilter = new List<FT>();

            public string FieldName = null;

            protected FT from;

            protected FT to;

            public override bool RunFilter(T item)
            {
                if (fromFilter.Count == 0)
                {
                    return true;
                }
                var fieldInfo = typeof(T).GetField(this.FieldName, BindingFlags.Public | BindingFlags.Instance);
                try
                {
                    var s = (FT)fieldInfo.GetValue(item);
                    var fieldNum = Convert.ToDouble(s);

                    for (int i = 0; i < this.fromFilter.Count; i++)
                    {
                        var ft = this.fromFilter[i];
                        var tt = this.toFilter[i];
                        var fn = Convert.ToDouble(ft);
                        var tn = Convert.ToDouble(tt);

                        if (fn <= fieldNum && tn >= fieldNum)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return true;
                }

                return false;
            }

            public override bool Show()
            {
                bool changed = false;
                GUILayout.BeginHorizontal();
                var fieldInfo = typeof(T).GetField(this.FieldName, BindingFlags.Public | BindingFlags.Instance);
                var bestName = fieldInfo.GetBestName();
                GUILayout.Label(bestName + "范围", GUILayout.Width(146));

                for (int i = 0; i < this.fromFilter.Count; i++)
                {
                    var ff = this.fromFilter[i];
                    var tf = this.toFilter[i];
                    if (GUILayout.Button(String.Format("{0}-{1} {2}", ff,tf, "X".SetColor(new Color(0, 0, 0, 0.2f)))))
                    {
                        fromFilter.RemoveAt(i);
                        toFilter.RemoveAt(i);
                        i--;
                        changed = true;
                    }
                }

                GUILayout.Space(20);
                GUILayout.FlexibleSpace();
                this.ShowAddNewHit(fieldInfo);
                if (GUILayout.Button("Add",GUILayout.Width(40)))
                {
                    fromFilter.Add(this.from);
                    toFilter.Add(this.to);
                    this.from = default(FT);
                    this.to = default(FT);
                    changed = true;
                }

                Name = this.GetName();
                GUILayout.EndHorizontal();

                return changed;
            }

            public override void OnDisable()
            {
                fromFilter.Clear();
                toFilter.Clear();
            }

            public override bool IsActive()
            {
                return fromFilter.Count > 0;
            }

            protected virtual void ShowAddNewHit(FieldInfo fieldInfo)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(100));
                this.from = (FT)FieldInspectorTool.GenericField(
                    "from", this.from,
                    typeof(FT),
                    this.GetType().GetField("from", BindingFlags.Public | BindingFlags.Instance),
                    this,
                    false);
                GUILayout.Label("-");
                this.to = (FT)FieldInspectorTool.GenericField(
                    "to", this.to,
                    typeof(FT),
                    this.GetType().GetField("to", BindingFlags.Public | BindingFlags.Instance),
                    this,
                    false);
                GUILayout.EndHorizontal();
            }

            protected virtual string GetName()
            {
                string[] names = new string[fromFilter.Count];
                for (int i = 0; i < this.fromFilter.Count; i++)
                {
                    var ff = this.fromFilter[i];
                    var tf = this.toFilter[i];
                    names[i] = String.Format("{0}-{1}",ff,tf);
                }
                return String.Format("{0}:{1}", FieldName, names.Joint("."));
            }
        }
    }

    public static class SearchTool
    {
        /// <summary>
        /// 中文字符工具类
        /// </summary>
        private const int LOCALE_SYSTEM_DEFAULT = 0x0800;
        private const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        private const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int LCMapString(int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest);

        /// <summary>
        /// 将字符转换成简体中文
        /// </summary>
        /// <param name="source">输入要转换的字符串</param>
        /// <returns>转换完成后的字符串</returns>
        public static string ToSimplified(string source)
        {
            String target = new String(' ', source.Length);
            int ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_SIMPLIFIED_CHINESE, source, source.Length, target, source.Length);
            return target;
        }

        /// <summary>
        /// 讲字符转换为繁体中文
        /// </summary>
        /// <param name="source">输入要转换的字符串</param>
        /// <returns>转换完成后的字符串</returns>
        public static string ToTraditional(string source)
        {
            String target = new String(' ', source.Length);
            int ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_TRADITIONAL_CHINESE, source, source.Length, target, source.Length);
            return target;
        }
    }
}