using PluginAPI.Core;

namespace SCPDiscord
{
    public static class Logger
    {
        public static void Info(string message)
        {
            Log.Info(message);
        }

        public static void Warn(string message)
        {
            Log.Warning(message);
        }

        public static void Error(string message)
        {
            Log.Error(message);
        }

        public static void Debug(string message)
        {
            if (Config.GetBool("settings.debug"))
            {
                Log.Info(message);
            }
        }

        public static void DebugWarn(string message)
        {
            if (Config.GetBool("settings.debug"))
            {
                Log.Warning(message);
            }
        }

        public static void DebugError(string message)
        {
            if (Config.GetBool("settings.debug"))
            {
                Log.Error(message);
            }
        }
    }
}