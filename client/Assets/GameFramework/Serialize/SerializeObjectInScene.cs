#if FRAMEWORK_SERIALIZE
using UnityEngine;

namespace GameFramework.Serialize
{
    /// <summary>
    /// 序列化游戏物体挂载脚本 - 标记该游戏物体需要序列化保存 （场景模式读取）
    /// </summary>
    public class SerializeObjectInScene : SerializeObject
    {
        [SerializeField] private string objectId;

        public string ObjectId => objectId;
        
        public override SaveSerializedGameObject Save()
        {
            var saveSerializedGameObject = TravesalSerialize(transform);
            return new SaveSerializedGameObjectInScene
            {
                Active = saveSerializedGameObject.Active,
                ObjectIdInScene = objectId,
                Children = saveSerializedGameObject.Children,
                ComponentSerializedList = saveSerializedGameObject.ComponentSerializedList,
                ExistType = GameObjectExistType.GameObjectInScene,
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