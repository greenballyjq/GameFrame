using UnityEngine;

namespace GameFramework.Serialize
{
    /// <summary>
    /// 序列化游戏物体挂载脚本 - 标记该游戏物体需要序列化保存 （资源模式读取）
    /// </summary>
    public class SerializeObjectInAsset : SerializeObject
    {
        [SerializeField] private string assetPath;
        
        public override SaveSerializedGameObject Save()
        {
            var saveSerializedComponents = TravesalSerialize(transform);
            return new SaveSerializedGameObjectInAsset
            {
                ExistType = GameObjectExistType.GameObjectInAsset,
                AssetPath = assetPath,
                SaveSerializedComponents = saveSerializedComponents,
            };  
        }
    }
}