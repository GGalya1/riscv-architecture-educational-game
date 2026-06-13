using System.Diagnostics;
using Debug = UnityEngine.Debug;

public static class CustomLog
{
    [Conditional("UNITY_EDITOR")]
    public static void LogEditorError(string message)
    {
        Debug.LogError(message);
    }

    [Conditional("UNITY_EDITOR")]
    public static void LogEditor(string message)
    {
        Debug.Log(message);
    }
    
    [Conditional("UNITY_EDITOR")]
    public static void LogEditorWarning(string message)
    {
        Debug.LogWarning(message);
    }
}