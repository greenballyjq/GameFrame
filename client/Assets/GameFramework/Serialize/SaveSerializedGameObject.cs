using System;
using System.Collections.Generic;

namespace GameFramework.Serialize
{
    /// <summary>
    /// 序列化游戏物体数据
    /// </summary>
    [Serializable]
    public class SaveSerializedGameObject
    {
        public SaveSerializedGameObject[] Children { get; set; }
        public bool Active { get; set; }
        public List<ComponentSerialized> ComponentSerializedList { get; set; }
        [Obsolete] public GameObjectExistType ExistType { get; set; }
    }

    public enum GameObjectExistType
    {
        GameObjectInScene,
        GameObjectInAsset,
    }
}