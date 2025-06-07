using GameFramework.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏主菜单UI
/// </summary>
public class UIMainMenu : UIBase
{
    // UI元素
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _creditsButton;
    [SerializeField] private Button _quitButton;
    
    public override void OnInit(object userData)
    {
        base.OnInit(userData);
        
        // 初始化UI元素
        InitUI();
    }
    
    /// <summary>
    /// 初始化UI元素
    /// </summary>
    private void InitUI()
    {
        // 设置按钮点击事件
        if (_startButton != null)
        {
            _startButton.onClick.AddListener(OnStartButtonClick);
        }
        
        if (_settingsButton != null)
        {
            _settingsButton.onClick.AddListener(OnSettingsButtonClick);
        }
        
        if (_creditsButton != null)
        {
            _creditsButton.onClick.AddListener(OnCreditsButtonClick);
        }
        
        if (_quitButton != null)
        {
            _quitButton.onClick.AddListener(OnQuitButtonClick);
        }
    }
    
    /// <summary>
    /// 开始按钮点击
    /// </summary>
    private void OnStartButtonClick()
    {
        // 加载游戏场景
        if (GameFrameworkManager.Instance != null)
        {
            var resourceManager = GameFrameworkManager.Instance.GetManager<ResourceManager>();
            if (resourceManager != null)
            {
                resourceManager.LoadSceneAsync("Assets/Scenes/GameMain.unity");
            }
        }
    }
    
    /// <summary>
    /// 设置按钮点击
    /// </summary>
    private void OnSettingsButtonClick()
    {
        // 显示设置界面
        if (GameFrameworkManager.Instance != null)
        {
            var uiManager = GameFrameworkManager.Instance.GetManager<UIManager>();
            if (uiManager != null)
            {
                uiManager.OpenUIAsync<UISettings>("UISettings", UIManager.UILayer.PopUp);
            }
        }
    }
    
    /// <summary>
    /// 制作人员按钮点击
    /// </summary>
    private void OnCreditsButtonClick()
    {
        // 显示制作人员界面
        if (GameFrameworkManager.Instance != null)
        {
            var uiManager = GameFrameworkManager.Instance.GetManager<UIManager>();
            if (uiManager != null)
            {
                uiManager.OpenUIAsync<UICredits>("UICredits", UIManager.UILayer.PopUp);
            }
        }
    }
    
    /// <summary>
    /// 退出按钮点击
    /// </summary>
    private void OnQuitButtonClick()
    {
        // 退出游戏
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    public override void OnClose()
    {
        // 移除按钮点击事件
        if (_startButton != null)
        {
            _startButton.onClick.RemoveListener(OnStartButtonClick);
        }
        
        if (_settingsButton != null)
        {
            _settingsButton.onClick.RemoveListener(OnSettingsButtonClick);
        }
        
        if (_creditsButton != null)
        {
            _creditsButton.onClick.RemoveListener(OnCreditsButtonClick);
        }
        
        if (_quitButton != null)
        {
            _quitButton.onClick.RemoveListener(OnQuitButtonClick);
        }
        
        base.OnClose();
    }
} 