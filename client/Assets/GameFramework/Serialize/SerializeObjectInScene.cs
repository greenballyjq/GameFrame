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
            var saveSerializedComponents = TravesalSerialize(transform);
            return new SaveSerializedGameObjectInScene
            {
                ExistType = GameObjectExistType.GameObjectInScene,
                ObjectIdInScene = objectId,
                SaveSerializedComponents = saveSerializedComponents,
            }; 
        }

        public override void Load(SaveSerializedComponents saveSerializedComponents)
        {
            TravesalDeserialize(transform, saveSerializedComponents);
        }
    }
}