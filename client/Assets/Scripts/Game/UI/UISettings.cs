using GameFramework.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏设置UI
/// </summary>
public class UISettings : UIBase
{
    // UI元素
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _soundSlider;
    [SerializeField] private Toggle _vibrationToggle;
    [SerializeField] private Button _closeButton;
    
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
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(OnCloseButtonClick);
        }
        
        // 设置滑动条事件
        if (_musicSlider != null)
        {
            _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            _musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        }
        
        if (_soundSlider != null)
        {
            _soundSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
            _soundSlider.value = PlayerPrefs.GetFloat("SoundVolume", 1.0f);
        }
        
        // 设置开关事件
        if (_vibrationToggle != null)
        {
            _vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
            _vibrationToggle.isOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
        }
    }
    
    /// <summary>
    /// 关闭按钮点击
    /// </summary>
    private void OnCloseButtonClick()
    {
        // 保存设置
        SaveSettings();
        
        // 关闭界面
        Close();
    }
    
    /// <summary>
    /// 音乐音量变化
    /// </summary>
    private void OnMusicVolumeChanged(float value)
    {
        // 应用音乐音量
        PlayerPrefs.SetFloat("MusicVolume", value);
    }
    
    /// <summary>
    /// 音效音量变化
    /// </summary>
    private void OnSoundVolumeChanged(float value)
    {
        // 应用音效音量
        PlayerPrefs.SetFloat("SoundVolume", value);
    }
    
    /// <summary>
    /// 振动开关变化
    /// </summary>
    private void OnVibrationChanged(bool isOn)
    {
        // 应用振动设置
        PlayerPrefs.SetInt("Vibration", isOn ? 1 : 0);
    }
    
    /// <summary>
    /// 保存设置
    /// </summary>
    private void SaveSettings()
    {
        PlayerPrefs.Save();
    }
    
    public override void OnClose()
    {
        // 移除按钮点击事件
        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClick);
        }
        
        // 移除滑动条事件
        if (_musicSlider != null)
        {
            _musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }
        
        if (_soundSlider != null)
        {
            _soundSlider.onValueChanged.RemoveListener(OnSoundVolumeChanged);
        }
        
        // 移除开关事件
        if (_vibrationToggle != null)
        {
            _vibrationToggle.onValueChanged.RemoveListener(OnVibrationChanged);
        }
        
        base.OnClose();
    }
} 