using System;
using System.IO;
using UnityEngine;

namespace GameFramework.Utility
{
    /// <summary>
    /// 调试日志封装类
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// 当前Play会话的日志文件路径
        /// </summary>
        private static string _logFilePath;

        /// <summary>
        /// 文件写入器
        /// </summary>
        private static StreamWriter _logWriter;

        /// <summary>
        /// 文件写入锁
        /// </summary>
        private static readonly object _fileLock = new object();

        #region 公共方法
        /// <summary>
        /// 输出普通日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Info(object message)
        {
            Debug.Log(message);
            WriteToFile("Info", message?.ToString() ?? "");
        }

        /// <summary>
        /// 输出带上下文的普通日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Info(object message, UnityEngine.Object context)
        {
            Debug.Log(message, context);
            WriteToFile("Info", message?.ToString() ?? "");
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Warning(object message)
        {
            Debug.LogWarning(message);
            WriteToFile("Warning", message?.ToString() ?? "");
        }

        /// <summary>
        /// 输出带上下文的警告日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Warning(object message, UnityEngine.Object context)
        {
            Debug.LogWarning(message, context);
            WriteToFile("Warning", message?.ToString() ?? "");
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Error(object message)
        {
            Debug.LogError(message);
            WriteToFile("Error", message?.ToString() ?? "");
        }

        /// <summary>
        /// 输出带上下文的错误日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Error(object message, UnityEngine.Object context)
        {
            Debug.LogError(message, context);
            WriteToFile("Error", message?.ToString() ?? "");
        }

        /// <summary>
        /// 输出异常日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Exception(Exception exception)
        {
            Debug.LogException(exception);
            WriteToFile("Exception", exception?.ToString() ?? "");
        }

        /// <summary>
        /// 输出带上下文的异常日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Exception(Exception exception, UnityEngine.Object context)
        {
            Debug.LogException(exception, context);
            WriteToFile("Exception", exception?.ToString() ?? "");
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 将日志写入文件
        /// </summary>
        private static void WriteToFile(string level, string message)
        {
            lock (_fileLock)
            {
                if (_logWriter == null)
                {
                    string logDir = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Logs");
                    Directory.CreateDirectory(logDir);
                    _logFilePath = Path.Combine(logDir, DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
                    _logWriter = new StreamWriter(_logFilePath, append: true);
                }

                string line = $"[{DateTime.Now:HH:mm:ss.fff}] [{level}] {message}";
                _logWriter.WriteLine(line);
                _logWriter.Flush();
            }
        }
        #endregion
    }
}
