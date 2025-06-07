using System.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;

/// <summary>
/// 游戏逻辑基类 - 具体游戏逻辑的基类
/// </summary>
public abstract class GameLogic : MonoBehaviour
{
    // 游戏框架管理器
    protected GameFrameworkManager FrameworkManager => GameFrameworkManager.Instance;
    
    // 平台管理器
    protected PlatformManager PlatformManager => FrameworkManager.GetManager<PlatformManager>();
    
    // 资源管理器
    protected ResourceManager ResourceManager => FrameworkManager.GetManager<ResourceManager>();
    
    // 配置管理器
    protected ConfigManager ConfigManager => FrameworkManager.GetManager<ConfigManager>();
    
    // 事件管理器
    protected EventManager EventManager => FrameworkManager.GetManager<EventManager>();
    
    // UI管理器
    protected UIManager UIManager => FrameworkManager.GetManager<UIManager>();
    
    // HTTP管理器
    protected HttpManager HttpManager => FrameworkManager.GetManager<HttpManager>();
    
    // 游戏状态
    public enum GameState
    {
        None,       // 未初始化
        Loading,    // 加载中
        Ready,      // 准备开始
        Playing,    // 游戏中
        Paused,     // 暂停
        GameOver,   // 游戏结束
    }
    
    // 当前游戏状态
    public GameState CurrentState { get; protected set; } = GameState.None;
    
    // 游戏启动器
    protected GameLauncher GameLauncher => GameLauncher.Instance;
    
    // 游戏初始化时调用
    protected virtual void Awake()
    {
        // 子类实现
    }
    
    // 游戏开始时调用
    protected virtual void Start()
    {
        // 初始化游戏
        InitGame();
    }
    
    // 游戏帧更新
    protected virtual void Update()
    {
        // 根据游戏状态更新
        switch (CurrentState)
        {
            case GameState.Playing:
                OnGamePlaying();
                break;
            
            case GameState.Paused:
                OnGamePaused();
                break;
            
            case GameState.GameOver:
                OnGameOver();
                break;
        }
    }
    
    // 初始化游戏
    protected async void InitGame()
    {
        CurrentState = GameState.Loading;
        
        // 等待框架初始化完成
        while (FrameworkManager == null || !FrameworkManager.IsInitialized)
        {
            await Task.Delay(10);
        }
        
        // 游戏特定的初始化
        await OnGameInit();
        
        // 加载完成，进入准备状态
        CurrentState = GameState.Ready;
        
        // 调用游戏准备事件
        OnGameReady();
    }
    
    // 游戏初始化，子类实现
    protected abstract Task OnGameInit();
    
    // 游戏准备，子类实现
    protected abstract void OnGameReady();
    
    // 开始游戏
    public virtual void StartGame()
    {
        if (CurrentState != GameState.Ready)
        {
            Debug.LogWarning("[GameLogic] 游戏未准备好，无法开始");
            return;
        }
        
        CurrentState = GameState.Playing;
        OnGameStart();
    }
    
    // 游戏开始，子类实现
    protected abstract void OnGameStart();
    
    // 暂停游戏
    public virtual void PauseGame()
    {
        if (CurrentState != GameState.Playing)
        {
            Debug.LogWarning("[GameLogic] 游戏未进行中，无法暂停");
            return;
        }
        
        CurrentState = GameState.Paused;
        OnGamePause();
    }
    
    // 游戏暂停，子类实现
    protected abstract void OnGamePause();
    
    // 恢复游戏
    public virtual void ResumeGame()
    {
        if (CurrentState != GameState.Paused)
        {
            Debug.LogWarning("[GameLogic] 游戏未暂停，无法恢复");
            return;
        }
        
        CurrentState = GameState.Playing;
        OnGameResume();
    }
    
    // 游戏恢复，子类实现
    protected abstract void OnGameResume();
    
    // 结束游戏
    public virtual void EndGame()
    {
        if (CurrentState != GameState.Playing && CurrentState != GameState.Paused)
        {
            Debug.LogWarning("[GameLogic] 游戏未进行中或暂停，无法结束");
            return;
        }
        
        CurrentState = GameState.GameOver;
        OnGameEnd();
    }
    
    // 游戏结束，子类实现
    protected abstract void OnGameEnd();
    
    // 重新开始游戏
    public virtual void RestartGame()
    {
        // 结束当前游戏
        if (CurrentState == GameState.Playing || CurrentState == GameState.Paused)
        {
            EndGame();
        }
        
        // 重新初始化游戏
        InitGame();
    }
    
    // 游戏进行中更新，子类实现
    protected abstract void OnGamePlaying();
    
    // 游戏暂停中更新，子类实现
    protected abstract void OnGamePaused();
    
    // 游戏结束后更新，子类实现
    protected abstract void OnGameOver();
} 