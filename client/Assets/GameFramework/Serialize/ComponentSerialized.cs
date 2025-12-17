using System;

namespace GameFramework.Serialize
{
    /// <summary>
    /// 游戏物体组件序列化数据
    /// </summary>
    [Serializable]
    public class ComponentSerialized
    {
        public string ComponentType { get; set; }
        public string JsonData { get; set; }
    }
}