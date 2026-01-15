namespace Project.VR.Runtime.HandPose
{
    public static class VRLogger
    {
        private const string k_Prefix = "<b>[VR HandSystem]</b>";

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void LogSetupWarning(string message)
        {
            UnityEngine.Debug.LogWarning($"{k_Prefix} <color=yellow>Setup Issue:</color> {message}");
        }
        
        public static void LogSimpleWarning(string message)
        {
            UnityEngine.Debug.LogWarning($"{k_Prefix} {message}");
        }
        
        public static void Log(string message)
        {
            UnityEngine.Debug.Log($"{k_Prefix} {message}");
        }

        public static void LogRuntimeError(string message, UnityEngine.Object context = null)
        {
            UnityEngine.Debug.LogError($"{k_Prefix} <color=red>Runtime Error:</color> {message}", context);
        }
    }
}