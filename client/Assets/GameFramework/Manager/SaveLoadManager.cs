#if FRAMEWORK_SERIALIZE
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameFramework.Core;
using GameFramework.Serialize;
using Newtonsoft.Json;
using UnityEngine;

namespace GameFramework.Manager
{
    /// <summary>
    /// 存读档管理器
    /// </summary>
    public class SaveLoadManager : IManager
    {
        /// <summary>
        /// 所有模式的序列化脚本列表，包括场景模式和资源模式
        /// </summary>
        private readonly List<SerializeObject> _serializeObjectList = new List<SerializeObject>();
        
        /// <summary>
        /// 资源模式的序列化脚本列表
        /// </summary>
        private readonly List<SerializeObjectInAsset> _serializeObjectInAssetList = new List<SerializeObjectInAsset>();

        /// <summary>
        /// 场景模式的序列化脚本字典
        /// ObjectId : SerializeObjectInScene
        /// </summary>
        private readonly Dictionary<string, SerializeObjectInScene> _serializeObjectInSceneDictionary =
            new Dictionary<string, SerializeObjectInScene>();

        /// <summary>
        /// 所有模式的已序列化对象列表
        /// </summary>
        private readonly List<SaveSerializedGameObject> _saveSerializedGameObjectList =
            new List<SaveSerializedGameObject>();

        /// <summary>
        /// 资源模式的已序列化对象列表
        /// </summary>
        private readonly List<SaveSerializedGameObjectInAsset> _saveSerializedGameObjectInAssetList =
            new List<SaveSerializedGameObjectInAsset>();

        /// <summary>
        /// 场景模式的已序列化对象列表
        /// </summary>
        private readonly List<SaveSerializedGameObjectInScene>
            _saveSerializedGameObjectInSceneList = new List<SaveSerializedGameObjectInScene>();

        /// <summary>
        /// 资源模式读取的对象父对象字典
        /// ObjectId : Transform
        /// </summary>
        private readonly Dictionary<string, Transform> _serializeObjectParentDictionary =
            new Dictionary<string, Transform>();

        public void Init()
        {
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                ClearSerializeData();
            }
        }
        
        public void Release()
        {
        }

        private void ClearSerializeData()
        {
            File.WriteAllText("C:\\Users\\sanqianpan\\Desktop\\loadObjectInscene.json", string.Empty);
            File.WriteAllText("C:\\Users\\sanqianpan\\Desktop\\loadObjectInAsset.json", string.Empty);
        }

        private void Save()
        {
            foreach (var serializeObject in _serializeObjectList)
            {
                var saveSerializedGameObject = serializeObject.Save();
                if (saveSerializedGameObject is SaveSerializedGameObjectInScene saveSerializedGameObjectInScene)
                {
                    // _saveSerializedGameObjectInSceneDictionary[saveSerializedGameObjectInScene.ObjectIdInScene] =
                    // saveSerializedGameObjectInScene;
                    _saveSerializedGameObjectInSceneList.Add(saveSerializedGameObjectInScene);
                }
                else if (saveSerializedGameObject is SaveSerializedGameObjectInAsset saveSerializedGameObjectInAsset)
                {
                    _saveSerializedGameObjectInAssetList.Add(saveSerializedGameObjectInAsset);
                }
            }

            using (var fileStream = File.OpenWrite($"C:\\Users\\sanqianpan\\Desktop\\loadObjectInscene.json"))
            {
                string json = JsonConvert.SerializeObject(_saveSerializedGameObjectInSceneList);
                var bytes = Encoding.UTF8.GetBytes(json);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Close();
            }
            
            using (var fileStream = File.OpenWrite($"C:\\Users\\sanqianpan\\Desktop\\loadObjectInAsset.json"))
            {
                string json = JsonConvert.SerializeObject(_saveSerializedGameObjectInAssetList);
                var bytes = Encoding.UTF8.GetBytes(json);
                fileStream.Write(bytes, 0, bytes.Length);
                fileStream.Close();
            }
        }

