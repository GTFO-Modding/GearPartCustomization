using BepInEx.Logging;

namespace GearPartCustomization
{
    static class Log
    {
        public static void Debug(object value) => s_Source.LogDebug(value);
        public static void Error(object value) => s_Source.LogError(value);
        public static void Fatal(object value) => s_Source.LogFatal(value);
        public static void Info(object value) => s_Source.LogInfo(value);
        public static void Message(object value) => s_Source.LogMessage(value);
        public static void Warning(object value) => s_Source.LogWarning(value);
        public static void Verbose(object value)
        {
            if (Plugin.VerboseLogging.Value) s_Source.LogDebug(value);
        }
        public static ManualLogSource s_Source;
    }
}