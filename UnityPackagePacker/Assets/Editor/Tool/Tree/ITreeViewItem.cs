namespace Assets.Tools.Script.Editor.Tool.Tree
{
    using System.Collections.Generic;

    public interface ITreeViewItem
    {
        string Name { get; set; }

        bool IsSelect { get; set; }

        bool IsOpen { get; set; }

        int Depth { get; set; }

        ITreeViewDirectory ParentDirectory { get; set; }

        TreeViewItemState Show();
    }
}