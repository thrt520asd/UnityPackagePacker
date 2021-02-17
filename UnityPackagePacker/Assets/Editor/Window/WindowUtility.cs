using System;
using System.Collections.Generic;
using System.Reflection;

//using Assets.Script.Editor.Util;
using Assets.Tools.Script.Reflec;


using UnityEditor;

using UnityEngine;

public class WindowUtility
{
    public enum DockPosition
    {
        Left,
        Right,
        Bottom
    }

    public class DockOperation
    {
        public DockPosition dockPosition;

        public EditorWindow parent;

        public EditorWindow child;

        public DockOperation(EditorWindow child, EditorWindow parent, DockPosition dockPosition)
        {
            this.child = child;
            this.parent = parent;
            this.dockPosition = dockPosition;
        }
    }
    public bool Dock(DockOperation oper)
    {
        return Dock(oper.dockPosition, oper.parent, oper.child);
    }

    /// <summary>
    /// 获得目标window停靠在一起的其他window
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns> 
    public List<EditorWindow> GetDockWindows(EditorWindow window)
    {
        List<EditorWindow>resultList=new List<EditorWindow>();
        object root = parentRoot(window);
      //  object dockArea = ReflecTool.GetPrivateField<object>(window, "m_Parent");
       // object splitView = ReflecTool.GetPrivateProperty(dockArea, "parent");
     //   Array panes = (Array)ReflecTool.GetPrivateProperty(splitView, "children");
        Array panes = (Array)ReflecTool.GetPrivateProperty(root, "allChildren");
        foreach (var pane in panes)
        {
            List<EditorWindow> windows = ReflecTool.GetPrivateField<List<EditorWindow>>(pane, "m_Panes");
          if(windows!=null)
            resultList.AddRange(windows);
        }
        return resultList;

    }

    private  object parentRoot(object obj)
    {
        object parent = ReflecTool.GetPrivateField<object>(obj, "m_Parent");
        if(parent==null)
            parent = ReflecTool.GetPrivateProperty(obj, "parent");
        if (parent == null)
        {
            return obj;
        }
        else
        {
            return parentRoot(parent);
        }
    }
    /// <summary>
    /// 将childwindow停靠在parentwindow中
    /// </summary>
    /// <param name="dockPosition"></param>
    /// <param name="parent"></param>
    /// <param name="child"></param>
    /// <returns></returns>
    public  bool Dock(DockPosition dockPosition, EditorWindow parent, EditorWindow child)
    {
        Vector2 mouseScreenPosition = Vector2.zero;
        //获得目标窗体的父容器 需要是一个dockarea 即可停靠的区域
        object dockArea = ReflecTool.GetPrivateField<object>(parent, "m_Parent");
        //DockArea 子窗体区域
        //   object dockArea_child = ReflecTool.GetPrivateField<object>(child, "m_Parent");
        //获取可停靠容器的分屏视图
        object splitView = ReflecTool.GetPrivateProperty(dockArea, "parent");
        //获得视图在屏幕中的位置
        Rect screenPosition = parent.position;// (Rect)ReflecTool.GetPrivateProperty(splitView, "screenPosition");
        float ZoneWidth = Mathf.Round(Mathf.Min(1f + screenPosition.width / 4f, 260f));
        float ZoneHeight = Mathf.Round(Mathf.Min(1f + screenPosition.height / 4f, 260f));
        
        //switch (dockPosition)
        //{
        //    case DockPosition.Left:
        //        {
        //            mouseScreenPosition = screenPosition.TopLeft() + new Vector2(ZoneWidth, 40f);
        //        } break;
        //    case DockPosition.Right:
        //        {
        //            mouseScreenPosition = screenPosition.TopRight() + new Vector2(-ZoneWidth, 40f); ;
        //        }
        //        break;
        //    case DockPosition.Bottom:
        //        {
        //            mouseScreenPosition = screenPosition.BottomLeft().DeltaXNew(screenPosition.width * 0.5f).DeltaY(-ZoneHeight);
        //        }
        //        break;
        //}
        /*   判断是否已经在分频视图内
         * Array panes = (Array)ReflecTool.GetPrivateProperty(splitView, "children");
          foreach (var pane in panes)
          {
              if (pane == dockArea_child) 
                  return false;
          }*/
        //生成提取tab和目标区域的放置信息
        //object dropInfo = InvokeMethod(area, "DragOver", new object[] { child, ((Rect)GetPrivateProperty(area, "screenPosition")).position });
        object dropInfo = ReflecTool.InvokeMethod(splitView, "DragOver", new object[] { child, mouseScreenPosition });
        #region 注释 内部分屏的具体算法
        /*  
      * object test = ReflecTool.InvokeMethod(splitView, "CheckRootWindowDropZones", new object[] { mouseScreenPosition }, BindingFlags.NonPublic | BindingFlags.Instance);
        Array panes = (Array)ReflecTool.GetPrivateProperty(splitView, "children");
        foreach (var pane in panes)
        {
            screenPosition = (Rect)ReflecTool.GetPrivateProperty(splitView, "screenPosition");
            int num = 0;
            float num2 = Mathf.Round(Mathf.Min(screenPosition.width / 3f, 300f));
            float num3 = Mathf.Round(Mathf.Min(screenPosition.height / 3f, 300f));
            Rect rect = new Rect(screenPosition.x, screenPosition.y + 39f, num2, screenPosition.height - 39f);
            if (rect.Contains(mouseScreenPosition))
            {
                num |= 1;
            }
            Rect rect2 = new Rect(screenPosition.x, screenPosition.yMax - num3, screenPosition.width, num3);
            if (rect2.Contains(mouseScreenPosition))
            {
                num |= 2;
            }
            Rect rect3 = new Rect(screenPosition.xMax - num2, screenPosition.y + 39f, num2, screenPosition.height - 39f);
            if (rect3.Contains(mouseScreenPosition))
            {
                num |= 4;
            }
            if (num == 3)
            {
                Vector2 vector = new Vector2(screenPosition.x, screenPosition.yMax) - mouseScreenPosition;
                Vector2 vector2 = new Vector2(num2, -num3);
                if (vector.x * vector2.y - vector.y * vector2.x < 0f)
                {
                    num = 1;
                }
                else
                {
                    num = 2;
                }
            }
            else if (num == 6)
            {
                Vector2 vector3 = new Vector2(screenPosition.xMax, screenPosition.yMax) - mouseScreenPosition;
                Vector2 vector4 = new Vector2(-num2, -num3);
                if (vector3.x * vector4.y - vector3.y * vector4.x < 0f)
                {
                    num = 2;
                }
                else
                {
                    num = 4;
                }
            }
            float num4 = Mathf.Round(Mathf.Max(screenPosition.width / 3f, 100f));
            float num5 = Mathf.Round(Mathf.Max(screenPosition.height / 3f, 100f));
        }*/
        #endregion

        if (dropInfo != null)
        {
            ReflecTool.SetPrivateFiled(
                dockArea,
                "s_OriginalDragSource",
                ReflecTool.GetPrivateField<object>(child, "m_Parent"),
                BindingFlags.NonPublic | BindingFlags.Static);
            try
            {
                ReflecTool.InvokeMethod(splitView, "PerformDrop", new object[] { child, dropInfo, mouseScreenPosition });
            }
            catch (Exception exception)
            {
                //Debug.LogError("放置区域错误 只能放置目标窗体左边或者右边 或者下面" + exception);
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}