        private void Load()
        {
            using (var streamReader1 = File.OpenText($"C:\\Users\\sanqianpan\\Desktop\\loadObjectInscene.json"))
            {
                var json1 = streamReader1.ReadToEnd();
                if (string.IsNullOrEmpty(json1))
                {
                    Debug.LogError($"[{GetType().Name}] Load object in scene failed");
                    return;
                }
            
                var saveSerializedGameObjectInSceneList =
                    JsonConvert.DeserializeObject<List<SaveSerializedGameObjectInScene>>(json1);
                Debug.Log(
                    $"[{GetType().Name}] Load object in scene complete {saveSerializedGameObjectInSceneList.Count}");
                foreach (var saveSerializedGameObjectInScene in saveSerializedGameObjectInSceneList)
                {
                    if (_serializeObjectInSceneDictionary.TryGetValue(saveSerializedGameObjectInScene.ObjectIdInScene,
                            out var serializeObjectInScene))
                    {
                        serializeObjectInScene.Load(saveSerializedGameObjectInScene);
                    }
                }
            }
            
            using (var streamReader2 = File.OpenText($"C:\\Users\\sanqianpan\\Desktop\\loadObjectInAsset.json"))
            {
                var json2 = streamReader2.ReadToEnd();
                Debug.Log($"[{GetType().Name}] load from asset: {json2}");
                if (string.IsNullOrEmpty(json2))
                {
                    Debug.LogError($"[{GetType().Name}] Load object in asset failed");
                    return;
                }
            
                var saveSerializedGameObjectInAssetList =
                    JsonConvert.DeserializeObject<List<SaveSerializedGameObjectInAsset>>(json2);
                Debug.Log(
                    $"[{GetType().Name}] Load object in asset complete {saveSerializedGameObjectInAssetList.Count}");
                foreach (var saveSerializedGameObjectInAsset in saveSerializedGameObjectInAssetList)
                {
                    Debug.Log($"[{GetType().Name}] path: {saveSerializedGameObjectInAsset.AssetPath}");
                    // TODO: 加载资源，并反序列化组件数据     
                    // Transform prefab = null;
                    // var go = GameObject.Instantiate(prefab);
                    // go.GetComponent<SerializeObject>().Load(saveSerializedGameObjectInAsset.SaveSerializedComponents);
                    var gameObject = Resources.Load<GameObject>(saveSerializedGameObjectInAsset.AssetPath);
                    if (gameObject == null)
                    {
                        Debug.Log($"[{GetType().Name}] Load object in null");
                        return;
                    }
                    var prefab = gameObject.transform;
                    Transform parent = null;
                    if (_serializeObjectParentDictionary.TryGetValue(saveSerializedGameObjectInAsset.ParentObjectId, out parent))
                    {
                        
                    }
                    var transform = GameObject.Instantiate(prefab, parent);
                    transform.GetComponent<SerializeObject>().Load(saveSerializedGameObjectInAsset);
                }
            }
        }
        
        public void AddSerializeObject(SerializeObject serializeObject)
        {
            _serializeObjectList.Add(serializeObject);
            if (serializeObject is SerializeObjectInScene serializeObjectInScene)
                _serializeObjectInSceneDictionary[serializeObjectInScene.ObjectId] = serializeObjectInScene;
            else if (serializeObject is SerializeObjectInAsset serializeObjectInAsset)
                _serializeObjectInAssetList.Add(serializeObjectInAsset);
        }

        public void AddSerializeObjectParent(string objectId, Transform parent)
        {
            _serializeObjectParentDictionary[objectId] = parent;
        }

        public void RemoveSerializeObjectParent(string objectId)
        {
            _serializeObjectParentDictionary.Remove(objectId);
        }

        public void RemoveSerializeObject(SerializeObject serializeObject)
        {
            _serializeObjectList.Remove(serializeObject);
            if (serializeObject is SerializeObjectInScene serializeObjectInScene)
                _serializeObjectInSceneDictionary.Remove(serializeObjectInScene.ObjectId);
            else if (serializeObject is SerializeObjectInAsset serializeObjectInAsset)
                _serializeObjectInAssetList.Remove(serializeObjectInAsset);
        }
    }
}
#endif