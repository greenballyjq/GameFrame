using System;
using UnityEngine;

namespace GameFramework.Utility
{
    /// <summary>
    /// 调试日志封装类
    /// </summary>
    public static class Log
    {
        #region 公共方法
        /// <summary>
        /// 输出普通日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Info(object message)
        {
            Debug.Log(message);
        }

        /// <summary>
        /// 输出带上下文的普通日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Info(object message, UnityEngine.Object context)
        {
            Debug.Log(message, context);
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Warning(object message)
        {
            Debug.LogWarning(message);
        }

        /// <summary>
        /// 输出带上下文的警告日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Warning(object message, UnityEngine.Object context)
        {
            Debug.LogWarning(message, context);
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Error(object message)
        {
            Debug.LogError(message);
        }

        /// <summary>
        /// 输出带上下文的错误日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Error(object message, UnityEngine.Object context)
        {
            Debug.LogError(message, context);
        }

        /// <summary>
        /// 输出异常日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Exception(Exception exception)
        {
            Debug.LogException(exception);
        }

        /// <summary>
        /// 输出带上下文的异常日志
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Exception(Exception exception, UnityEngine.Object context)
        {
            Debug.LogException(exception, context);
        }
        #endregion
    }
}
