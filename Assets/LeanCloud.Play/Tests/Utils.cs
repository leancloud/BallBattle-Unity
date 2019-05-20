using System;
using UnityEngine;

namespace LeanCloud.Play.Test {
    internal static class Utils {
        internal static Client NewClient(string userId) {
            var appId = "Eohx7L4EMfe4xmairXeT7q1w-gzGzoHsz";
            var appKey = "GSBSGpYH9FsRdCss8TGQed0F";
            return new Client(appId, appKey, userId);
        }

        internal static void Log(LogLevel level, string info) { 
            switch (level) {
                case LogLevel.Debug:
                    Debug.LogFormat("[DEBUG] {0}", info);
                    break;
                case LogLevel.Warn:
                    Debug.LogFormat("[WARNING] {0}", info);
                    break;
                case LogLevel.Error:
                    Debug.LogFormat("[ERROR] {0}", info);
                    break;
                default:
                    Debug.Log(info);
                    break;
            }
        }
    }
}
