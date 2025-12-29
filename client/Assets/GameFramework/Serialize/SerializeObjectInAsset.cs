#if FRAMEWORK_SERIALIZE

using System;
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

        private void OnEnable()
        {
            _parent = GetComponentInParent<SerializeObjectParent>();
        }

        public override SaveSerializedGameObject Save()
        {
            var serializedGameObject = TravesalSerialize(transform);
            return new SaveSerializedGameObjectInAsset
            {
                Active = serializedGameObject.Active,
                Children = serializedGameObject.Children,
                ComponentSerializedList = serializedGameObject.ComponentSerializedList,
                ExistType = GameObjectExistType.GameObjectInAsset,
                AssetPath = assetPath,
                ParentObjectId = _parent.ObjectId,
            };
        }

        public override void Load(SaveSerializedGameObject saveSerializedGameObject)
        {
            gameObject.SetActive(saveSerializedGameObject.Active);
            TravesalDeserialize(transform, saveSerializedGameObject);
        }
    }
}
#endif