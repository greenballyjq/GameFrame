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
        private readonly List<SerializeObject> _serializeObjectList = new List<SerializeObject>();
        private readonly List<SerializeObjectInAsset> _serializeObjectInAssetList = new List<SerializeObjectInAsset>();

        private readonly Dictionary<string, SerializeObjectInScene> _serializeObjectInSceneDictionary =
            new Dictionary<string, SerializeObjectInScene>();

        private readonly List<SaveSerializedGameObject> _saveSerializedGameObjectList =
            new List<SaveSerializedGameObject>();

        private readonly List<SaveSerializedGameObjectInAsset> _saveSerializedGameObjectInAssetList =
            new List<SaveSerializedGameObjectInAsset>();

        private readonly Dictionary<string, SaveSerializedGameObjectInScene>
            _saveSerializedGameObjectInSceneDictionary = new Dictionary<string, SaveSerializedGameObjectInScene>();

        public void Init()
        {
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Save();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                Load();
            }
        }

        private void Save()
        {
            foreach (var serializeObject in _serializeObjectList)
            {
                var saveSerializedGameObject = serializeObject.Save();
                if (saveSerializedGameObject is SaveSerializedGameObjectInScene saveSerializedGameObjectInScene)
                {
                    _saveSerializedGameObjectInSceneDictionary[saveSerializedGameObjectInScene.ObjectIdInScene] =
                        saveSerializedGameObjectInScene;
                }
                else if (saveSerializedGameObject is SaveSerializedGameObjectInAsset saveSerializedGameObjectInAsset)
                {
                    _saveSerializedGameObjectInAssetList.Add(saveSerializedGameObjectInAsset);
                }
            }

            Debug.Log($"[{GetType().Name}] _serializeObjectList : {_serializeObjectList.Count}");
            foreach (var item in _saveSerializedGameObjectInSceneDictionary)
            {
                Debug.Log($"[{GetType().Name}] item : {item.Key} : {item.Value.SaveSerializedComponents.ComponentSerializeList.Count}");
            }
            

            // 模拟保存序列化数据
            using (var fileStream = File.OpenWrite($"C:\\Users\\sanqianpan\\Desktop\\loadObjectInscene.json"))
            {
                string json = JsonConvert.SerializeObject(_saveSerializedGameObjectInSceneDictionary.Values);
                Debug.Log($"[{GetType().Name}] {json}");
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
                        serializeObjectInScene.Load(saveSerializedGameObjectInScene.SaveSerializedComponents);
                    }
                }
            }

            using (var streamReader2 = File.OpenText($"C:\\Users\\sanqianpan\\Desktop\\loadObjectInAsset.json"))
            {
                var json2 = streamReader2.ReadToEnd();
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
                    // TODO: 加载资源，并反序列化组件数据
                }
            }
        }

        public void Release()
        {
        }

        public void AddSerializeObject(SerializeObject serializeObject)
        {
            Debug.Log($"[{GetType().Name}] AddSerializeObject()");
            _serializeObjectList.Add(serializeObject);
            if (serializeObject is SerializeObjectInScene serializeObjectInScene)
                _serializeObjectInSceneDictionary[serializeObjectInScene.ObjectId] = serializeObjectInScene;
            else if (serializeObject is SerializeObjectInAsset serializeObjectInAsset)
                _serializeObjectInAssetList.Add(serializeObjectInAsset);
        }
    }
}