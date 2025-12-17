using System;

namespace GameFramework.Serialize
{
    /// <summary>
    /// 序列化游戏物体数据 - 资源模式读取
    /// </summary>
    [Serializable]
    public class SaveSerializedGameObjectInAsset : SaveSerializedGameObject
    {
        public string AssetPath { get; set; }
        public string ParentObjectId { get; set; }
    }
}