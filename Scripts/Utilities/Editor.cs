#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public static class EditorHelper
{
    [System.Diagnostics.Conditional("DEBUG")]
    public static void Log(string message)
    {
        Debug.Log(message);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void SetDirty(Object obj)
    {
#if UNITY_EDITOR 
        EditorUtility.SetDirty(obj);
#endif
    }
}