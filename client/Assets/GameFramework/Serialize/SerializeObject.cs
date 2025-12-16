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
        private const string TestJson =
            "{\"Children\":[{\"Children\":[],\"ComponentSerializes\":[{\"ComponentType\":\"Transform\",\"JsonData\":\"{\\\"Position\\\":{\\\"X\\\":1.0,\\\"Y\\\":1.0,\\\"Z\\\":1.0},\\\"LocalPosition\\\":{\\\"X\\\":1.0,\\\"Y\\\":1.0,\\\"Z\\\":1.0},\\\"Rotation\\\":{\\\"W\\\":1.0,\\\"X\\\":0.0,\\\"Y\\\":0.0,\\\"Z\\\":0.0},\\\"LocalRotation\\\":{\\\"W\\\":1.0,\\\"X\\\":0.0,\\\"Y\\\":0.0,\\\"Z\\\":0.0},\\\"LocalScale\\\":{\\\"X\\\":1.0,\\\"Y\\\":1.0,\\\"Z\\\":1.0}}\"}],\"MonoBehaviourSerializes\":[]}],\"ComponentSerializes\":[{\"ComponentType\":\"Transform\",\"JsonData\":\"{\\\"Position\\\":{\\\"X\\\":0.0,\\\"Y\\\":0.0,\\\"Z\\\":0.0},\\\"LocalPosition\\\":{\\\"X\\\":0.0,\\\"Y\\\":0.0,\\\"Z\\\":0.0},\\\"Rotation\\\":{\\\"W\\\":1.0,\\\"X\\\":0.0,\\\"Y\\\":0.0,\\\"Z\\\":0.0},\\\"LocalRotation\\\":{\\\"W\\\":1.0,\\\"X\\\":0.0,\\\"Y\\\":0.0,\\\"Z\\\":0.0},\\\"LocalScale\\\":{\\\"X\\\":1.0,\\\"Y\\\":1.0,\\\"Z\\\":1.0}}\"},{\"ComponentType\":\"Rigidbody2D\",\"JsonData\":\"{\\\"Position\\\":{\\\"X\\\":0.0,\\\"Y\\\":0.0},\\\"Rotation\\\":0.0,\\\"Velocity\\\":{\\\"X\\\":0.0,\\\"Y\\\":0.0},\\\"AngularDrag\\\":0.05,\\\"AngularVelocity\\\":0.0,\\\"GravityScale\\\":0.0,\\\"IsKinematic\\\":false,\\\"FreezeRotation\\\":true,\\\"Mass\\\":1.0,\\\"Constraints\\\":4}\"}],\"MonoBehaviourSerializes\":[{\"MonoBehaviourType\":\"Health\",\"JsonData\":\"{\\\"TotalHp\\\":0.0,\\\"CurrentHp\\\":0.0,\\\"DamageMultiplier\\\":1.0}\"},{\"MonoBehaviourType\":\"EnemyBase\",\"JsonData\":\"{\\\"IsDead\\\":true}\"}]}";

        private SaveLoadManager _saveLoadManager;
        
        private async void Start()
        {
            // TODO: 加入到序列化管理器
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

        public abstract SaveSerializedGameObject Save();

        public void Load(SaveSerializedComponents saveSerializedComponents)
        {
            TravesalDeserialize(transform, saveSerializedComponents);
        }

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
                if (component is MonoBehaviour) continue;
                componentsSerializedList.Add(new ComponentSerialized()
                {
                    ComponentType = component.GetType().Name,
                    JsonData = component.Serialize(),
                });
            }

            List<MonoBehaviourSerialized> monoBehaviourSerializedList = new List<MonoBehaviourSerialized>();
            foreach (var serializableMonoBehaviour in target.GetComponents<ISerializableMonoBehaviour>())
            {
                monoBehaviourSerializedList.Add(new MonoBehaviourSerialized
                {
                    MonoBehaviourType = serializableMonoBehaviour.GetType().Name,
                    JsonData = serializableMonoBehaviour.SerializeMonoBehaviour(),
                });
            }

            var saveItem = new SaveSerializedComponents
            {
                ComponentSerializeList = componentsSerializedList,
                MonoBehaviourSerializeList = monoBehaviourSerializedList,
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
        private void TravesalDeserialize(Transform target, SaveSerializedComponents loadedSaveSerializedComponents)
        {
            if (!target) return;
            Debug.Log($"[{GetType().Name}] TravesalDeserialize {target.name}");
            var components = target.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component is MonoBehaviour) continue;
                var componentSerializedList = loadedSaveSerializedComponents.ComponentSerializeList;
                foreach (var componentSerialized in componentSerializedList)
                {
                    if (component.GetType().Name == componentSerialized.ComponentType)
                    {
                        component.Deserialize(componentSerialized.JsonData);
                    }
                }
            }

            var serializableMonoBehaviours = target.GetComponents<ISerializableMonoBehaviour>();
            foreach (var serializableMonoBehaviour in serializableMonoBehaviours)
            {
                var monoBehaviourSerializedList = loadedSaveSerializedComponents.MonoBehaviourSerializeList;
                foreach (var monoBehaviourSerialized in monoBehaviourSerializedList)
                {
                    if (serializableMonoBehaviour.GetType().Name == monoBehaviourSerialized.MonoBehaviourType)
                    {
                        serializableMonoBehaviour.DeserializeMonoBehaviour(monoBehaviourSerialized.JsonData);
                    }
                }
            }

            for (int i = 0; i < target.childCount; i++)
            {
                TravesalDeserialize(target.GetChild(i), loadedSaveSerializedComponents.Children[i]);
            }
        }
    }
}