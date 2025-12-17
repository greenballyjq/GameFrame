using System;
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.Manager;
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

        public abstract void Load(SaveSerializedComponents saveSerializedComponents);

        /// <summary>
        /// 回溯算法实现游戏物体和子物体的组件与脚本实例序列化
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected SaveSerializedComponents TravesalSerialize(Transform target)
        {
            if (!target) return null;
            var components = target.GetComponents<Component>();
            List<ComponentSerialized> componentsSerializedList = new List<ComponentSerialized>();
            foreach (var component in components)
            {
                // if (component is MonoBehaviour) continue;
                // componentsSerializedList.Add(new ComponentSerialized()
                // {
                //     ComponentType = component.GetType().Name,
                //     JsonData = component.Serialize(),
                // });
                // 组件是否为脚本组件
                if (component is MonoBehaviour)
                {
                    // 该脚本组件是否实现了ISerializable接口
                    if (component is ISerializable serializable)
                    {
                        componentsSerializedList.Add(new ComponentSerialized
                        {
                            ComponentType = component.GetType().Name,
                            JsonData = serializable.Serialize(),
                        });
                    }
                }
                else
                {
                    componentsSerializedList.Add(new ComponentSerialized
                    {
                        ComponentType = component.GetType().Name,
                        JsonData = component.Serialize()
                    });
                }
            }

            // List<MonoBehaviourSerialized> monoBehaviourSerializedList = new List<MonoBehaviourSerialized>();
            // foreach (var serializableMonoBehaviour in target.GetComponents<ISerializable>())
            // {
            //     monoBehaviourSerializedList.Add(new MonoBehaviourSerialized
            //     {
            //         MonoBehaviourType = serializableMonoBehaviour.GetType().Name,
            //         JsonData = serializableMonoBehaviour.Serialize(),
            //     });
            // }

            var saveItem = new SaveSerializedComponents
            {
                ComponentSerializeList = componentsSerializedList,
                // MonoBehaviourSerializeList = monoBehaviourSerializedList,
                Children = new SaveSerializedComponents[target.childCount]
            };

            for (int i = 0; i < target.childCount; i++)
            {
                saveItem.Children[i] = TravesalSerialize(target.GetChild(i));
            }

            return saveItem;
        }

        /// <summary>
        /// 回溯算法实现游戏物体和子物体的组件与脚本实例反序列化
        /// </summary>
        /// <param name="target"></param>
        /// <param name="loadedSaveSerializedComponents"></param>
        /// <returns></returns>
        protected void TravesalDeserialize(Transform target, SaveSerializedComponents loadedSaveSerializedComponents)
        {
            if (!target) return;
            Debug.Log($"[{GetType().Name}] TravesalDeserialize {target.name}");
            var components = target.GetComponents<Component>();
            foreach (var component in components)
            {
                // if (component is MonoBehaviour) continue;
                // var componentSerializedList = loadedSaveSerializedComponents.ComponentSerializeList;
                // foreach (var componentSerialized in componentSerializedList)
                // {
                //     if (component.GetType().Name == componentSerialized.ComponentType)
                //     {
                //         component.Deserialize(componentSerialized.JsonData);
                //     }
                // }
                
                var componentSerializedList = loadedSaveSerializedComponents.ComponentSerializeList;
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

            // var serializableMonoBehaviours = target.GetComponents<ISerializable>();
            // foreach (var serializableMonoBehaviour in serializableMonoBehaviours)
            // {
            //     var monoBehaviourSerializedList = loadedSaveSerializedComponents.MonoBehaviourSerializeList;
            //     foreach (var monoBehaviourSerialized in monoBehaviourSerializedList)
            //     {
            //         if (serializableMonoBehaviour.GetType().Name == monoBehaviourSerialized.MonoBehaviourType)
            //         {
            //             serializableMonoBehaviour.Deserialize(monoBehaviourSerialized.JsonData);
            //         }
            //     }
            // }

            for (int i = 0; i < target.childCount; i++)
            {
                TravesalDeserialize(target.GetChild(i), loadedSaveSerializedComponents.Children[i]);
            }
        }
    }
}