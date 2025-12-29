#if FRAMEWORK_SERIALIZE
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.Manager;
using GameFramework.Serialize.Interfaces;
using UnityEngine;

namespace GameFramework.Serialize
{
    /// <summary>
    /// 序列化游戏物体挂载脚本 - 标记该游戏物体需要序列化保存
    /// </summary>
    public abstract class SerializeObject : MonoBehaviour
    {
        private SaveLoadManager _saveLoadManager;

        private async void Start()
        {
            await GameFrameworkManager.Instance.WaitForInitializationAsync();
            _saveLoadManager = GameFrameworkManager.Instance.GetManager<SaveLoadManager>();
            _saveLoadManager.AddSerializeObject(this);

            #region Test

            // var saveItem = JsonConvert.DeserializeObject<SaveComponents>(TestJson);
            // if (saveItem == null)
            // {
            //     Debug.Log($"[{GetType().Name}] SaveItem is null");
            // }
            // else
            // {
            //     Debug.Log($"[{GetType().Name}] Start Deserialize");
            //     Load(saveItem);
            // }
            // var saveItem = Save();
            // Debug.Log($"[{GetType().Name}] {JsonConvert.SerializeObject(saveItem)}");

            #endregion
        }

        private void OnDestroy()
        {
            _saveLoadManager.RemoveSerializeObject(this);
        }

        public abstract SaveSerializedGameObject Save();

        public abstract void Load(SaveSerializedGameObject saveSerializedGameObject);

        /// <summary>
        /// 回溯算法实现游戏物体和子物体的组件与脚本实例序列化
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected SaveSerializedGameObject TravesalSerialize(Transform target)
        {
            if (!target) return null;
            var components = target.GetComponents<Component>();
            List<ComponentSerialized> componentSerializedList = new List<ComponentSerialized>();
            foreach (var component in components)
            {
                // 组件是否为脚本组件
                if (component is MonoBehaviour)
                {
                    // 该脚本组件是否实现了ISerializable接口
                    if (component is ISerializable serializable)
                    {
                        componentSerializedList.Add(new ComponentSerialized
                        {
                            ComponentType = component.GetType().Name,
                            JsonData = serializable.Serialize(),
                        });
                    }
                }
                else
                {
                    componentSerializedList.Add(new ComponentSerialized
                    {
                        ComponentType = component.GetType().Name,
                        JsonData = component.Serialize()
                    });
                }
            }

            var saveGameObject = new SaveSerializedGameObject
            {
                Children = new SaveSerializedGameObject[target.childCount],
                Active = target.gameObject.activeSelf,
                ComponentSerializedList = componentSerializedList,
            };

            for (int i = 0; i < target.childCount; i++)
            {
                saveGameObject.Children[i] = TravesalSerialize(target.GetChild(i));
            }

            return saveGameObject;
        }

        /// <summary>
        /// 回溯算法实现游戏物体和子物体的组件与脚本实例反序列化
        /// </summary>
        /// <param name="target"></param>
        /// <param name="loadedSaveSerializedGameObject"></param>
        /// <returns></returns>
        protected void TravesalDeserialize(Transform target, SaveSerializedGameObject loadedSaveSerializedGameObject)
        {
            if (!target) return;
            Debug.Log($"[{GetType().Name}] TravesalDeserialize {target.name}");
            var components = target.GetComponents<Component>();
            foreach (var component in components)
            {
                var componentSerializedList = loadedSaveSerializedGameObject.ComponentSerializedList;
                foreach (var componentSerialized in componentSerializedList)
                {
                    if (component.GetType().Name == componentSerialized.ComponentType)
                    {
                        if (component is MonoBehaviour)
                        {
                            if (component is ISerializable serializable)
                            {
                                serializable.Deserialize(componentSerialized.JsonData);
                            }
                        }
                        else
                        {
                            component.Deserialize(componentSerialized.JsonData);
                        }
                    }
                }
            }

            for (int i = 0; i < target.childCount; i++)
            {
                TravesalDeserialize(target.GetChild(i), loadedSaveSerializedGameObject.Children[i]);
            }
        }
    }
}
#endif