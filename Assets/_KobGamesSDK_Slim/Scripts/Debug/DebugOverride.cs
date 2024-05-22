using System;
using System.Diagnostics;
using UnityEngine;

#if !ENABLE_LOGS
// When ENABLE_LOGS is not defined, this class will override Unity's Debug class in order to strip all the functions from release version
// The Conditional("DUMMY") is in charge to strip all the calls to those functions, since there will never be a "DUMMY" defined
public static class Debug
{
#if UNITY_EDITOR
    public static bool isDebugBuild = true;
#else
    public static bool isDebugBuild { get { return UnityEngine.Debug.isDebugBuild; } }

#endif
    public static ILogger unityLogger { get { return UnityEngine.Debug.unityLogger; } }



    [Conditional("DUMMY")]
    public static void Assert(bool condition, string format, params object[] args) { }
    [Conditional("DUMMY")]
    public static void Assert(bool condition, string message, UnityEngine.Object context) { }
    [Conditional("DUMMY")]
    public static void Assert(bool condition) { }
    [Conditional("DUMMY")]
    public static void Assert(bool condition, object message, UnityEngine.Object context) { }
    [Conditional("DUMMY")]
    public static void Assert(bool condition, string message) { }
    [Conditional("DUMMY")]
    public static void Assert(bool condition, object message) { }
    [Conditional("DUMMY")]
    public static void Assert(bool condition, UnityEngine.Object context) { }


    [Conditional("DUMMY")]
    public static void AssertFormat(bool condition, UnityEngine.Object context, string format, params object[] args) { }
    [Conditional("DUMMY")]
    public static void AssertFormat(bool condition, string format, params object[] args) { }


    [Conditional("DUMMY")]
    public static void Break() { }
    [Conditional("DUMMY")]
    public static void ClearDeveloperConsole() { }
    [Conditional("DUMMY")]
    public static void DebugBreak() { }


    [Conditional("DUMMY")]
    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration) { }
    [Conditional("DUMMY")]
    public static void DrawLine(Vector3 start, Vector3 end, Color color) { }
    [Conditional("DUMMY")]
    public static void DrawLine(Vector3 start, Vector3 end) { }
    [Conditional("DUMMY")]
    public static void DrawLine(Vector3 start, Vector3 end, [UnityEngine.Internal.DefaultValue("Color.white")] Color color, [UnityEngine.Internal.DefaultValue("0.0f")] float duration, [UnityEngine.Internal.DefaultValue("true")] bool depthTest) { }
    

    [Conditional("DUMMY")]
    public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration) { }
    [Conditional("DUMMY")]
    public static void DrawRay(Vector3 start, Vector3 dir, Color color) { }
    [Conditional("DUMMY")]
    public static void DrawRay(Vector3 start, Vector3 dir, [UnityEngine.Internal.DefaultValue("Color.white")] Color color, [UnityEngine.Internal.DefaultValue("0.0f")] float duration, [UnityEngine.Internal.DefaultValue("true")] bool depthTest) { }
    [Conditional("DUMMY")]
    public static void DrawRay(Vector3 start, Vector3 dir) { }


    [Conditional("DUMMY")]
    public static void Log(object message) { }
    [Conditional("DUMMY")]
    public static void Log(object message, UnityEngine.Object context) { }


    [Conditional("DUMMY")]
    public static void LogAssertion(object message, UnityEngine.Object context) { }
    [Conditional("DUMMY")]
    public static void LogAssertion(object message) { }


    [Conditional("DUMMY")]
    public static void LogAssertionFormat(UnityEngine.Object context, string format, params object[] args) { }
    [Conditional("DUMMY")]
    public static void LogAssertionFormat(string format, params object[] args) { }


    [Conditional("DUMMY")]
    public static void LogError(object message, UnityEngine.Object context) { }
    [Conditional("DUMMY")]
    public static void LogError(object message) { }


    [Conditional("DUMMY")]
    public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args) { }
    [Conditional("DUMMY")]
    public static void LogErrorFormat(string format, params object[] args) { }


    [Conditional("DUMMY")]
    public static void LogException(Exception exception) { }
    [Conditional("DUMMY")]
    public static void LogException(Exception exception, UnityEngine.Object context) { }


    [Conditional("DUMMY")]
    public static void LogFormat(string format, params object[] args) { }
    [Conditional("DUMMY")]
    public static void LogFormat(UnityEngine.Object context, string format, params object[] args) { }
    [Conditional("DUMMY")]
    public static void LogFormat(LogType logType, LogOption logOptions, UnityEngine.Object context, string format, params object[] args) { }
    
    
    [Conditional("DUMMY")]
    public static void LogWarning(object message, UnityEngine.Object context) { }
    [Conditional("DUMMY")]
    public static void LogWarning(object message) { }


    [Conditional("DUMMY")]
    public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args) { }
    [Conditional("DUMMY")]
    public static void LogWarningFormat(string format, params object[] args) { }





    [Conditional("DUMMY")] // Since dummy is not presented all functions will be stripped
    public static void LogInfo(string message, UnityEngine.Object obj = null)
    {
        //UnityEngine.Debug.Log(message, obj);
    }

    [Conditional("DUMMY")]
    public static void LogWarning(string message, UnityEngine.Object obj = null)
    {
        //UnityEngine.Debug.LogWarning(message, obj);
    }
}
#endif