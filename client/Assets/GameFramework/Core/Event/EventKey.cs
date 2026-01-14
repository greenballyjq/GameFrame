using System;
namespace GameFramework.Core
{
    /// <summary>
    /// 事件键基类
    /// </summary>
    public class EventKey {}

    /// <summary>
    /// 泛型事件键
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventKey<T> : EventKey where T : EventArgs { }

    /// <summary>
    /// 事件参数基类
    /// </summary>
    public class EventArgs
    {
        public object Sender { get; set; }
    }


}