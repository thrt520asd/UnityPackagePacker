namespace Assets.Tools.Script.Editor.Tool
{
    using System;
    using System.Collections.Generic;

    using Assets.Tools.Script.Editor.Window;
    using Assets.Tools.Script.Reflec;

    using UnityEditor;

    using UnityEngine;

    public class GUIAddableList<T>
        where T : class
    {
        public void Draw(
            List<T> datas,
            Func<T, string> getDataName,
            Action<T> onSelect = null,
            Action<T> onAdd = null)
        {
            if (datas == null)
            {
                return;
            }
            GUILayout.BeginVertical();
            
            for (int i = 0; i < datas.Count; i++)
            {
                GUILayout.BeginHorizontal();
                var data = datas[i];
                var dataName = getDataName(data);
                if (GUILayout.Button(dataName))
                {
                    if (onSelect != null)
                    {
                        onSelect(data);
                    }
                }

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    datas.RemoveAt(i);
                    i--;
                }
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add"))
            {
                var instantiate = ReflecTool.Instantiate<T>();
                if (onAdd != null)
                {
                    onAdd(instantiate);
                }
                datas.Add(instantiate);
            }

            GUILayout.EndVertical();
        }
    }

    public class GUIAddableList<T,T2>
    {
        /// <summary>
        /// 显示一组可添加内容
        /// </summary>
        /// <param name="datas">当前数据</param>
        /// <param name="addSource">添加数据来源</param>
        /// <param name="getDataName">获得当前数据显示名字</param>
        /// <param name="getAddDataName">获得来源数据显示名字</param>
        /// <param name="onAdd">添加新回调</param>
        /// <param name="onSelect">选中当前数据回调</param>
        /// <param name="screen">筛选来源数据</param>
        public void Draw(
            List<T> datas,
            List<T2> addSource,
            Func<T, string> getDataName,
            Func<T2, string> getAddDataName,
            Action<T2> onAdd,
            Action<T> onSelect,
            Func<T2, bool> screen
            )
        {
            GUILayout.BeginVertical();

            for (int i = 0; i < datas.Count; i++)
            {
                GUILayout.BeginHorizontal();
                var data = datas[i];
                var dataName = getDataName(data);
                if (GUILayout.Button(dataName))
                {
                    if (onSelect != null)
                    {
                        onSelect(data);
                    }
                }

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    datas.RemoveAt(i);
                    i--;
                }
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add"))
            {
                var popMenuWindow = new PopMenuWindow();
                popMenuWindow.HasSearchBar = true;
                popMenuWindow.HasSelectTag = false;
                popMenuWindow.AutoSortItem = true;

                foreach (var addData in addSource)
                {
                    if (screen != null)
                    {
                        if (!screen(addData))
                        {
                            continue;
                        }
                    }
                    var addDataName = getAddDataName(addData);
                    popMenuWindow.AddItem(addDataName,false,
                        (o) =>
                            { onAdd((T2)o); }, addData);
                }

                popMenuWindow.PopWindow();
            }

            GUILayout.EndVertical();
        }
    }
}