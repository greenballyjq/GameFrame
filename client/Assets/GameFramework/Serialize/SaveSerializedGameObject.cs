using System;

namespace GameFramework.Serialize
{
    /// <summary>
    /// 序列化游戏物体数据
    /// </summary>
    [Serializable]
    public class SaveSerializedGameObject
    {
        public GameObjectExistType ExistType { get; set; }
        public SaveSerializedComponents SaveSerializedComponents { get; set; }
    }

    public enum GameObjectExistType
    {
        GameObjectInScene,
        GameObjectInAsset,
    }
}