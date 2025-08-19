using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFramework.Core;
using GooseCatcher;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI管理器 - 管理游戏中的UI界面
/// </summary>
public class UIManager : IManager
{
    // UI界面层级
    public enum UILayer
    {
        Background, // 背景层
        Normal,     // 普通层
        Fixed,      // 固定层
        PopUp,      // 弹窗层
        Guide,      // 引导层
        Tips,       // 提示层
        Loading,    // 加载层
        Game        // 游戏层
    }
    
    // UI根节点
    private GameObject _uiRoot;
    
    // 各层级的父节点
    private readonly Dictionary<UILayer, Transform> _layerParents = new Dictionary<UILayer, Transform>();
    
    // 所有已打开的界面
    private readonly Dictionary<string, UIBase> _openedUIs = new Dictionary<string, UIBase>();
    
    // 界面预制体缓存
    private readonly Dictionary<string, GameObject> _uiPrefabCache = new Dictionary<string, GameObject>();
    
    // 资源管理器引用
    private ResourceManager _resourceManager;
    
    public void Init()
    {
        Debug.Log("[UIManager] 初始化UI管理器");
        
        _resourceManager = GameFrameworkManager.Instance.GetManager<ResourceManager>();
        
        // 创建UI根节点
        CreateUIRoot();
    }

    public void Update()
    {
        // 更新所有已打开的界面
        foreach (var ui in _openedUIs.Values)
        {
            ui.OnUpdate();
        }
    }

    public void Release()
    {
        // 关闭所有界面
        CloseAllUI();
        
        // 清除缓存
        _uiPrefabCache.Clear();
        
        Debug.Log("[UIManager] 释放UI管理器");
    }
    
    /// <summary>
    /// 创建UI根节点
    /// </summary>
    private void CreateUIRoot()
    {
        // 创建UI根节点
        _uiRoot = new GameObject("UIRoot");
        GameObject.DontDestroyOnLoad(_uiRoot);
        
        // 添加Canvas和CanvasScaler组件
        var canvas = _uiRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        var scaler = _uiRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        ScreenAdapter screenAdapter = _uiRoot.AddComponent<ScreenAdapter>();

        _uiRoot.AddComponent<GraphicRaycaster>();
        
        // 创建各层级父节点
        foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
        {
            var layerObj = new GameObject(layer.ToString());
            layerObj.transform.SetParent(_uiRoot.transform, false);
            
            // 添加RectTransform并设置为全屏
            var rectTransform = layerObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            
            // 添加Canvas组件，用于控制层级顺序
            var layerCanvas = layerObj.AddComponent<Canvas>();
            layerCanvas.overrideSorting = true;
            layerCanvas.sortingOrder = 100 + (int)layer * 10;
            
            // 添加GraphicRaycaster组件
            layerObj.AddComponent<GraphicRaycaster>();
            
            if (layer == UILayer.Game) screenAdapter.SetRootRect(rectTransform);
            
            _layerParents.Add(layer, layerObj.transform);
        }
        
        Debug.Log("[UIManager] UI根节点创建完成");
    }
    
