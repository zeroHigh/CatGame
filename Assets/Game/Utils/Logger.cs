using UnityEngine;

namespace Game
{
    public class Logger
    {
        //是否要打开日志记录
        public static bool EnableLog = false;

        private static string blue = "55A4FF";
        private static string yellow = "EFEC1F";
        private static string green = "1AFF30";
        private static string red = "FF1104";

        public static void Log(string info)
        {
            if (EnableLog)
            {
                Debug.Log(info);
#if !UNITY_EDITOR
            UnityCallWeb.Log(info);
#endif
            }

        }

        public static void LogWarning(string info)
        {
            if (EnableLog)
                Debug.Log(info);
        }

        public static void LogError(string info)
        {
            if (EnableLog)
                Debug.LogError(info);
        }

        public static void LogBlue(string value)
        {
            Log(blue, value);
        }

        public static void LogYellow(string value)
        {
            Log(yellow, value);
        }

        public static void LogGreen(string value)
        {
            Log(green, value);
        }

        public static void LogRed(string value)
        {
            Log(red, value);
        }

        private static void Log(string color, object msg)
        {
            if (!EnableLog)
                return;
#if UNITY_EDITOR
            msg = string.Concat("<color=#", color, ">", msg.ToString(), "</color>");
#endif
            Debug.Log(msg);
        }
    }
}