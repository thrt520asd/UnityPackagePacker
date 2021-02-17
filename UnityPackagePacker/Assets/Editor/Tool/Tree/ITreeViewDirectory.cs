namespace Assets.Tools.Script.Editor.Tool.Tree
{
    using System.Collections.Generic;

    public interface ITreeViewDirectory : ITreeViewItem
    {
        void AddChild(ITreeViewItem item);

        void RemoveChild(ITreeViewItem item);

        ITreeViewItem GetChild(string name);

        List<ITreeViewItem> GetChildren();

        void BuildItemList(List<ITreeViewItem> list,int depth,bool withNotOpen);
    }
}