    /// <summary>
    /// 异步打开UI界面
    /// </summary>
    public async Task<T> OpenUIAsync<T>(string uiName, UILayer layer = UILayer.Normal, object userData = null) where T : UIBase
    {
        // 检查界面是否已经打开
        if (_openedUIs.TryGetValue(uiName, out var existingUI))
        {
            Debug.Log($"[UIManager] UI界面 {uiName} 已经打开");
            return existingUI as T;
        }
        
        // 从缓存获取或加载界面预制体
        GameObject uiPrefab = null;
        
        if (_uiPrefabCache.TryGetValue(uiName, out var cachedPrefab))
        {
            uiPrefab = cachedPrefab;
        }
        else
        {
            // 从资源管理器加载界面预制体
            uiPrefab = await _resourceManager.LoadAssetAsync<GameObject>($"Arts/Prefabs/UI/{uiName}.prefab");
            
            if (uiPrefab == null)
            {
                Debug.LogError($"[UIManager] 加载UI界面 {uiName} 失败");
                return null;
            }
            
            // 添加到缓存
            _uiPrefabCache.Add(uiName, uiPrefab);
        }
        
        // 获取层级父节点
        if (!_layerParents.TryGetValue(layer, out var layerParent))
        {
            Debug.LogError($"[UIManager] 无效的UI层级: {layer}");
            return null;
        }
        
        // 实例化界面
        var uiObj = GameObject.Instantiate(uiPrefab, layerParent);
        uiObj.name = uiName;
        
        // 获取UIBase组件
        var uiComponent = uiObj.GetComponent<T>();
        if (uiComponent == null)
        {
            Debug.LogError($"[UIManager] UI界面 {uiName} 没有 {typeof(T).Name} 组件");
            GameObject.Destroy(uiObj);
            return null;
        }
        
        // 初始化界面
        _openedUIs.Add(uiName, uiComponent);
        uiComponent.OnInit(userData);
        
        Debug.Log($"[UIManager] 打开UI界面: {uiName}");
        return uiComponent;
    }
    
    /// <summary>
    /// 关闭UI界面
    /// </summary>
    public void CloseUI(string uiName)
    {
        if (!_openedUIs.TryGetValue(uiName, out var ui))
        {
            Debug.LogWarning($"[UIManager] UI界面 {uiName} 未打开");
            return;
        }
        
        // 调用界面的关闭方法
        ui.OnClose();
        
        // 销毁界面对象
        GameObject.Destroy(ui.gameObject);
        
        // 从已打开界面中移除
        _openedUIs.Remove(uiName);
        
        Debug.Log($"[UIManager] 关闭UI界面: {uiName}");
    }
    
    /// <summary>
    /// 关闭所有UI界面
    /// </summary>
    public void CloseAllUI()
    {
        foreach (var ui in _openedUIs.Values)
        {
            ui.OnClose();
            GameObject.Destroy(ui.gameObject);
        }
        
        _openedUIs.Clear();
        
        Debug.Log("[UIManager] 关闭所有UI界面");
    }
    
    /// <summary>
    /// 获取已打开的UI界面
    /// </summary>
    public T GetUI<T>(string uiName) where T : UIBase
    {
        if (_openedUIs.TryGetValue(uiName, out var ui))
        {
            return ui as T;
        }
        
        return null;
    }
    
    /// <summary>
    /// 判断UI界面是否已打开
    /// </summary>
    public bool IsUIOpened(string uiName)
    {
        return _openedUIs.ContainsKey(uiName);
    }
    
    /// <summary>
    /// 预加载UI界面
    /// </summary>
    public async Task PreloadUIAsync(string uiName)
    {
        if (_uiPrefabCache.ContainsKey(uiName))
        {
            Debug.Log($"[UIManager] UI界面 {uiName} 已预加载");
            return;
        }
        
        // 从资源管理器加载界面预制体
        var uiPrefab = await _resourceManager.LoadAssetAsync<GameObject>($"Arts/Prefabs/UI/{uiName}.prefab");
        
        if (uiPrefab == null)
        {
            Debug.LogError($"[UIManager] 预加载UI界面 {uiName} 失败");
            return;
        }
        
        // 添加到缓存
        _uiPrefabCache.Add(uiName, uiPrefab);
        
        Debug.Log($"[UIManager] 预加载UI界面 {uiName} 成功");
    }
    
    /// <summary>
    /// 清除预加载的UI界面
    /// </summary>
    public void ClearPreloadedUI(string uiName)
    {
        if (_uiPrefabCache.ContainsKey(uiName) && !_openedUIs.ContainsKey(uiName))
        {
            _uiPrefabCache.Remove(uiName);
            Debug.Log($"[UIManager] 清除预加载的UI界面: {uiName}");
        }
    }
} 