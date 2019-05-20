using System;

namespace LeanCloud.Play
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel {
        /// <summary>
        /// 调试级别
        /// </summary>
        Debug,
        /// <summary>
        /// 警告级别
        /// </summary>
        Warn,
        /// <summary>
        /// 错误级别
        /// </summary>
        Error,
    }

    /// <summary>
    /// 日志类
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// 日志回调接口，方便开发者调试
        /// </summary>
        /// <value>The log delegate.</value>
        public static Action<LogLevel, string> LogDelegate {
            get; set;
        }

        internal static void Debug(string log) {
            if (LogDelegate != null) {
                LogDelegate.Invoke(LogLevel.Debug, log);
            }
        }

        internal static void Debug(string format, params object[] args) {
            if (LogDelegate != null) {
                LogDelegate.Invoke(LogLevel.Debug, string.Format(format, args));
            }
        }

        internal static void Warn(string log) {
            if (LogDelegate != null) {
                LogDelegate.Invoke(LogLevel.Warn, log);
            }
        }

        internal static void Warn(string format, params object[] args) {
            if (LogDelegate != null) {
                LogDelegate.Invoke(LogLevel.Warn, string.Format(format, args));
            }
        }

        internal static void Error(string log) {
            if (LogDelegate != null) {
                LogDelegate.Invoke(LogLevel.Error, log);
            }
        }

        internal static void Error(string format, params object[] args) {
            if (LogDelegate != null) {
                LogDelegate.Invoke(LogLevel.Error, string.Format(format, args));
            }
        }
    }
}
