using GameFramework.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏结束UI
/// </summary>
public class UIGameOver : UIBase
{
    // 游戏逻辑引用
    private GooseCatcherGame _gameLogic;
    
    // UI元素
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _shareButton;
    [SerializeField] private Button _homeButton;
    
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
        if (_restartButton != null)
        {
            _restartButton.onClick.AddListener(OnRestartButtonClick);
        }
        
        if (_shareButton != null)
        {
            _shareButton.onClick.AddListener(OnShareButtonClick);
        }
        
        if (_homeButton != null)
        {
            _homeButton.onClick.AddListener(OnHomeButtonClick);
        }
    }
    
    /// <summary>
    /// 设置游戏逻辑引用
    /// </summary>
    public void SetGameLogic(GooseCatcherGame gameLogic)
    {
        _gameLogic = gameLogic;
    }
    
    /// <summary>
    /// 设置分数
    /// </summary>
    public void SetScore(int score)
    {
        if (_scoreText != null)
        {
            _scoreText.text = $"最终分数: {score}";
        }
    }
    
    /// <summary>
    /// 重新开始按钮点击
    /// </summary>
    private void OnRestartButtonClick()
    {
        if (_gameLogic != null)
        {
            _gameLogic.RestartGame();
            Close();
        }
    }
    
    /// <summary>
    /// 分享按钮点击
    /// </summary>
    private void OnShareButtonClick()
    {
        if (_gameLogic != null && GameFrameworkManager.Instance != null)
        {
            var platformManager = GameFrameworkManager.Instance.GetManager<PlatformManager>();
            
            if (platformManager != null && platformManager.CurrentPlatform != null)
            {
                var shareInfo = new ShareInfo
                {
                    Title = "捉鹅游戏",
                    Desc = $"我在捉鹅游戏中获得了{_gameLogic.Score}分，快来挑战我吧！",
                    ImageUrl = "",  // 实际使用时需要设置分享图片URL
                    Link = ""       // 实际使用时需要设置分享链接
                };
                
                platformManager.CurrentPlatform.Share(shareInfo, 
                    () => Debug.Log("分享成功"), 
                    (error) => Debug.LogError($"分享失败: {error}"));
            }
        }
    }
    
    /// <summary>
    /// 返回主页按钮点击
    /// </summary>
    private void OnHomeButtonClick()
    {
        // 关闭当前界面
        Close();
        
        // 加载主菜单场景或显示主菜单界面
        if (GameFrameworkManager.Instance != null)
        {
            var resourceManager = GameFrameworkManager.Instance.GetManager<ResourceManager>();
            if (resourceManager != null)
            {
                resourceManager.LoadSceneAsync("Assets/Scenes/MainMenu.unity");
            }
        }
    }
    
    public override void OnClose()
    {
        // 移除按钮点击事件
        if (_restartButton != null)
        {
            _restartButton.onClick.RemoveListener(OnRestartButtonClick);
        }
        
        if (_shareButton != null)
        {
            _shareButton.onClick.RemoveListener(OnShareButtonClick);
        }
        
        if (_homeButton != null)
        {
            _homeButton.onClick.RemoveListener(OnHomeButtonClick);
        }
        
        base.OnClose();
    }
} 