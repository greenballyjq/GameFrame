using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;
using YooAsset;

/// <summary>
/// 配置管理器 - 基于Luban实现
/// </summary>
public class ConfigManager : IManager
{
    // 配置表容器
    private readonly Dictionary<Type, object> _tables = new Dictionary<Type, object>();
    
    // 资源管理器引用
    private ResourceManager _resourceManager;
    
    // 配置文件所在路径
    private const string ConfigAssetPath = "Assets/GameData/Config";
    
    public void Init()
    {
        Debug.Log("[ConfigManager] 初始化配置管理器");
        
        _resourceManager = GameFrameworkManager.Instance.GetManager<ResourceManager>();
    }

    public void Update()
    {
        // 不需要每帧更新
    }

    public void Release()
    {
        _tables.Clear();
    }
    
    /// <summary>
    /// 异步加载配置表
    /// </summary>
    public async Task LoadTablesAsync()
    {
        Debug.Log("[ConfigManager] 开始加载配置表");
        
        try
        {
            // 从资源系统加载配置表清单
            var configManifest = await _resourceManager.LoadAssetAsync<TextAsset>("ConfigManifest.json");
            if (configManifest == null)
            {
                Debug.LogError("[ConfigManager] 加载配置表清单失败");
                return;
            }
            
            // 解析清单
            var manifest = JsonUtility.FromJson<ConfigManifest>(configManifest.text);
            
            // 加载所有配置表
            foreach (var configInfo in manifest.configs)
            {
                await LoadConfigAsync(configInfo.name, configInfo.assetPath);
            }
            
            Debug.Log("[ConfigManager] 配置表加载完成");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ConfigManager] 加载配置表出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 加载单个配置表
    /// </summary>
    private async Task LoadConfigAsync(string configName, string assetPath)
    {
        try
        {
            var configAsset = await _resourceManager.LoadAssetAsync<TextAsset>(assetPath);
            if (configAsset == null)
            {
                Debug.LogError($"[ConfigManager] 加载配置表 {configName} 失败, 路径: {assetPath}");
                return;
            }
            
            // 根据不同的配置表类型进行加载
            // 注意：这里需要根据实际Luban生成的代码来修改
            switch (configName)
            {
                case "ItemConfig":
                    LoadTable<ItemConfig>(configAsset.text);
                    break;
                case "CharacterConfig":
                    LoadTable<CharacterConfig>(configAsset.text);
                    break;
                // 添加其他配置表...
                default:
                    Debug.LogWarning($"[ConfigManager] 未知的配置表类型: {configName}");
                    break;
            }
            
            Debug.Log($"[ConfigManager] 配置表 {configName} 加载成功");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ConfigManager] 加载配置表 {configName} 出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 加载表格
    /// </summary>
    private void LoadTable<T>(string jsonData) where T : class, new()
    {
        try
        {
            // 注意：这里需要根据实际Luban生成的代码来修改
            // 这里假设Luban生成的表是通过JsonUtility可直接反序列化的
            var table = JsonUtility.FromJson<T>(jsonData);
            _tables[typeof(T)] = table;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ConfigManager] 解析配置表 {typeof(T).Name} 出错: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 获取配置表
    /// </summary>
    public T GetTable<T>() where T : class
    {
        if (_tables.TryGetValue(typeof(T), out var table))
        {
            return table as T;
        }
        
        Debug.LogWarning($"[ConfigManager] 配置表 {typeof(T).Name} 不存在");
        return null;
    }
}

/// <summary>
/// 配置表清单
/// </summary>
[Serializable]
public class ConfigManifest
{
    public List<ConfigInfo> configs = new List<ConfigInfo>();
}

/// <summary>
/// 配置表信息
/// </summary>
[Serializable]
public class ConfigInfo
{
    public string name;
    public string assetPath;
}

/// <summary>
/// 示例物品配置表
/// </summary>
[Serializable]
public class ItemConfig
{
    public List<ItemData> items = new List<ItemData>();
    
    [Serializable]
    public class ItemData
    {
        public int id;
        public string name;
        public string icon;
        public int type;
        public string description;
    }
}

/// <summary>
/// 示例角色配置表
/// </summary>
[Serializable]
public class CharacterConfig
{
    public List<CharacterData> characters = new List<CharacterData>();
    
    [Serializable]
    public class CharacterData
    {
        public int id;
        public string name;
        public string model;
        public int hp;
        public int attack;
        public int defense;
    }
} 