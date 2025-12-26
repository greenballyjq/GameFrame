#if FRAMEWORK_SERIALIZE

using GameFramework.Core;
using GameFramework.Manager;
using UnityEngine;

namespace GameFramework.Serialize
{
    /// <summary>
    /// 标记资源加载模式的对象的父物体
    /// </summary>
    public class SerializeObjectParent : MonoBehaviour
    {
        [SerializeField] private string objectId;

        public string ObjectId => objectId;
        
        private  SaveLoadManager _saveLoadManager;

        private async void Start()
        {
            await GameFrameworkManager.Instance.WaitForInitializationAsync();
            _saveLoadManager = GameFrameworkManager.Instance.GetManager<SaveLoadManager>();
            _saveLoadManager.AddSerializeObjectParent(objectId, transform);
        }

        private void OnDestroy()
        {
            _saveLoadManager.RemoveSerializeObjectParent(objectId);
        }
    }
}
#endif