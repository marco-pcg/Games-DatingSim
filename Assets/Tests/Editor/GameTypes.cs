using System;
using System.Reflection;

internal static class GameTypes
{
    public static Type Find(string name)
    {
        Type type = Type.GetType(name + ", Assembly-CSharp", true);
        return type;
    }
    public static object Call(object target, string name, params object[] args)
    {
        return target.GetType().GetMethod(name).Invoke(target, args);
    }
    public static void Set(object target, string name, object value)
    {
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .SetValue(target, value);
    }
    public static object Get(object target, string name)
    {
        return target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .GetValue(target);
    }
}
