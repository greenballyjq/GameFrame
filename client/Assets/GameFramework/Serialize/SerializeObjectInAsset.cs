using UnityEngine;

namespace GameFramework.Serialize
{
    /// <summary>
    /// 序列化游戏物体挂载脚本 - 标记该游戏物体需要序列化保存 （资源模式读取）
    /// </summary>
    public class SerializeObjectInAsset : SerializeObject
    {
        [SerializeField] private string assetPath;
        
        private SerializeObjectParent _parent;
        
        public override SaveSerializedGameObject Save()
        {
            var saveSerializedComponents = TravesalSerialize(transform);
            return new SaveSerializedGameObjectInAsset
            {
                ParentObjectId = "",
                AssetPath = assetPath,
                ExistType = GameObjectExistType.GameObjectInAsset,
                SaveSerializedComponents = saveSerializedComponents,
            };
        }

        public override void Load(SaveSerializedComponents saveSerializedComponents)
        {
            TravesalDeserialize(transform, saveSerializedComponents);
        }
        
        public void SetParent(SerializeObjectParent parent) => _parent = parent;
    }
}