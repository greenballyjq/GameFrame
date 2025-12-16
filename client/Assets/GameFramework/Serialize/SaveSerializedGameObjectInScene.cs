using System;

namespace GameFramework.Serialize
{
    /// <summary>
    /// 序列化游戏物体数据 - 场景模式读取
    /// </summary>
    [Serializable]
    public class SaveSerializedGameObjectInScene : SaveSerializedGameObject
    {
        public string ObjectIdInScene { get; set; }
    }
}