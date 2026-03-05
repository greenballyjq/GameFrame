using System;
using System.Diagnostics;

namespace GameFramework.Utility
{
    /// <summary>
    /// 代码性能计时类
    /// </summary>
    public class CodeTimer : IDisposable
    {
        /// <summary>
        /// 计时名称
        /// </summary>
        private string _name;

        /// <summary>
        /// 代码执行次数
        /// </summary>
        private int _iterations;

        /// <summary>
        /// 高精度计时器
        /// </summary>
        private Stopwatch _stopwatch;

        private CodeTimer(string name, int iterations)
        {
            _name = name;
            _iterations = iterations;
            _stopwatch = Stopwatch.StartNew();
        }

        #region 公共方法
        /// <summary>
        /// 开始计时
        /// </summary>
        public static CodeTimer Start(string name, int iterations = 1)
        {
            return new CodeTimer(name, iterations);
        }

        /// <summary>
        /// 停止计时并输出日志
        /// </summary>
        public void Dispose()
        {
            _stopwatch.Stop();
            double totalMs = _stopwatch.Elapsed.TotalMilliseconds;

            string message;
            if (_iterations <= 1)
            {
                message = $"[CodeTimer] {_name} - 执行了 1 次，耗时 {totalMs:F2} 毫秒";
            }
            else
            {
                double avgMs = totalMs / _iterations;
                message = $"[CodeTimer] {_name} - 执行了 {_iterations} 次，总耗时 {totalMs:F2} 毫秒，平均耗时 {avgMs:F4} 毫秒";
            }

            Log.Info(message);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
