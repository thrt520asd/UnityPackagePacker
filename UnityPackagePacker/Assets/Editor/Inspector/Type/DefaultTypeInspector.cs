namespace Assets.Tools.Script.Editor.Inspector.Type
{
    using System;
    using System.Reflection;

    public abstract class DefaultTypeInspector
    {
        public abstract Type GetInspectorType();
        public abstract object Show(
            string name,
            object value,
            Type t,
            FieldInfo fieldInfo,
            object instance,
            bool withName = true);
    }
}