using System;

namespace GameFramework.Serialize
{
    /// <summary>
    /// 脚本序列化数据
    /// </summary>
    [Serializable]
    public class MonoBehaviourSerialized
    {
        public string MonoBehaviourType { get; set; }
        public string JsonData { get; set; }
    }
}