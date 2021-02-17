namespace Assets.Tools.Script.Editor.Tool.Tree
{
    public struct TreeViewItemState
    {
        public TreeViewOpenOperation OpenOperation;

        public TreeViewSelectOperation SelectOperation;
    }

    public enum TreeViewOpenOperation
    {
        None,
        Open,
        Close,
    }

    public enum TreeViewSelectOperation
    {
        None,
        Select,
        Deselect,
    }
}