using GameFramework.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏主界面UI
/// </summary>
public class UIMainGame : UIBase
{
    // 游戏逻辑引用
    private GooseCatcherGame _gameLogic;
    
    // UI元素
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private Slider _timeSlider;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _restartButton;
    
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
        
        if (_pauseButton != null)
        {
            _pauseButton.onClick.AddListener(OnPauseButtonClick);
        }
        
        if (_resumeButton != null)
        {
            _resumeButton.onClick.AddListener(OnResumeButtonClick);
        }
        
        if (_restartButton != null)
        {
            _restartButton.onClick.AddListener(OnRestartButtonClick);
        }
        
        // 初始状态下，只显示开始按钮
        UpdateButtonState(true, false, false, false);
    }
    
    /// <summary>
    /// 设置游戏逻辑引用
    /// </summary>
    public void SetGameLogic(GooseCatcherGame gameLogic)
    {
        _gameLogic = gameLogic;
    }
    
    /// <summary>
    /// 更新UI显示
    /// </summary>
    public void UpdateUI(int score, float gameTime, float maxGameTime)
    {
        // 更新分数
        if (_scoreText != null)
        {
            _scoreText.text = $"分数: {score}";
        }
        
        // 更新时间
        if (_timeText != null)
        {
            int remainingTime = Mathf.Max(0, Mathf.FloorToInt(maxGameTime - gameTime));
            _timeText.text = $"时间: {remainingTime}s";
        }
        
        // 更新时间进度条
        if (_timeSlider != null)
        {
            _timeSlider.maxValue = maxGameTime;
            _timeSlider.value = maxGameTime - gameTime;
        }
        
        // 根据游戏状态更新按钮显示
        if (_gameLogic != null)
        {
            switch (_gameLogic.CurrentState)
            {
                case GameLogic.GameState.Ready:
                    UpdateButtonState(true, false, false, false);
                    break;
                
                case GameLogic.GameState.Playing:
                    UpdateButtonState(false, true, false, false);
                    break;
                
                case GameLogic.GameState.Paused:
                    UpdateButtonState(false, false, true, true);
                    break;
                
                case GameLogic.GameState.GameOver:
                    UpdateButtonState(false, false, false, true);
                    break;
            }
        }
    }
    
    /// <summary>
    /// 更新按钮状态
    /// </summary>
    private void UpdateButtonState(bool showStart, bool showPause, bool showResume, bool showRestart)
    {
        if (_startButton != null)
        {
            _startButton.gameObject.SetActive(showStart);
        }
        
        if (_pauseButton != null)
        {
            _pauseButton.gameObject.SetActive(showPause);
        }
        
        if (_resumeButton != null)
        {
            _resumeButton.gameObject.SetActive(showResume);
        }
        
        if (_restartButton != null)
        {
            _restartButton.gameObject.SetActive(showRestart);
        }
    }
    
    /// <summary>
    /// 开始按钮点击
    /// </summary>
    private void OnStartButtonClick()
    {
        if (_gameLogic != null)
        {
            _gameLogic.StartGame();
        }
    }
    
    /// <summary>
    /// 暂停按钮点击
    /// </summary>
    private void OnPauseButtonClick()
    {
        if (_gameLogic != null)
        {
            _gameLogic.PauseGame();
        }
    }
    
    /// <summary>
    /// 恢复按钮点击
    /// </summary>
    private void OnResumeButtonClick()
    {
        if (_gameLogic != null)
        {
            _gameLogic.ResumeGame();
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
        }
    }
    
    public override void OnClose()
    {
        // 移除按钮点击事件
        if (_startButton != null)
        {
            _startButton.onClick.RemoveListener(OnStartButtonClick);
        }
        
        if (_pauseButton != null)
        {
            _pauseButton.onClick.RemoveListener(OnPauseButtonClick);
        }
        
        if (_resumeButton != null)
        {
            _resumeButton.onClick.RemoveListener(OnResumeButtonClick);
        }
        
        if (_restartButton != null)
        {
            _restartButton.onClick.RemoveListener(OnRestartButtonClick);
        }
        
        base.OnClose();
    }
} 