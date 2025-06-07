using System.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;

/// <summary>
/// 捉鹅游戏实现类 - 作为示例实现
/// </summary>
public class GooseCatcherGame : GameLogic
{
    // 游戏分数
    private int _score = 0;
    public int Score => _score;
    
    // 游戏时间(秒)
    private float _gameTime = 0f;
    public float GameTime => _gameTime;
    
    // 游戏最大时间
    [SerializeField] private float _maxGameTime = 60f;
    public float MaxGameTime => _maxGameTime;
    
    // UI界面
    private UIMainGame _uiMainGame;
    private UIGameOver _uiGameOver;
    
    protected override void Awake()
    {
        base.Awake();
        
        Debug.Log("[GooseCatcherGame] 初始化捉鹅游戏");
    }
    
    protected override async Task OnGameInit()
    {
        Debug.Log("[GooseCatcherGame] 加载捉鹅游戏资源");
        
        // 重置游戏数据
        _score = 0;
        _gameTime = 0f;
        
        // 预加载UI
        await UIManager.PreloadUIAsync("UIMainGame");
        await UIManager.PreloadUIAsync("UIGameOver");
        
        // 初始化网络设置
        InitNetwork();
        
        // 加载游戏相关资源
        // 实际项目中可能需要加载更多资源
        
        Debug.Log("[GooseCatcherGame] 捉鹅游戏资源加载完成");
        
        return;
    }
    
    /// <summary>
    /// 初始化网络设置
    /// </summary>
    private void InitNetwork()
    {
        // 设置基础URL（示例）
        HttpManager.SetBaseUrl("https://api.example.com");
        
        // 设置默认请求头（示例）
        HttpManager.SetDefaultHeader("Authorization", "Bearer YOUR_TOKEN");
        
        Debug.Log("[GooseCatcherGame] 网络设置初始化完成");
    }
    
    protected override void OnGameReady()
    {
        Debug.Log("[GooseCatcherGame] 捉鹅游戏准备就绪");
        
        // 显示主界面
        ShowMainUI();
    }
    
    protected override void OnGameStart()
    {
        Debug.Log("[GooseCatcherGame] 捉鹅游戏开始");
        
        // 游戏开始逻辑
        // 比如生成鹅、初始化玩家等
        
        // 重置游戏数据
        _score = 0;
        _gameTime = 0f;
        
        // 更新UI
        UpdateUI();
    }
    
    protected override void OnGamePause()
    {
        Debug.Log("[GooseCatcherGame] 捉鹅游戏暂停");
        
        // 暂停游戏逻辑
        // 比如暂停鹅的移动、暂停计时器等
    }
    
    protected override void OnGameResume()
    {
        Debug.Log("[GooseCatcherGame] 捉鹅游戏恢复");
        
        // 恢复游戏逻辑
        // 比如恢复鹅的移动、恢复计时器等
    }
    
    protected override void OnGameEnd()
    {
        Debug.Log("[GooseCatcherGame] 捉鹅游戏结束");
        
        // 游戏结束逻辑
        // 比如清理场景、显示结算界面等
        
        // 上传游戏分数
        UploadGameScore();
        
        // 显示游戏结束界面
        ShowGameOverUI();
    }
    
    protected override void OnGamePlaying()
    {
        // 游戏进行中的逻辑
        
        // 更新游戏时间
        _gameTime += Time.deltaTime;
        
        // 检查游戏是否超时
        if (_gameTime >= _maxGameTime)
        {
            EndGame();
            return;
        }
        
        // 更新UI
        UpdateUI();
        
        // 其他游戏逻辑
        // 比如检测鹅的捕获、更新分数等
    }
    
    protected override void OnGamePaused()
    {
        // 游戏暂停中的逻辑
    }
    
    protected override void OnGameOver()
    {
        // 游戏结束后的逻辑
    }
    
    /// <summary>
    /// 增加分数
    /// </summary>
    public void AddScore(int score)
    {
        if (CurrentState != GameState.Playing)
            return;
        
        _score += score;
        Debug.Log($"[GooseCatcherGame] 增加分数: {score}, 当前分数: {_score}");
        
        // 更新UI
        UpdateUI();
    }
    
    /// <summary>
    /// 上传游戏分数
    /// </summary>
    private async void UploadGameScore()
    {
        // 创建分数数据对象
        ScoreData scoreData = new ScoreData
        {
            score = _score,
            gameTime = _gameTime,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        
        Debug.Log($"[GooseCatcherGame] 准备上传游戏分数: {_score}");
        
        // 发送POST请求上传分数
        HttpResponse response = await HttpManager.PostJsonAsync("/api/scores", scoreData);
        
        if (response.Success)
        {
            Debug.Log($"[GooseCatcherGame] 分数上传成功: {response.Data}");
            
            // 可以解析服务器返回的数据
            // ScoreResponse scoreResponse = response.ParseJson<ScoreResponse>();
        }
        else
        {
            Debug.LogError($"[GooseCatcherGame] 分数上传失败: {response.Error}, 状态码: {response.StatusCode}");
        }
    }
    
    /// <summary>
    /// 获取排行榜数据
    /// </summary>
    public async Task<LeaderboardData> GetLeaderboardAsync()
    {
        Debug.Log("[GooseCatcherGame] 获取排行榜数据");
        
        // 发送GET请求获取排行榜数据
        HttpResponse response = await HttpManager.GetAsync("/api/leaderboard");
        
        if (response.Success)
        {
            Debug.Log("[GooseCatcherGame] 获取排行榜数据成功");
            
            // 解析排行榜数据
            LeaderboardData leaderboard = response.ParseJson<LeaderboardData>();
            return leaderboard;
        }
        else
        {
            Debug.LogError($"[GooseCatcherGame] 获取排行榜数据失败: {response.Error}, 状态码: {response.StatusCode}");
            return null;
        }
    }
    
    /// <summary>
    /// 显示主界面
    /// </summary>
    private async void ShowMainUI()
    {
        _uiMainGame = await UIManager.OpenUIAsync<UIMainGame>("UIMainGame", UIManager.UILayer.Normal);
        
        if (_uiMainGame != null)
        {
            _uiMainGame.SetGameLogic(this);
            _uiMainGame.UpdateUI(Score, GameTime, MaxGameTime);
        }
    }
    
    /// <summary>
    /// 显示游戏结束界面
    /// </summary>
    private async void ShowGameOverUI()
    {
        _uiGameOver = await UIManager.OpenUIAsync<UIGameOver>("UIGameOver", UIManager.UILayer.PopUp);
        
        if (_uiGameOver != null)
        {
            _uiGameOver.SetGameLogic(this);
            _uiGameOver.SetScore(Score);
        }
    }
    
    /// <summary>
    /// 更新UI
    /// </summary>
    private void UpdateUI()
    {
        if (_uiMainGame != null)
        {
            _uiMainGame.UpdateUI(Score, GameTime, MaxGameTime);
        }
    }
}

/// <summary>
/// 分数数据
/// </summary>
[System.Serializable]
public class ScoreData
{
    public int score;
    public float gameTime;
    public string timestamp;
}

/// <summary>
/// 排行榜数据
/// </summary>
[System.Serializable]
public class LeaderboardData
{
    public LeaderboardEntry[] entries;
}

/// <summary>
/// 排行榜条目
/// </summary>
[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;
    public string timestamp;
} 